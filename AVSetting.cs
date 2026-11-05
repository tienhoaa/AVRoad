using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Win32;

namespace AVSettingApp
{
    public class AVSettingForm : Form
    {
        private TextBox txtFont;
        private TextBox txtPlotStyle;
        private TextBox txtPlotter;
        private CheckBox chkAutoStart;
        private CheckBox chkCheckUpdate;
        private ComboBox cboUpdateChannel;
        private Image logoImg = null;
        private string appDir;
        private string iniPath;

        public AVSettingForm()
        {
            appDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
            iniPath = Path.Combine(appDir, "AVRoad.ini");

            string logoPath = Path.Combine(appDir, "av_bim_logo_transparent.png");
            if (File.Exists(logoPath))
            {
                try { logoImg = Image.FromFile(logoPath); } catch { }
            }

            string icoPath = Path.Combine(appDir, "AVRoad.ico");
            if (File.Exists(icoPath))
            {
                try { this.Icon = new Icon(icoPath); } catch { }
            }

            this.Text = "AVRoad 2026 - Cấu Hình & Tối Ưu Hệ Thống";
            this.Size = new Size(720, 605);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

            BuildUI();
            LoadSettings();
        }

        private void BuildUI()
        {
            // Header Panel
            Panel header = new Panel()
            {
                Dock = DockStyle.Top,
                Height = 90,
                BackColor = Color.FromArgb(7, 35, 26)
            };
            header.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                using (LinearGradientBrush brush = new LinearGradientBrush(header.ClientRectangle, Color.FromArgb(7, 35, 26), Color.FromArgb(17, 85, 65), LinearGradientMode.Horizontal))
                {
                    g.FillRectangle(brush, header.ClientRectangle);
                }

                if (logoImg != null)
                {
                    int lw = 150;
                    int lh = (int)(logoImg.Height * (lw / (double)logoImg.Width));
                    g.DrawImage(logoImg, new Rectangle(20, (header.Height - lh) / 2, lw, lh));
                }
                else
                {
                    using (Font fontAV = new Font("Arial", 28f, FontStyle.Bold | FontStyle.Italic))
                    using (Brush b = new SolidBrush(Color.White))
                    {
                        g.DrawString("AV", fontAV, b, 20, 18);
                    }
                }

                using (Font f1 = new Font("Segoe UI", 16f, FontStyle.Bold))
                using (Brush b1 = new SolidBrush(Color.FromArgb(94, 234, 212)))
                {
                    g.DrawString("Road 2026 Solution - Trung Tâm Cấu Hình", f1, b1, 185, 16);
                }

                using (Font f2 = new Font("Segoe UI", 9.5f, FontStyle.Regular))
                using (Brush b2 = new SolidBrush(Color.FromArgb(204, 251, 241)))
                {
                    g.DrawString("Thiết lập đường dẫn Font, Bảng in và Quản lý tối ưu AVCAD Core", f2, b2, 185, 50);
                }
            };
            this.Controls.Add(header);

