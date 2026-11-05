using AVRoadDeployment;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace AVRoadUpdater
{
    public class UpdaterForm : Form
    {
        private ProgressBar progressBar;
        private Label lblStatus;
        private Label lblDetails;
        private Button btnAction;
        private TextBox txtLog;
        private System.Windows.Forms.Timer countdownTimer;
        private int countdown = 3;

        private string appDir;
        private string patchUrl = "";
        private string targetVersion = "";
        private string changelog = "";
        private string tempZipPath = "";
        private bool isFinished = false;
        private string expectedPatchSha = "";

        public UpdaterForm(string[] args)
        {
            appDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');

            foreach (string arg in args)
            {
                if (arg.StartsWith("/appdir:", StringComparison.OrdinalIgnoreCase)) appDir = Path.GetFullPath(arg.Substring(8));
                if (arg.StartsWith("/sha:", StringComparison.OrdinalIgnoreCase)) expectedPatchSha = arg.Substring(5).Trim('"', ' ');
                if (arg.StartsWith("/url:", StringComparison.OrdinalIgnoreCase))
                {
                    patchUrl = arg.Substring(5).Trim('"', ' ');
                }
                if (arg.StartsWith("/ver:", StringComparison.OrdinalIgnoreCase))
                {
                    targetVersion = arg.Substring(5).Trim('"', ' ');
                }
            }

            string icoPath = Path.Combine(appDir, "AVRoad.ico");
            if (File.Exists(icoPath))
            {
                try { this.Icon = new Icon(icoPath); } catch { }
            }

            this.Text = "AVRoad 2026 - Cập Nhật Bản Vá Trực Tuyến";
            this.Size = new Size(580, 420);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Font = new Font("Segoe UI", 9.5f);

            BuildUI();
            this.Shown += (s, e) => StartUpdateProcess();
        }

        private void BuildUI()
        {
            Panel header = new Panel()
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(7, 35, 26)
            };
            header.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (LinearGradientBrush b = new LinearGradientBrush(header.ClientRectangle, Color.FromArgb(7, 35, 26), Color.FromArgb(17, 85, 65), LinearGradientMode.Horizontal))
                {
                    g.FillRectangle(b, header.ClientRectangle);
                }

                using (Font f1 = new Font("Segoe UI", 15f, FontStyle.Bold))
                using (Brush br1 = new SolidBrush(Color.FromArgb(94, 234, 212)))
                {
                    g.DrawString("CẬP NHẬT BẢN VÁ TRỰC TUYẾN (HOTFIX)", f1, br1, 20, 16);
                }
                using (Font f2 = new Font("Segoe UI", 9f))
                using (Brush br2 = new SolidBrush(Color.FromArgb(204, 251, 241)))
                {
                    g.DrawString("Tự động tải và áp dụng bản vá nhanh từ GitHub Releases vào AVRoad", f2, br2, 20, 46);
                }
            };
            this.Controls.Add(header);

            Panel body = new Panel()
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24, 18, 24, 18)
            };
            this.Controls.Add(body);
            body.BringToFront();

            lblStatus = new Label()
            {
                Text = "Đang kiểm tra tiến trình CAD và chuẩn bị cập nhật...",
                Location = new Point(20, 14),
                AutoSize = true,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42)
            };
            body.Controls.Add(lblStatus);

            lblDetails = new Label()
            {
                Text = "Vui lòng đợi...",
                Location = new Point(22, 40),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 116, 139)
            };
            body.Controls.Add(lblDetails);

            progressBar = new ProgressBar()
            {
                Location = new Point(22, 68),
                Size = new Size(520, 22),
                Style = ProgressBarStyle.Continuous
            };
            body.Controls.Add(progressBar);

            txtLog = new TextBox()
            {
                Location = new Point(22, 102),
                Size = new Size(520, 145),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(241, 245, 249),
                Font = new Font("Consolas", 8.75f),
                ForeColor = Color.FromArgb(30, 41, 59)
            };
            body.Controls.Add(txtLog);

            btnAction = new Button()
            {
                Text = "Đang Cập Nhật...",
                Location = new Point(22, 258),
                Size = new Size(520, 40),
                BackColor = Color.FromArgb(203, 213, 225),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Enabled = false,
                Cursor = Cursors.Hand
            };
            btnAction.FlatAppearance.BorderSize = 0;
            btnAction.Click += (s, e) =>
            {
                if (isFinished)
                {
                    RelaunchAVRoad();
                }
                else
                {
                    this.Close();
                }
            };
            body.Controls.Add(btnAction);
        }

        private void AppendLog(string msg)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((Action)(() => AppendLog(msg)));
                return;
            }
            txtLog.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + msg + Environment.NewLine);
        }

        private void SetStatus(string title, string detail)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((Action)(() => SetStatus(title, detail)));
                return;
            }
            lblStatus.Text = title;
            lblDetails.Text = detail;
        }

        private void StartUpdateProcess()
        {
            Thread t = new Thread(() =>
            {
                try
                {
                    AppendLog("Bắt đầu tiến trình cập nhật...");
                    string channel = "Stable";

                    try
                    {
                        ServicePointManager.Expect100Continue = true;
                        ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072 | (SecurityProtocolType)12288 | SecurityProtocolType.Tls;
                        // Use platform certificate validation.
                    }
                    catch { }

                    // Never terminate CAD or interrupt a save prompt.
                    foreach (Process cad in Process.GetProcessesByName("acad"))
                    {
                        try
                        {
                            string installedCad = Path.GetFullPath(Path.Combine(appDir, "acad.exe"));
                            if (!string.Equals(cad.MainModule.FileName, installedCad, StringComparison.OrdinalIgnoreCase)) continue;
                            SetStatus("Vui lòng lưu và đóng AVRoad", "Sau khi đóng CAD, mở lại trình cập nhật để tiếp tục.");
                            AppendLog("Dừng cập nhật: AVRoad trong thư mục này vẫn đang chạy.");
                            EnableCloseButton();
                            return;
                        }
                        catch (InvalidOperationException) { }
                        catch (System.ComponentModel.Win32Exception)
                        {
                            throw new IOException("Không kiểm tra được tiến trình CAD. Hãy đóng CAD trước khi cập nhật.");
                        }
                        finally { cad.Dispose(); }
                    }
                    // 2. Fetch patch info if not provided
                    if (string.IsNullOrEmpty(patchUrl))
                    {
                        SetStatus("Đang kiểm tra bản vá từ GitHub...", "Kết nối máy chủ GitHub Releases...");
                        AppendLog("Đang truy vấn thông tin bản vá từ GitHub...");

                        string iniPath = Path.Combine(appDir, "AVRoad.ini");
                                                string customUrl = null;
                        if (File.Exists(iniPath))
                        {
                            foreach (string line in File.ReadAllLines(iniPath))
                            {
                                if (line.StartsWith("UpdateChannel=", StringComparison.OrdinalIgnoreCase))
                                {
                                    channel = line.Substring(14).Trim();
                                }
                                else if (line.StartsWith("UpdateUrl=", StringComparison.OrdinalIgnoreCase))
                                {
                                    customUrl = line.Substring(10).Trim();
                                }
                            }
                        }

                        string checkUrl = "https://raw.githubusercontent.com/tienhoaa/AVRoad/main/version.json";
                        if (string.Equals(channel, "RC", StringComparison.OrdinalIgnoreCase))
                        {
                            if (string.IsNullOrEmpty(customUrl) || customUrl.EndsWith("/version.json", StringComparison.OrdinalIgnoreCase))
                            {
                                checkUrl = "https://raw.githubusercontent.com/tienhoaa/AVRoad/main/version-2.0-rc.json";
                            }
                            else
                            {
                                checkUrl = customUrl;
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(customUrl) || customUrl.EndsWith("/version-2.0-rc.json", StringComparison.OrdinalIgnoreCase))
                            {
                                checkUrl = "https://raw.githubusercontent.com/tienhoaa/AVRoad/main/version.json";
                            }
                            else
                            {
                                checkUrl = customUrl;
                            }
                        }

                        ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; // Tls12
                        using (WebClient wc = new WebClient())
                        {
                            wc.Headers.Add("User-Agent", "AVRoad-Updater");
                            string json = wc.DownloadString(checkUrl);
                            AppendLog("Nhận dữ liệu version.json thành công.");
                            Match mSha = Regex.Match(json, "\"patch_sha256\"\\s*:\\s*\"([a-fA-F0-9]{64})\"");
                            if (mSha.Success) expectedPatchSha = mSha.Groups[1].Value;

                            Match mVer = Regex.Match(json, "\"version\"\\s*:\\s*\"([^\"]+)\"");
                            if (mVer.Success) targetVersion = mVer.Groups[1].Value;

                            Match mUrl = Regex.Match(json, "\"patch_download_url\"\\s*:\\s*\"([^\"]+)\"");
                            if (!mUrl.Success) mUrl = Regex.Match(json, "\"download_url\"\\s*:\\s*\"([^\"]+)\"");
                            if (mUrl.Success) patchUrl = mUrl.Groups[1].Value;

                            Match mChange = Regex.Match(json, "\"changelog\"\\s*:\\s*\"([^\"]+)\"");
                            if (mChange.Success) changelog = mChange.Groups[1].Value.Replace("\\n", "\n");
                        }
                    }

                    if (string.IsNullOrEmpty(patchUrl))
                    {
                        AppendLog("Lỗi: Không tìm thấy liên kết tải bản vá (.zip) trên GitHub!");
                        SetStatus("Không tìm thấy bản vá", "Vui lòng thử lại sau hoặc tải trực tiếp từ GitHub.");
                        EnableCloseButton();
                        return;
                    }

                    // 2b. Ensure expected SHA256 if missing
                    if (string.IsNullOrEmpty(expectedPatchSha))
                    {
                        try
                        {
                            string checkJsonUrl = string.Equals(channel, "RC", StringComparison.OrdinalIgnoreCase)
                                ? "https://raw.githubusercontent.com/tienhoaa/AVRoad/main/version-2.0-rc.json"
                                : "https://raw.githubusercontent.com/tienhoaa/AVRoad/main/version.json";
                            using (WebClient wcSha = new WebClient())
                            {
                                wcSha.Headers.Add("User-Agent", "AVRoad-Updater");
                                string j = wcSha.DownloadString(checkJsonUrl);
                                Match m = Regex.Match(j, "\"patch_sha256\"\\s*:\\s*\"([a-fA-F0-9]{64})\"");
                                if (m.Success) expectedPatchSha = m.Groups[1].Value;
                            }
                        }
                        catch { }
                    }

                    // 3. Download Patch
                    SetStatus("Đang tải bản vá v" + targetVersion + "...", "Từ GitHub Releases...");
                    AppendLog("Bắt đầu tải bản vá từ: " + patchUrl);

                    tempZipPath = Path.Combine(appDir, "patch_temp.zip");
                    if (File.Exists(tempZipPath)) File.Delete(tempZipPath);

                    using (WebClient dlClient = new WebClient())
                    {
                        dlClient.Headers.Add("User-Agent", "AVRoad-Updater");
                        dlClient.DownloadProgressChanged += (s, e) =>
                        {
                            this.BeginInvoke((Action)(() =>
                            {
                                progressBar.Value = e.ProgressPercentage;
                                lblDetails.Text = string.Format("Đã tải: {0:F1} MB / {1:F1} MB ({2}%)", 
                                    e.BytesReceived / (1024.0 * 1024.0), 
                                    e.TotalBytesToReceive / (1024.0 * 1024.0), 
                                    e.ProgressPercentage);
                            }));
                        };

                        Exception downloadError = null;
                        AutoResetEvent done = new AutoResetEvent(false);
                        dlClient.DownloadFileCompleted += (s, e) =>
                        {
                            if (e.Error != null)
                            {
                                downloadError = e.Error;
                                AppendLog("Lỗi tải file: " + e.Error.Message);
                            }
                            else
                            {
                                AppendLog("Tải file bản vá hoàn tất!");
                            }
                            done.Set();
                        };

                        dlClient.DownloadFileAsync(new Uri(patchUrl), tempZipPath);
                        done.WaitOne();
                        if (downloadError != null) throw new IOException("Patch download failed", downloadError);
                    }

                    if (!File.Exists(tempZipPath) || new FileInfo(tempZipPath).Length == 0)
                    {
                        SetStatus("Lỗi tải file", "Không thể tải file bản vá về máy.");
                        EnableCloseButton();
                        return;
                    }

                    // 4. Extract and Apply Patch
                    SetStatus("Đang áp dụng bản vá...", "Đang trích xuất và cập nhật các tệp tin...");
                    AppendLog("Bắt đầu trích xuất các tệp tin bản vá...");

                    string rollback = PatchTransaction.Apply(tempZipPath, appDir, expectedPatchSha, false);
                    AppendLog("Rollback: " + rollback);

                    // Clean temp zip
                    try { File.Delete(tempZipPath); } catch { }

                    AppendLog("CẬP NHẬT HOÀN TẤT THÀNH CÔNG!");
                    SetStatus("Cập nhật thành công!", "Bản vá đã được áp dụng. Sẵn sàng khởi động lại!");

                    // Ready to Relaunch
                    this.BeginInvoke((Action)(() =>
                    {
                        isFinished = true;
                        progressBar.Value = 100;
                        btnAction.Enabled = true;
                        btnAction.BackColor = Color.FromArgb(16, 185, 129);
                        btnAction.Text = "KHỞI ĐỘNG LẠI AVROAD NGAY (3s)";

                        countdownTimer = new System.Windows.Forms.Timer();
                        countdownTimer.Interval = 1000;
                        countdownTimer.Tick += (sender, args2) =>
                        {
                            countdown--;
                            if (countdown <= 0)
                            {
                                countdownTimer.Stop();
                                RelaunchAVRoad();
                            }
                            else
                            {
                                btnAction.Text = "KHỞI ĐỘNG LẠI AVROAD NGAY (" + countdown + "s)";
                            }
                        };
                        countdownTimer.Start();
                    }));
                }
                catch (Exception ex)
                {
                    AppendLog("LỖI: " + ex.Message);
                    SetStatus("Lỗi quá trình cập nhật", ex.Message);
                    EnableCloseButton();
                }
            });
            t.IsBackground = true;
            t.Start();
        }

        private void EnableCloseButton()
        {
            this.BeginInvoke((Action)(() =>
            {
                btnAction.Enabled = true;
                btnAction.Text = "Đóng";
                btnAction.BackColor = Color.FromArgb(239, 68, 68);
            }));
        }

        private void RelaunchAVRoad()
        {
            if (countdownTimer != null) countdownTimer.Stop();

            string launcher = Path.Combine(appDir, "AVRoad.exe");
            if (File.Exists(launcher))
            {
                try
                {
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = launcher,
                        Arguments = "/direct",
                        WorkingDirectory = appDir
                    });
                }
                catch { }
            }
            Application.Exit();
        }

        [STAThread]
        public static void Main(string[] args)
        {
            // Same transaction engine as online updates; test mode never writes Registry.
            if (args.Length == 5 && args[0] == "/test-apply")
            {
                try { string journal = PatchTransaction.Apply(args[1], args[2], args[3], false); File.WriteAllText(args[4], journal); }
                catch (Exception ex) { File.WriteAllText(args[4], ex.ToString()); Environment.Exit(1); }
                return;
            }
            if (args.Length == 3 && args[0] == "/rollback")
            {
                try { PatchTransaction.Rollback(args[1]); File.WriteAllText(args[2], "Rollback completed"); }
                catch (Exception ex) { File.WriteAllText(args[2], ex.ToString()); Environment.Exit(1); }
                return;
            }
            // Run outside the installation so neither updater nor deployment DLL is locked.
            if (Array.IndexOf(args, "/detached") < 0)
            {
                string original = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
                string temp = Path.Combine(Path.GetTempPath(), "AVRoadUpdate-" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(temp);
                string runner = Path.Combine(temp, "AVUpdater.v2.exe");
                File.Copy(Application.ExecutablePath, runner);
                string dep = Path.Combine(original, "AVRoad.Deployment.dll"); if (File.Exists(dep)) File.Copy(dep, Path.Combine(temp, "AVRoad.Deployment.dll"));
                string forwarded = String.Join(" ", Array.ConvertAll(args, a => "\"" + a.Replace("\"", "\\\"") + "\""));
                Process.Start(new ProcessStartInfo(runner, forwarded + " /detached \"/appdir:" + original + "\"") { WorkingDirectory = temp });
                return;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072 | (SecurityProtocolType)12288 | SecurityProtocolType.Tls;
                // Use platform certificate validation.
            }
            catch { }

            Application.Run(new UpdaterForm(args));
        }
    }
}
