using System;
using System.Collections.Generic;
using System.Globalization;

namespace AVRoad.Security
{
    /// <summary>
    /// Thông tin yêu cầu cấp bản quyền gửi từ Client (.avreq).
    /// </summary>
    public class LicenseRequest
    {
        private string _appName;
        private string _product;
        private string _softwareCode;
        private string _requestVersion;
        private string _customerName;
        private string _company;
        private string _phone;
        private string _email;
        private string _machineName;
        private string _osVersion;
        private string _hardwareId;
        private DateTime _requestDate;

        public string AppName { get { return _appName; } set { _appName = value; } }
        public string Product { get { return _product; } set { _product = value; } }
        public string SoftwareCode { get { return _softwareCode; } set { _softwareCode = value; } }
        public string RequestVersion { get { return _requestVersion; } set { _requestVersion = value; } }
        public string CustomerName { get { return _customerName; } set { _customerName = value; } }
        public string Company { get { return _company; } set { _company = value; } }
        public string Phone { get { return _phone; } set { _phone = value; } }
        public string Email { get { return _email; } set { _email = value; } }
        public string MachineName { get { return _machineName; } set { _machineName = value; } }
        public string OsVersion { get { return _osVersion; } set { _osVersion = value; } }
        public string HardwareId { get { return _hardwareId; } set { _hardwareId = value; } }
        public DateTime RequestDate { get { return _requestDate; } set { _requestDate = value; } }

        public LicenseRequest()
        {
            _appName = "AVRoad 2026 (TDT 9.1)";
            _product = "AVRoad 2026 (TDT 9.1)";
            _softwareCode = "AVROAD_TDT91";
            _requestVersion = "1.1";
            _customerName = string.Empty;
            _company = string.Empty;
            _phone = string.Empty;
            _email = string.Empty;
            _machineName = string.Empty;
            _osVersion = string.Empty;
            _hardwareId = string.Empty;
            _requestDate = DateTime.UtcNow;
        }