            // Body Panel
            Panel body = new Panel()
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24, 16, 24, 16)
            };
            this.Controls.Add(body);
            body.BringToFront();

            int y = 14;

            // Group 1: Đường Dẫn Dữ Liệu
            GroupBox grpPaths = new GroupBox()
            {
                Text = "  1. Cấu hình Thư mục Dữ liệu & Font CAD  ",
                Location = new Point(20, y),
                Size = new Size(660, 200),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42)
            };
            body.Controls.Add(grpPaths);

            int py = 30;

            // Font
            Label lblFont = new Label() { Text = "Thư mục Font CAD bổ sung:", Location = new Point(16, py), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Regular) };
            txtFont = new TextBox() { Location = new Point(16, py + 22), Size = new Size(520, 25), Font = new Font("Segoe UI", 9f) };
            Button btnFont = CreateSmallBtn("Duyệt...", new Point(544, py + 21));
            btnFont.Click += (s, e) => BrowseFolder(txtFont, "Chọn thư mục chứa Font CAD (.shx, .ttf)");
            grpPaths.Controls.AddRange(new Control[] { lblFont, txtFont, btnFont });

            py += 54;
            // PlotStyle
            Label lblPS = new Label() { Text = "Thư mục Bảng nét in (Plot Styles .ctb/.stb):", Location = new Point(16, py), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Regular) };
            txtPlotStyle = new TextBox() { Location = new Point(16, py + 22), Size = new Size(520, 25), Font = new Font("Segoe UI", 9f) };
            Button btnPS = CreateSmallBtn("Duyệt...", new Point(544, py + 21));
            btnPS.Click += (s, e) => BrowseFolder(txtPlotStyle, "Chọn thư mục bảng nét in (.ctb, .stb)");
            grpPaths.Controls.AddRange(new Control[] { lblPS, txtPlotStyle, btnPS });

            py += 54;
            // Plotter
            Label lblPlt = new Label() { Text = "Thư mục Cấu hình máy in (Plotters .pc3):", Location = new Point(16, py), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Regular) };
            txtPlotter = new TextBox() { Location = new Point(16, py + 22), Size = new Size(520, 25), Font = new Font("Segoe UI", 9f) };
            Button btnPlt = CreateSmallBtn("Duyệt...", new Point(544, py + 21));
            btnPlt.Click += (s, e) => BrowseFolder(txtPlotter, "Chọn thư mục cấu hình máy in (.pc3)");
            grpPaths.Controls.AddRange(new Control[] { lblPlt, txtPlotter, btnPlt });

            y += 215;

            // Group 2: Tùy Chọn & Tối Ưu
            GroupBox grpOpt = new GroupBox()
            {
                Text = "  2. Tùy chọn Khởi động & Kênh Cập Nhật  ",
                Location = new Point(20, y),
                Size = new Size(660, 155),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42)
            };
            body.Controls.Add(grpOpt);

            chkAutoStart = new CheckBox()
            {
                Text = "Tự động vào thẳng AVRoad từ các lần sau (Giữ SHIFT khi mở nếu muốn vào lại màn hình này)",
                Location = new Point(18, 25),
                AutoSize = true,
                Checked = true,
                Font = new Font("Segoe UI", 9.25f, FontStyle.Regular),
                ForeColor = Color.FromArgb(30, 41, 59)
            };
            chkCheckUpdate = new CheckBox()
            {
                Text = "Tự động kiểm tra bản vá từ GitHub ở góc màn hình khi khởi động (Toast notification)",
                Location = new Point(18, 52),
                AutoSize = true,
                Checked = true,
                Font = new Font("Segoe UI", 9.25f, FontStyle.Regular),
                ForeColor = Color.FromArgb(30, 41, 59)
            };

            Label lblChannel = new Label()
            {
                Text = "Kênh cập nhật:",
                Location = new Point(18, 83),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = Color.FromArgb(30, 41, 59)
            };
            cboUpdateChannel = new ComboBox()
            {
                Location = new Point(115, 80),
                Size = new Size(330, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9f)
            };
            cboUpdateChannel.Items.Add("Chính thức (Stable v1.9 - Khuyến nghị)");
            cboUpdateChannel.Items.Add("Thử nghiệm (Release Candidate - RC v2.0)");
            cboUpdateChannel.SelectedIndex = 0;

            Button btnCheckNow = new Button()
            {
                Text = "Kiểm Tra Ngay...",
                Location = new Point(455, 79),
                Size = new Size(130, 27),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                BackColor = Color.FromArgb(226, 232, 240),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCheckNow.FlatAppearance.BorderSize = 0;
            btnCheckNow.Click += (s, e) => CheckOnlineUpdateGUI();

            Button btnClearCache = new Button()
            {
                Text = "Dọn Cache & Tối Ưu CAD",
                Location = new Point(18, 116),
                Size = new Size(170, 26),
                Font = new Font("Segoe UI", 8.5f),
                BackColor = Color.FromArgb(241, 245, 249),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClearCache.Click += (s, e) =>
            {
                OptimizeCAD();
                MessageBox.Show("Đã làm sạch bộ đệm tạm và tối ưu nhân AVCAD Engine!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            grpOpt.Controls.AddRange(new Control[] { chkAutoStart, chkCheckUpdate, lblChannel, cboUpdateChannel, btnCheckNow, btnClearCache });

            y += 168;

            // Action Buttons
            Button btnSaveAndRun = new Button()
            {
                Text = "LƯU CẤU HÌNH & VÀO AVROAD 2026",
                Location = new Point(20, y),
                Size = new Size(330, 44),
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSaveAndRun.FlatAppearance.BorderSize = 0;
            btnSaveAndRun.Click += (s, e) =>
            {
                SaveSettings();
                ApplyProfileToRegistry();
                LaunchAVRoad();
                this.Close();
            };
            body.Controls.Add(btnSaveAndRun);

            Button btnSaveOnly = new Button()
            {
                Text = "Chỉ Lưu Cấu Hình",
                Location = new Point(362, y),
                Size = new Size(150, 44),
                BackColor = Color.FromArgb(226, 232, 240),
                ForeColor = Color.FromArgb(30, 41, 59),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSaveOnly.FlatAppearance.BorderSize = 0;
            btnSaveOnly.Click += (s, e) =>
            {
                SaveSettings();
                ApplyProfileToRegistry();
                MessageBox.Show("Đã lưu cấu hình thành công!", "AVRoad 2026", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            body.Controls.Add(btnSaveOnly);

            Button btnExit = new Button()
            {
                Text = "Thoát",
                Location = new Point(524, y),
                Size = new Size(156, 44),
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = Color.FromArgb(71, 85, 105),
                Font = new Font("Segoe UI", 9.5f),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnExit.FlatAppearance.BorderSize = 0;
            btnExit.Click += (s, e) => this.Close();
            body.Controls.Add(btnExit);
        }

        private Button CreateSmallBtn(string text, Point loc)
        {
            Button btn = new Button()
            {
                Text = text,
                Location = loc,
                Size = new Size(95, 26),
                Font = new Font("Segoe UI", 8.5f),
                BackColor = Color.FromArgb(226, 232, 240),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void BrowseFolder(TextBox target, string desc)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = desc;
                if (!string.IsNullOrEmpty(target.Text) && Directory.Exists(target.Text))
                {
                    fbd.SelectedPath = target.Text;
                }
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    target.Text = fbd.SelectedPath;
                }
            }
        }

        private void LoadSettings()
        {
            string channel = "Stable";
            txtPlotStyle.Text = Path.Combine(appDir, "Plot Styles");
            txtPlotter.Text = Path.Combine(appDir, "Plotters");
            txtFont.Text = "";

            if (File.Exists(iniPath))
            {
                try
                {
                    foreach (string l in File.ReadAllLines(iniPath))
                    {
                        int eq = l.IndexOf('=');
                        if (eq > 0)
                        {
                            string k = l.Substring(0, eq).Trim();
                            string v = l.Substring(eq + 1).Trim();
                            if (k == "AutoStart") chkAutoStart.Checked = (v == "1");
                            if (k == "CheckUpdateOnStartup") chkCheckUpdate.Checked = (v == "1");
                            if (k == "UpdateChannel") channel = v;
                            if (k == "FontPath") txtFont.Text = v;
                            if (k == "PlotStylePath") txtPlotStyle.Text = v;
                            if (k == "PlotterPath") txtPlotter.Text = v;
                        }
                    }
                }
                catch { }
            }

            if (cboUpdateChannel != null) cboUpdateChannel.SelectedIndex = string.Equals(channel, "RC", StringComparison.OrdinalIgnoreCase) ? 1 : 0;

            if (string.IsNullOrEmpty(txtFont.Text))
            {
                string[] defFonts = new string[] {
                    @"C:\Program Files\Autodesk\AutoCAD 2026\fonts",
                    @"C:\Program Files\Autodesk\AutoCAD 2023\fonts",
                    @"C:\Program Files\Autodesk\AutoCAD 2021\fonts",
                    @"C:\Program Files\Autodesk\AutoCAD 2018\fonts",
                    @"C:\Program Files (x86)\AutoCAD 2007\fonts",
                    Path.Combine(appDir, "Fonts")
                };
                foreach (string df in defFonts)
                {
                    if (Directory.Exists(df))
                    {
                        txtFont.Text = df;
                        break;
                    }
                }
            }
        }

        private void SaveSettings()
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(iniPath, false, System.Text.Encoding.UTF8))
                {
                    string channel = (cboUpdateChannel != null && cboUpdateChannel.SelectedIndex == 1) ? "RC" : "Stable";
                    string updateUrl = (channel == "RC")
                        ? "https://raw.githubusercontent.com/tienhoaa/AVRoad/main/version-2.0-rc.json"
                        : "https://raw.githubusercontent.com/tienhoaa/AVRoad/main/version.json";

                    sw.WriteLine("[AVSetting]");
                    sw.WriteLine("Configured=1");
                    sw.WriteLine("AutoStart=" + (chkAutoStart.Checked ? "1" : "0"));
                    sw.WriteLine("CheckUpdateOnStartup=" + (chkCheckUpdate.Checked ? "1" : "0"));
                    sw.WriteLine("UpdateChannel=" + channel);
                    sw.WriteLine("UpdateUrl=" + updateUrl);
                    sw.WriteLine("FontPath=" + txtFont.Text.Trim());
                    sw.WriteLine("PlotStylePath=" + txtPlotStyle.Text.Trim());
                    sw.WriteLine("PlotterPath=" + txtPlotter.Text.Trim());
                    sw.WriteLine("ArgProfilePath=" + Path.Combine(appDir, "AVRoad_Profile.arg"));
                    sw.WriteLine("VTEnable=1");
                    sw.WriteLine("ZoomFactor=80");
                    sw.WriteLine("WhipArc=1");
                    sw.WriteLine("SelectionPreview=2");
                    sw.WriteLine("BgColor=BIM");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu file AVRoad.ini: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OptimizeCAD()
        {
            try
            {
                string tempDir = Path.Combine(appDir, "Temp");
                if (Directory.Exists(tempDir))
                {
                    foreach (string f in Directory.GetFiles(tempDir))
                    {
                        try { File.Delete(f); } catch { }
                    }
                }
            }
            catch { }
        }

        private void ApplyProfileToRegistry()
        {
            string profilesKeyPath = @"Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409\Profiles";
            try
            {
                using (RegistryKey profilesKey = Registry.CurrentUser.OpenSubKey(profilesKeyPath, true))
                {
                    if (profilesKey != null)
                    {
                        string supportPath = Path.Combine(appDir, "Support");
                        string fontsPath = Path.Combine(appDir, "Fonts");
                        string expressPath = Path.Combine(appDir, "Express");
                        string colorPath = Path.Combine(appDir, @"Support\color");
                        string helpPath = Path.Combine(appDir, "Help");
                        string drvPath = Path.Combine(appDir, "Drv");
                        string templatePath = Path.Combine(appDir, "Template");

                        string cleanAcad = "";
                        if (!string.IsNullOrEmpty(txtFont.Text.Trim()) && Directory.Exists(txtFont.Text.Trim()))
                        {
                            cleanAcad += txtFont.Text.Trim() + ";";
                        }
                        cleanAcad += supportPath + ";" + fontsPath + ";" + expressPath + ";" + colorPath + ";" + helpPath;

                        using (RegistryKey genKey = profilesKey.CreateSubKey(@"AVRoad\General"))
                        {
                            if (genKey != null)
                            {
                                genKey.SetValue("ACAD", cleanAcad);
                                if (!string.IsNullOrEmpty(txtPlotter.Text.Trim())) genKey.SetValue("PrinterConfigDir", txtPlotter.Text.Trim());
                                if (!string.IsNullOrEmpty(txtPlotStyle.Text.Trim())) genKey.SetValue("PrinterStyleSheetDir", txtPlotStyle.Text.Trim());
                                genKey.SetValue("ColorBookLocation", colorPath);
                                genKey.SetValue("ACADDRV", drvPath);
                                genKey.SetValue("TemplatePath", templatePath);
                            }
                        }

                        // Permanently block Communication Center and LaunchNFW
                        using (RegistryKey appKey1 = Registry.CurrentUser.CreateSubKey(@"Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409\Applications\WSCommCntrAcCon"))
                        {
                            if (appKey1 != null)
                            {
                                appKey1.SetValue("LOADCTRLS", 0, RegistryValueKind.DWord);
                                appKey1.SetValue("LOADER", "", RegistryValueKind.String);
                            }
                        }
                        using (RegistryKey appKey2 = Registry.CurrentUser.CreateSubKey(@"Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409\Applications\AcadAcLaunchNFW"))
                        {
                            if (appKey2 != null)
                            {
                                appKey2.SetValue("LOADCTRLS", 0, RegistryValueKind.DWord);
                                appKey2.SetValue("LOADER", "", RegistryValueKind.String);
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private void LaunchAVRoad()
        {
            string launcherExe = Path.Combine(appDir, "AVRoad.exe");
            if (File.Exists(launcherExe))
            {
                try
                {
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = launcherExe,
                        Arguments = "/direct",
                        WorkingDirectory = appDir
                    });
                }
                catch { }
            }
        }

        private void CheckOnlineUpdateGUI()
        {
            try
            {
                string localVer = "1.0";
                string verPath = Path.Combine(appDir, "version.json");
                if (File.Exists(verPath))
                {
                    string localJson = File.ReadAllText(verPath);
                    Match mLocal = Regex.Match(localJson, "\"version\"\\s*:\\s*\"([^\"]+)\"");
                    if (mLocal.Success) localVer = mLocal.Groups[1].Value;
                }

                bool isRc = (cboUpdateChannel != null && cboUpdateChannel.SelectedIndex == 1);
                string channelName = isRc ? "Thử nghiệm (RC)" : "Chính thức (Stable)";
                string checkUrl = isRc
                    ? "https://raw.githubusercontent.com/tienhoaa/AVRoad/main/version-2.0-rc.json"
                    : "https://raw.githubusercontent.com/tienhoaa/AVRoad/main/version.json";

                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; // TLS 1.2
                string fetchUrl = checkUrl + (checkUrl.Contains("?") ? "&" : "?") + "t=" + DateTime.UtcNow.Ticks;
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(fetchUrl);
                req.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.BypassCache);
                req.UserAgent = "AVRoad-Client";
                req.Timeout = 6000;

                string remoteJson = "";
                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                {
                    remoteJson = sr.ReadToEnd();
                }

                Match mRemote = Regex.Match(remoteJson, "\"version\"\\s*:\\s*\"([^\"]+)\"");
                if (!mRemote.Success)
                {
                    MessageBox.Show("Không thể đọc thông tin phiên bản từ GitHub!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string remoteVer = mRemote.Groups[1].Value;
                string changelog = "";
                Match mChange = Regex.Match(remoteJson, "\"changelog\"\\s*:\\s*\"([^\"]+)\"");
                if (mChange.Success) changelog = mChange.Groups[1].Value.Replace("\\n", "\n");

                string patchUrl = "";
                Match mUrl = Regex.Match(remoteJson, "\"patch_download_url\"\\s*:\\s*\"([^\"]+)\"");
                if (!mUrl.Success) mUrl = Regex.Match(remoteJson, "\"download_url\"\\s*:\\s*\"([^\"]+)\"");
                if (mUrl.Success) patchUrl = mUrl.Groups[1].Value;

                string patchSha = "";
                Match shaMatch = Regex.Match(remoteJson, "\"patch_sha256\"\\s*:\\s*\"([a-fA-F0-9]{64})\"");
                if (shaMatch.Success) patchSha = shaMatch.Groups[1].Value;

                bool isNewer = IsNewer(remoteVer, localVer);

                if (isNewer)
                {
                    DialogResult dr = MessageBox.Show(
                        string.Format("Đã tìm thấy bản cập nhật mới v{0} trên kênh {1}!\n(Phiên bản hiện tại của bạn: v{2})\n\nNội dung bản vá:\n{3}\n\nBạn có muốn mở trình tải bản vá (AVUpdater) để cập nhật ngay không?", remoteVer, channelName, localVer, changelog),
                        "Có Bản Cập Nhật Mới - AVRoad 2026",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information);

                    if (dr == DialogResult.Yes)
                    {
                        SaveSettings();
                        LaunchUpdater(patchUrl, remoteVer, patchSha);
                        this.Close();
                    }
                }
                else
                {
                    MessageBox.Show(
                        string.Format("Bạn đang sử dụng phiên bản: v{0}!\nKênh cập nhật: {1}.\n\nKết nối GitHub Releases: THÀNH CÔNG (100% OK).\nHiện tại không có bản cập nhật mới hơn trên kênh này.", localVer, channelName),
                        "Kiểm Tra Cập Nhật - AVRoad 2026",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối kiểm tra cập nhật từ GitHub:\n" + ex.Message + "\n\nVui lòng kiểm tra lại kết nối mạng hoặc thử lại sau.", "Lỗi Kiểm Tra Cập Nhật", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LaunchUpdater(string patchUrl, string remoteVer, string patchSha)
        {
            string updaterExe = Path.Combine(appDir, "AVUpdater.v2.exe");
            if (!File.Exists(updaterExe)) updaterExe = Path.Combine(appDir, "AVUpdater.exe");
            if (File.Exists(updaterExe))
            {
                string args = "";
                if (!string.IsNullOrEmpty(patchUrl)) args += " \"/url:" + patchUrl + "\"";
                if (!string.IsNullOrEmpty(remoteVer)) args += " \"/ver:" + remoteVer + "\"";
                if (!string.IsNullOrEmpty(patchSha)) args += " \"/sha:" + patchSha + "\"";

                Process.Start(new ProcessStartInfo()
                {
                    FileName = updaterExe,
                    Arguments = args.Trim(),
                    WorkingDirectory = appDir
                });
            }
        }

        private bool IsNewer(string remote, string local)
        {
            try
            {
                string[] rParts = remote.Split('.');
                string[] lParts = local.Split('.');
                int len = Math.Max(rParts.Length, lParts.Length);
                for (int i = 0; i < len; i++)
                {
                    int rv = 0;
                    int lv = 0;
                    if (i < rParts.Length)
                    {
                        Match m = Regex.Match(rParts[i], @"\d+");
                        if (m.Success) int.TryParse(m.Value, out rv);
                    }
                    if (i < lParts.Length)
                    {
                        Match m = Regex.Match(lParts[i], @"\d+");
                        if (m.Success) int.TryParse(m.Value, out lv);
                    }
                    if (rv > lv) return true;
                    if (rv < lv) return false;
                }
            }
            catch { }
            return false;
        }



        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new AVSettingForm());
        }
    }
}
