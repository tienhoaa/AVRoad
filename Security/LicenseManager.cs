using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace AVRoad.Security
{
    /// <summary>
    /// Quản lý kiểm tra và bảo vệ hệ thống AVRoad 2026 theo cơ chế Tàng hình (Stealth Mode).
    /// Mặc định người dùng sử dụng bình thường 100% không cần thao tác bản quyền hay nhập key.
    /// Hệ thống chạy ngầm kiểm tra danh sách thu hồi (blacklist) từ GitHub:
    /// Nếu thiết bị nằm trong blacklist, hiển thị thông báo nhẹ nhàng:
    /// "Phần mềm đã cũ, bạn vui lòng cập nhật phiên bản mới để tiếp tục sử dụng."
    /// </summary>
    public static class LicenseManager
    {
        public static readonly string LicenseDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AVRoad");
        public static readonly string LicenseFilePath = Path.Combine(LicenseDirectory, "AV_License.avlic");
        public static readonly string OutdatedFlagPath = Path.Combine(LicenseDirectory, ".outdated");

        /// <summary>
        /// URL GitHub kiểm tra danh sách HWID thu hồi.
        /// </summary>
        public static string RevocationCheckUrl = "https://raw.githubusercontent.com/tienhoaa/AV_Releases/main/revoked_hwid.json";

        private static LicenseValidationResult _cachedResult;
        private static DateTime _lastValidatedTime = DateTime.MinValue;
        private static bool _isRevoked = false;
        private static string _revocationReason = string.Empty;
        private static int _backgroundCheckStarted = 0;
        private static readonly object _syncLock = new object();
        private static SynchronizationContext _uiContext;

        /// <summary>
        /// Kiểm tra xem máy hiện tại có bị thu hồi / khóa ngầm hay không.
        /// </summary>
        public static bool IsRevoked
        {
            get
            {
                if (_isRevoked) return true;
                if (File.Exists(OutdatedFlagPath)) return true;
                return false;
            }
        }

        /// <summary>
        /// Kiểm tra hệ thống đã có bản quyền hợp lệ hay chưa.
        /// </summary>
        public static bool IsLicensed
        {
            get
            {
                return IsActiveLicenseValid();
            }
        }

        /// <summary>
        /// Kiểm tra xem bản quyền máy hiện tại có hợp lệ, đúng HWID, đúng sản phẩm AVRoad và còn hạn hay không.
        /// </summary>
        public static bool IsActiveLicenseValid()
        {
            LicenseValidationResult res = ValidateLicense(false);
            return res != null && res.IsValid;
        }

        /// <summary>
        /// Khởi tạo và kích hoạt kiểm tra thu hồi ngầm (chỉ chạy 1 lần khi CAD khởi động).
        /// </summary>
        public static void Initialize()
        {
            try
            {
                _uiContext = SynchronizationContext.Current;
                EnsureDirectoryExists();
                if (File.Exists(OutdatedFlagPath))
                {
                    _isRevoked = true;
                }
                StartBackgroundRevocationCheck();
            }
            catch { }
        }

        public static void EnsureDirectoryExists()
        {
            try
            {
                if (!Directory.Exists(LicenseDirectory))
                {
                    Directory.CreateDirectory(LicenseDirectory);
                }
            }
            catch { }
        }

        /// <summary>
        /// Xuất file yêu cầu cấp bản quyền (.avreq) nếu cần hỗ trợ thủ công.
        /// </summary>
        public static void ExportRequest(string filePath, string customerName, string company, string phone, string email)
        {
            LicenseRequest req = new LicenseRequest();
            req.AppName = "AVRoad 2026 (TDT 9.1)";
            req.Product = "AVRoad 2026 (TDT 9.1)";
            req.SoftwareCode = "AVROAD_TDT91";
            req.RequestVersion = "1.1";
            req.CustomerName = (customerName != null) ? customerName.Trim() : string.Empty;
            req.Company = !string.IsNullOrEmpty(company) ? company.Trim() : (Environment.UserDomainName ?? string.Empty);
            req.Phone = (phone != null) ? phone.Trim() : string.Empty;
            req.Email = (email != null) ? email.Trim() : string.Empty;
            req.MachineName = HardwareIdentifier.GetMachineName();
            req.OsVersion = HardwareIdentifier.GetOsVersion();
            req.HardwareId = HardwareIdentifier.GetHardwareId();
            req.RequestDate = DateTime.UtcNow;

            string json = req.ToJson();

            string dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(filePath, json, Encoding.UTF8);
        }

        /// <summary>
        /// Nhập file bản quyền (.avlic).
        /// </summary>
        public static bool ImportLicense(string filePath, out string message)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    message = "Tập tin không tồn tại.";
                    return false;
                }

                string fileContent = File.ReadAllText(filePath, Encoding.UTF8);
                if (string.IsNullOrEmpty(fileContent) || string.IsNullOrEmpty(fileContent.Trim()))
                {
                    message = "Nội dung tập tin trống.";
                    return false;
                }

                string payload = SimpleJson.ExtractRawProperty(fileContent, "payload");
                string signature = SimpleJson.ExtractRawProperty(fileContent, "signature");

                if (string.IsNullOrEmpty(payload) || string.IsNullOrEmpty(signature))
                {
                    message = "Tập tin không chứa đầy đủ payload hoặc chữ ký số.";
                    return false;
                }

                bool signatureValid = CryptoHelper.VerifySignature(payload, signature);
                if (!signatureValid)
                {
                    message = "Chữ ký số RSA không hợp lệ.";
                    return false;
                }

                Dictionary<string, object> payloadDict = SimpleJson.DeserializeObject(payload);
                if (payloadDict == null)
                {
                    message = "Không thể đọc dữ liệu bản quyền trong payload.";
                    return false;
                }

                LicenseData licData = LicenseData.FromDictionary(payloadDict);
                if (licData == null)
                {
                    message = "Dữ liệu bản quyền không hợp lệ.";
                    return false;
                }

                // Kiểm tra đúng định danh phần mềm AVRoad 2026 / TDT 9.1 hoặc Trọn gói ALL
                if (!string.IsNullOrEmpty(licData.SoftwareCode))
                {
                    bool isAvRoad = string.Equals(licData.SoftwareCode, "AVROAD_TDT91", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(licData.SoftwareCode, "TDT_ROAD", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(licData.SoftwareCode, "ALL", StringComparison.OrdinalIgnoreCase);

                    if (!isAvRoad)
                    {
                        message = string.Format("Tập tin bản quyền này được cấp cho '{0}' ({1}), không tương thích với AVRoad 2026 (TDT 9.1).", licData.Product, licData.SoftwareCode);
                        return false;
                    }
                }
                else if (!string.IsNullOrEmpty(licData.Product) && licData.Product.IndexOf("Civil", StringComparison.OrdinalIgnoreCase) >= 0 && licData.Product.IndexOf("Road", StringComparison.OrdinalIgnoreCase) < 0 && licData.Product.IndexOf("TDT", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    message = "Tập tin bản quyền này dành cho AV_Civil, không áp dụng cho AVRoad 2026 (TDT 9.1).";
                    return false;
                }

                string currentHwid = HardwareIdentifier.GetHardwareId();
                if (!string.Equals(licData.HardwareId, "*", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(licData.HardwareId, currentHwid, StringComparison.OrdinalIgnoreCase))
                {
                    message = string.Format("Tập tin này được cấp cho thiết bị khác ({0}). Mã máy này là {1}.", licData.HardwareId, currentHwid);
                    return false;
                }

                EnsureDirectoryExists();
                File.WriteAllText(LicenseFilePath, fileContent, Encoding.UTF8);

                lock (_syncLock)
                {
                    _cachedResult = null;
                    _isRevoked = false;
                }

                try
                {
                    if (File.Exists(OutdatedFlagPath))
                        File.Delete(OutdatedFlagPath);
                }
                catch { }

                LicenseValidationResult check = ValidateLicense(true);
                if (check.IsValid)
                {
                    message = string.Format("Kích hoạt bản quyền thành công cho: {0}!", licData.CustomerName);
                    return true;
                }
                else
                {
                    message = check.Message;
                    return false;
                }
            }
            catch (Exception ex)
            {
                message = "Lỗi kích hoạt: " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Xác thực bản quyền hệ thống. Hoạt động 100% offline.
        /// </summary>
        public static LicenseValidationResult ValidateLicense(bool forceRefresh)
        {
            if (!forceRefresh && _cachedResult != null && (DateTime.UtcNow - _lastValidatedTime).TotalSeconds < 30)
            {
                return _cachedResult;
            }

            lock (_syncLock)
            {
                if (!forceRefresh && _cachedResult != null && (DateTime.UtcNow - _lastValidatedTime).TotalSeconds < 30)
                {
                    return _cachedResult;
                }

                _cachedResult = PerformValidation();
                _lastValidatedTime = DateTime.UtcNow;
                return _cachedResult;
            }
        }

        private static LicenseValidationResult PerformValidation()
        {
            // 1. Nếu đã bị thu hồi / đánh dấu outdated
            if (IsRevoked)
            {
                return LicenseValidationResult.Outdated(LicenseStatus.Revoked, _revocationReason);
            }

            // 2. Chốt chặn bắt buộc: Phải có file bản quyền .avlic
            if (!File.Exists(LicenseFilePath))
            {
                return LicenseValidationResult.NotActivated("Phần mềm chưa được kích hoạt bản quyền. Vui lòng đăng ký mã máy.");
            }

            try
            {
                string json = File.ReadAllText(LicenseFilePath, Encoding.UTF8);
                if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(json.Trim()))
                {
                    return LicenseValidationResult.Fail(LicenseStatus.NotFound, "Tập tin bản quyền rỗng.");
                }

                string payload = SimpleJson.ExtractRawProperty(json, "payload");
                string signature = SimpleJson.ExtractRawProperty(json, "signature");

                if (string.IsNullOrEmpty(payload) || string.IsNullOrEmpty(signature))
                {
                    return LicenseValidationResult.Fail(LicenseStatus.InvalidSignature, "Tập tin không chứa đầy đủ payload hoặc chữ ký số.");
                }

                if (!CryptoHelper.VerifySignature(payload, signature))
                {
                    return LicenseValidationResult.Fail(LicenseStatus.InvalidSignature, "Chữ ký số RSA không hợp lệ hoặc tập tin đã bị chỉnh sửa.");
                }

                Dictionary<string, object> dict = SimpleJson.DeserializeObject(payload);
                if (dict == null)
                {
                    return LicenseValidationResult.Fail(LicenseStatus.Error, "Không thể đọc dữ liệu bản quyền trong payload.");
                }

                LicenseData data = LicenseData.FromDictionary(dict);
                if (data == null)
                {
                    return LicenseValidationResult.Fail(LicenseStatus.Error, "Dữ liệu bản quyền không hợp lệ.");
                }

                // 3. Kiểm tra định danh phần mềm (AVRoad 2026 / TDT 9.1 hoặc Trọn gói ALL)
                if (!string.IsNullOrEmpty(data.SoftwareCode))
                {
                    bool isAvRoad = string.Equals(data.SoftwareCode, "AVROAD_TDT91", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(data.SoftwareCode, "TDT_ROAD", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(data.SoftwareCode, "ALL", StringComparison.OrdinalIgnoreCase);

                    if (!isAvRoad)
                    {
                        return LicenseValidationResult.Fail(LicenseStatus.Error, string.Format("Bản quyền này được cấp cho '{0}' ({1}), không tương thích với AVRoad 2026 (TDT 9.1).", data.Product, data.SoftwareCode));
                    }
                }
                else if (!string.IsNullOrEmpty(data.Product))
                {
                    if (data.Product.IndexOf("Civil", StringComparison.OrdinalIgnoreCase) >= 0 &&
                        data.Product.IndexOf("Road", StringComparison.OrdinalIgnoreCase) < 0 &&
                        data.Product.IndexOf("TDT", StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        return LicenseValidationResult.Fail(LicenseStatus.Error, "Bản quyền này dành cho AV_Civil, không áp dụng cho AVRoad 2026 (TDT 9.1).");
                    }
                }

                // 4. Kiểm tra đúng Mã máy (HWID)
                string currentHwid = HardwareIdentifier.GetHardwareId();
                if (!string.Equals(data.HardwareId, "*", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(data.HardwareId, currentHwid, StringComparison.OrdinalIgnoreCase))
                {
                    return LicenseValidationResult.Fail(LicenseStatus.InvalidHardware, string.Format("Bản quyền này được cấp cho thiết bị khác ({0}). Mã máy này là {1}.", data.HardwareId, currentHwid));
                }

                // 5. Kiểm tra thời hạn bản quyền
                if (data.ExpiryDate.HasValue && !data.IsPerpetual && DateTime.Now > data.ExpiryDate.Value)
                {
                    return LicenseValidationResult.Fail(LicenseStatus.Expired, string.Format("Bản quyền đã hết hạn vào ngày {0:dd/MM/yyyy}.", data.ExpiryDate.Value));
                }

                int days = data.ExpiryDate.HasValue ? (int)(data.ExpiryDate.Value - DateTime.Now).TotalDays : 9999;
                return LicenseValidationResult.Success(data, days);
            }
            catch (Exception ex)
            {
                return LicenseValidationResult.Fail(LicenseStatus.Error, "Lỗi kiểm tra bản quyền: " + ex.Message);
            }
        }

        public static bool IsModuleAuthorized(string moduleName)
        {
            if (IsRevoked) return false;
            return IsActiveLicenseValid();
        }

        public static bool IsCommandAuthorized(string commandName)
        {
            if (string.IsNullOrEmpty(commandName)) return true;
            string cmd = commandName.Trim().ToUpperInvariant();

            // Luôn cho phép các lệnh quản trị giao diện, cập nhật, bản quyền, trợ giúp, HWID
            if (cmd.StartsWith("AV_HWID") ||
                cmd.StartsWith("AV_LICENSE") || cmd.StartsWith("AV_BANQUYEN") ||
                cmd.StartsWith("AV_ABOUT") || cmd.StartsWith("AV_HELP") ||
                cmd.StartsWith("AVROAD") ||
                cmd.StartsWith("AV_RELOADUI") || cmd.StartsWith("RELOADGUI") ||
                cmd.StartsWith("KHOIDONG") || cmd.StartsWith("AV_UPDATE"))
            {
                return true;
            }

            if (IsRevoked) return false;
            if (!IsActiveLicenseValid()) return false;

            return true;
        }

        public static bool EnsureAuthorized(string moduleName)
        {
            if (IsRevoked)
            {
                NotifyOutdated();
                return false;
            }

            if (!IsActiveLicenseValid())
            {
                LicenseDialog.ShowActivationDialog();
                return false;
            }

            return true;
        }

        public static void NotifyOutdated()
        {
            try
            {
                if (_uiContext != null)
                {
                    _uiContext.Post(delegate { StealthNoticeToast.ShowNotice(); }, null);
                }
                else
                {
                    StealthNoticeToast.ShowNotice();
                }
            }
            catch
            {
                try { StealthNoticeToast.ShowNotice(); } catch { }
            }
        }

        /// <summary>
        /// Chạy ngầm kiểm tra thu hồi bản quyền qua GitHub.
        /// Sử dụng ThreadPool + HttpWebRequest (tương thích .NET 2.0+), hoàn toàn async, không chặn luồng CAD.
        /// </summary>
        public static void StartBackgroundRevocationCheck()
        {
            if (Interlocked.Exchange(ref _backgroundCheckStarted, 1) == 1)
                return;

            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    // Chờ 3 giây sau khi AutoCAD mở để tránh ảnh hưởng I/O khởi động
                    Thread.Sleep(3000);

                    string currentHwid = HardwareIdentifier.GetHardwareId();
                    string url = RevocationCheckUrl;
                    if (url.IndexOf('?') < 0)
                        url += "?t=" + DateTime.UtcNow.Ticks;
                    else
                        url += "&t=" + DateTime.UtcNow.Ticks;

                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                    req.Method = "GET";
                    req.Timeout = 5000;
                    req.UserAgent = "AVRoad-Client/1.0";
                    req.KeepAlive = false;

                    using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                    {
                        if (resp.StatusCode == HttpStatusCode.OK)
                        {
                            using (Stream stream = resp.GetResponseStream())
                            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                            {
                                string content = reader.ReadToEnd();
                                if (!string.IsNullOrEmpty(content))
                                {
                                    bool isFound = CheckIfHwidRevoked(content, currentHwid);
                                    if (isFound)
                                    {
                                        lock (_syncLock)
                                        {
                                            _isRevoked = true;
                                            _revocationReason = "Thiết bị nằm trong danh sách cần cập nhật.";
                                            _cachedResult = null;
                                        }

                                        try
                                        {
                                            EnsureDirectoryExists();
                                            File.WriteAllText(OutdatedFlagPath, DateTime.UtcNow.ToString("o"));
                                        }
                                        catch { }

                                        NotifyOutdated();
                                    }
                                    else
                                    {
                                        lock (_syncLock)
                                        {
                                            _isRevoked = false;
                                            _cachedResult = null;
                                        }

                                        try
                                        {
                                            if (File.Exists(OutdatedFlagPath))
                                            {
                                                File.Delete(OutdatedFlagPath);
                                            }
                                        }
                                        catch { }
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // Lỗi mạng hoặc offline: Bỏ qua trong im lặng, đảm bảo người dùng bình thường tiếp tục làm việc
                }
            });
        }

        private static bool CheckIfHwidRevoked(string jsonOrText, string hwid)
        {
            if (string.IsNullOrEmpty(jsonOrText) || string.IsNullOrEmpty(hwid))
                return false;

            try
            {
                object parsed = SimpleJson.Deserialize(jsonOrText);
                if (parsed is List<object>)
                {
                    List<object> list = (List<object>)parsed;
                    foreach (object item in list)
                    {
                        if (item is string && string.Equals((string)item, hwid, StringComparison.OrdinalIgnoreCase))
                            return true;

                        if (item is Dictionary<string, object>)
                        {
                            Dictionary<string, object> d = (Dictionary<string, object>)item;
                            if (d.ContainsKey("hwid") && string.Equals(Convert.ToString(d["hwid"]), hwid, StringComparison.OrdinalIgnoreCase))
                                return true;
                        }
                    }
                }
                else if (parsed is Dictionary<string, object>)
                {
                    Dictionary<string, object> dict = (Dictionary<string, object>)parsed;
                    if (dict.ContainsKey("revoked_hwids"))
                    {
                        object rev = dict["revoked_hwids"];
                        if (rev is List<object>)
                        {
                            foreach (object item in (List<object>)rev)
                            {
                                if (item is string && string.Equals((string)item, hwid, StringComparison.OrdinalIgnoreCase))
                                    return true;
                                if (item is Dictionary<string, object>)
                                {
                                    Dictionary<string, object> d = (Dictionary<string, object>)item;
                                    if (d.ContainsKey("hwid") && string.Equals(Convert.ToString(d["hwid"]), hwid, StringComparison.OrdinalIgnoreCase))
                                        return true;
                                }
                            }
                        }
                        else if (rev is Dictionary<string, object>)
                        {
                            Dictionary<string, object> d = (Dictionary<string, object>)rev;
                            if (d.ContainsKey(hwid)) return true;
                        }
                    }

                    if (dict.ContainsKey("revoked") && dict["revoked"] is List<object>)
                    {
                        foreach (object item in (List<object>)dict["revoked"])
                        {
                            if (item is string && string.Equals((string)item, hwid, StringComparison.OrdinalIgnoreCase))
                                return true;
                        }
                    }
                }
            }
            catch { }

            // Fallback kiểm tra chuỗi trực tiếp
            if (jsonOrText.IndexOf(hwid, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            return false;
        }
    }
}