        public string ToJson()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["app_name"] = _appName;
            dict["product"] = _product;
            dict["software_code"] = _softwareCode;
            dict["request_version"] = _requestVersion;
            dict["customer_name"] = _customerName;
            dict["company"] = _company;
            dict["phone"] = _phone;
            dict["email"] = _email;
            dict["machine_name"] = _machineName;
            dict["os_version"] = _osVersion;
            dict["hwid"] = _hardwareId;
            dict["request_date"] = _requestDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            return SimpleJson.Serialize(dict, true);
        }
    }

    /// <summary>
    /// Nội dung chi tiết của bản quyền (Payload được ký số).
    /// </summary>
    public class LicenseData
    {
        private string _licenseId;
        private string _product;
        private string _softwareCode;
        private string _customerName;
        private string _company;
        private string _hardwareId;
        private List<string> _modules;
        private DateTime _issuedDate;
        private DateTime? _expiryDate;
        private DateTime? _updateExpiryDate;
        private string _licenseType;
        private string _notes;

        public string LicenseId { get { return _licenseId; } set { _licenseId = value; } }
        public string Product { get { return _product; } set { _product = value; } }
        public string SoftwareCode { get { return _softwareCode; } set { _softwareCode = value; } }
        public string CustomerName { get { return _customerName; } set { _customerName = value; } }
        public string Company { get { return _company; } set { _company = value; } }
        public string HardwareId { get { return _hardwareId; } set { _hardwareId = value; } }
        public List<string> Modules { get { return _modules; } set { _modules = value; } }
        public DateTime IssuedDate { get { return _issuedDate; } set { _issuedDate = value; } }
        public DateTime? ExpiryDate { get { return _expiryDate; } set { _expiryDate = value; } }
        public DateTime? UpdateExpiryDate { get { return _updateExpiryDate; } set { _updateExpiryDate = value; } }
        public string LicenseType { get { return _licenseType; } set { _licenseType = value; } }
        public string Notes { get { return _notes; } set { _notes = value; } }

        public LicenseData()
        {
            _licenseId = string.Empty;
            _product = "AVRoad 2026 (TDT 9.1)";
            _softwareCode = "AVROAD_TDT91";
            _customerName = string.Empty;
            _company = string.Empty;
            _hardwareId = string.Empty;
            _modules = new List<string>(new string[] { "ALL" });
            _issuedDate = DateTime.UtcNow;
            _licenseType = "Commercial";
            _notes = string.Empty;
        }

        /// <summary>
        /// Bản quyền vĩnh viễn (Perpetual) nếu không có hạn dùng hoặc hạn >= năm 2099.
        /// </summary>
        public bool IsPerpetual
        {
            get
            {
                return !_expiryDate.HasValue || _expiryDate.Value.Year >= 2099;
            }
        }

        public static LicenseData FromDictionary(Dictionary<string, object> dict)
        {
            if (dict == null) return null;
            LicenseData data = new LicenseData();

            if (dict.ContainsKey("license_id")) data._licenseId = Convert.ToString(dict["license_id"]);
            if (dict.ContainsKey("product")) data._product = Convert.ToString(dict["product"]);
            if (dict.ContainsKey("software_code")) data._softwareCode = Convert.ToString(dict["software_code"]);
            if (dict.ContainsKey("customer_name")) data._customerName = Convert.ToString(dict["customer_name"]);
            if (dict.ContainsKey("company")) data._company = Convert.ToString(dict["company"]);

            // Support both "hardware_id" and "hwid"
            if (dict.ContainsKey("hardware_id")) data._hardwareId = Convert.ToString(dict["hardware_id"]);
            else if (dict.ContainsKey("hwid")) data._hardwareId = Convert.ToString(dict["hwid"]);

            if (dict.ContainsKey("license_type")) data._licenseType = Convert.ToString(dict["license_type"]);
            if (dict.ContainsKey("notes")) data._notes = Convert.ToString(dict["notes"]);

            if (dict.ContainsKey("modules"))
            {
                data._modules.Clear();
                object modObj = dict["modules"];
                if (modObj is List<object>)
                {
                    foreach (object item in (List<object>)modObj)
                    {
                        if (item != null) data._modules.Add(item.ToString());
                    }
                }
                else if (modObj is string)
                {
                    data._modules.Add(modObj.ToString());
                }
            }

            if (dict.ContainsKey("issued_date") && dict["issued_date"] != null)
            {
                DateTime dt;
                if (DateTime.TryParse(dict["issued_date"].ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                    data._issuedDate = dt;
            }

            if (dict.ContainsKey("expiry_date") && dict["expiry_date"] != null)
            {
                string expStr = dict["expiry_date"].ToString().Trim();
                DateTime dt;
                if (!string.IsNullOrEmpty(expStr) && !expStr.Equals("null", StringComparison.OrdinalIgnoreCase))
                {
                    if (DateTime.TryParse(expStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                        data._expiryDate = dt;
                }
            }

            if (dict.ContainsKey("update_expiry_date") && dict["update_expiry_date"] != null)
            {
                string updStr = dict["update_expiry_date"].ToString().Trim();
                DateTime dt;
                if (!string.IsNullOrEmpty(updStr) && !updStr.Equals("null", StringComparison.OrdinalIgnoreCase))
                {
                    if (DateTime.TryParse(updStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                        data._updateExpiryDate = dt;
                }
            }

            return data;
        }
    }

    /// <summary>
    /// File bản quyền ký số (.avlic) được lưu trên máy tính.
    /// </summary>
    public class SignedLicense
    {
        private string _version;
        private string _signatureAlgorithm;
        private string _payload;
        private string _signature;
        private LicenseData _parsedData;

        public string Version { get { return _version; } set { _version = value; } }
        public string SignatureAlgorithm { get { return _signatureAlgorithm; } set { _signatureAlgorithm = value; } }
        public string Payload { get { return _payload; } set { _payload = value; } }
        public string Signature { get { return _signature; } set { _signature = value; } }
        public LicenseData ParsedData { get { return _parsedData; } set { _parsedData = value; } }

        public SignedLicense()
        {
            _version = "1.0";
            _signatureAlgorithm = "RSA-SHA256";
            _payload = string.Empty;
            _signature = string.Empty;
        }
    }

    /// <summary>
    /// Trạng thái kiểm tra bản quyền.
    /// </summary>
    public enum LicenseStatus
    {
        Valid,
        Expired,
        Revoked,
        InvalidHardware,
        InvalidSignature,
        NotFound,
        Error
    }

    /// <summary>
    /// Kết quả xác thực bản quyền.
    /// </summary>
    public class LicenseValidationResult
    {
        private bool _isValid;
        private LicenseStatus _status;
        private string _message;
        private LicenseData _data;
        private int _daysRemaining;

        public bool IsValid { get { return _isValid; } set { _isValid = value; } }
        public LicenseStatus Status { get { return _status; } set { _status = value; } }
        public string Message { get { return _message; } set { _message = value; } }
        public LicenseData Data { get { return _data; } set { _data = value; } }
        public int DaysRemaining { get { return _daysRemaining; } set { _daysRemaining = value; } }

        public LicenseValidationResult()
        {
            _message = string.Empty;
        }

        public static LicenseValidationResult Success(LicenseData data, int daysRemaining)
        {
            LicenseValidationResult res = new LicenseValidationResult();
            res._isValid = true;
            res._status = LicenseStatus.Valid;
            res._data = data;
            res._daysRemaining = daysRemaining;
            res._message = "Bản quyền hợp lệ.";
            return res;
        }

        public static LicenseValidationResult Outdated(LicenseStatus status, string detail)
        {
            LicenseValidationResult res = new LicenseValidationResult();
            res._isValid = false;
            res._status = status;
            res._message = "Phần mềm đã cũ, bạn vui lòng cập nhật phiên bản mới để tiếp tục sử dụng.";
            return res;
        }
    }
}
