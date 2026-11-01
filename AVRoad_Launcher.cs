using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace AVRoadLauncher
{
    public class SplashScreen : Form
    {
        private System.Windows.Forms.Timer animTimer;
        private System.Windows.Forms.Timer autoCloseTimer;
        private int progress = 0;
        private string statusText = "Đang kết nối không gian dữ liệu CDE...";
        private Image logoImg = null;

        public SplashScreen()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(640, 360);
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.DoubleBuffered = true;

            string appDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
            string logoPath = Path.Combine(appDir, "av_bim_logo_transparent.png");
            if (File.Exists(logoPath))
            {
                try { logoImg = Image.FromFile(logoPath); } catch { }
            }

            animTimer = new System.Windows.Forms.Timer();
            animTimer.Interval = 20;
            animTimer.Tick += (s, e) =>
            {
                if (progress < 96) progress += 3;
                if (progress < 30) statusText = "Đang kết nối không gian dữ liệu CDE...";
                else if (progress < 60) statusText = "Đang tối ưu hiệu năng nhân đồ họa AVCAD Engine...";
                else if (progress < 85) statusText = "Đang nạp module AV_KS, AV_Road, Express Tools...";
                else statusText = "Sẵn sàng làm việc!";
                this.Invalidate();
            };
            animTimer.Start();

            autoCloseTimer = new System.Windows.Forms.Timer();
            autoCloseTimer.Interval = 2200;
            autoCloseTimer.Tick += (s, e) =>
            {
                autoCloseTimer.Stop();
                this.Close();
            };
            autoCloseTimer.Start();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (animTimer != null) animTimer.Dispose();
                if (autoCloseTimer != null) autoCloseTimer.Dispose();
                if (logoImg != null) logoImg.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Brand Gradient: Deep Forest Green (#07231A to #17664F)
            using (LinearGradientBrush bgBrush = new LinearGradientBrush(this.ClientRectangle, Color.FromArgb(7, 35, 26), Color.FromArgb(23, 102, 79), LinearGradientMode.ForwardDiagonal))
            {
                g.FillRectangle(bgBrush, this.ClientRectangle);
            }

            // Outer Border Mint Neon
            using (Pen borderPen = new Pen(Color.FromArgb(45, 212, 191), 1.5f))
            {
                g.DrawRectangle(borderPen, 0, 0, this.Width - 1, this.Height - 1);
            }

            // Highway curve
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddBezier(0, 290, 190, 210, 390, 320, 640, 230);
                using (Pen roadPen = new Pen(Color.FromArgb(45, 45, 212, 191), 38))
                {
                    g.DrawPath(roadPen, path);
                }
                using (Pen linePen = new Pen(Color.FromArgb(220, 255, 255, 255), 2.5f))
                {
                    linePen.DashStyle = DashStyle.Dash;
                    g.DrawPath(linePen, path);
                }
            }

            // Logo
            int logoY = 32;
            if (logoImg != null)
            {
                int lw = 175;
                int lh = (int)(logoImg.Height * (lw / (double)logoImg.Width));
                g.DrawImage(logoImg, new Rectangle(42, logoY, lw, lh));
            }
            else
            {
                using (Font fontAV = new Font("Arial", 42f, FontStyle.Bold | FontStyle.Italic))
                using (Brush brushAV = new SolidBrush(Color.White))
                {
                    g.DrawString("AV", fontAV, brushAV, 42, logoY);
                }
            }

            // Title "Road 2026"
            using (Font fontRoad = new Font("Segoe UI", 38f, FontStyle.Bold))
            using (Brush brushRoad = new SolidBrush(Color.FromArgb(94, 234, 212)))
            {
                g.DrawString("Road", fontRoad, brushRoad, 225, logoY - 4);
            }
            using (Font tagFont = new Font("Segoe UI", 12f, FontStyle.Bold))
            using (Brush tagBrush = new SolidBrush(Color.FromArgb(153, 246, 228)))
            {
                g.DrawString("2026", tagFont, tagBrush, 360, logoY + 22);
            }

            // Slogan CDE
            using (Font sloganFont = new Font("Segoe UI", 11.5f, FontStyle.Bold))
            using (Brush sloganBrush = new SolidBrush(Color.FromArgb(254, 240, 138)))
            {
                g.DrawString("CHUẨN HÓA DỮ LIỆU CDE • TỐI ƯU THIẾT KẾ HẠ TẦNG BIM", sloganFont, sloganBrush, 46, 122);
            }

            // Subtitle
            using (Font subFont = new Font("Segoe UI", 9.5f, FontStyle.Regular))
            using (Brush subBrush = new SolidBrush(Color.FromArgb(204, 251, 241)))
            {
                g.DrawString("COMMON DATA ENVIRONMENT • HỆ SINH THÁI THIẾT KẾ TUYẾN CHUYÊN NGHIỆP", subFont, subBrush, 46, 150);
            }

            // Engine badge
            using (Font engFont = new Font("Segoe UI", 8.5f, FontStyle.Regular))
            using (Brush engBrush = new SolidBrush(Color.FromArgb(167, 243, 208)))
            {
                g.DrawString("GRAPHICS CORE: AVCAD 2026 ENGINE (PORTABLE ISOLATED)", engFont, engBrush, 46, 175);
            }

            // Status Text
            using (Font statusFont = new Font("Segoe UI", 9.5f, FontStyle.Italic))
            using (Brush statusBrush = new SolidBrush(Color.FromArgb(240, 253, 250)))
            {
                g.DrawString(statusText, statusFont, statusBrush, 46, 260);
            }

            // Progress Bar Track
            int barX = 46;
            int barY = 288;
            int barW = 548;
            int barH = 6;
            using (Brush trackBrush = new SolidBrush(Color.FromArgb(18, 55, 42)))
            {
                g.FillRectangle(trackBrush, barX, barY, barW, barH);
            }

            // Progress Bar Fill
            int fillW = (int)(barW * (progress / 100.0));
            if (fillW > 0)
            {
                using (LinearGradientBrush fillBrush = new LinearGradientBrush(new Rectangle(barX, barY, fillW, barH), Color.FromArgb(45, 212, 191), Color.FromArgb(16, 185, 129), LinearGradientMode.Horizontal))
                {
                    g.FillRectangle(fillBrush, barX, barY, fillW, barH);
                }
            }

            // Copyright footer
            using (Font copyFont = new Font("Segoe UI", 8f, FontStyle.Regular))
            using (Brush copyBrush = new SolidBrush(Color.FromArgb(120, 175, 150)))
            {
                g.DrawString("AV Technology Ecosystem • Hệ sinh thái AV BIM • Khởi chạy độc lập hoàn toàn", copyFont, copyBrush, 46, 316);
            }
        }
    }

    public class UpdateToastForm : Form
    {
        public UpdateToastForm(string newVer, string changelog, string patchUrl, Process acadProc, string appDir)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.Size = new Size(380, 180);
            this.Text = "AVRoad 2026 - Bản vá mới";
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.DoubleBuffered = true;

            // Position at bottom-right of primary screen
            Rectangle wa = Screen.PrimaryScreen.WorkingArea;
            this.Location = new Point(wa.Right - this.Width - 20, wa.Bottom - this.Height - 20);

            BuildUI(newVer, changelog, patchUrl, acadProc, appDir);
        }

        private void BuildUI(string newVer, string changelog, string patchUrl, Process acadProc, string appDir)
        {
            this.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                using (LinearGradientBrush brush = new LinearGradientBrush(this.ClientRectangle, Color.FromArgb(7, 35, 26), Color.FromArgb(17, 85, 65), LinearGradientMode.ForwardDiagonal))
                {
                    g.FillRectangle(brush, this.ClientRectangle);
                }

                using (Pen pen = new Pen(Color.FromArgb(45, 212, 191), 1.5f))
                {
                    g.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
                }

                // Accent icon / badge
                using (SolidBrush b = new SolidBrush(Color.FromArgb(45, 212, 191)))
                {
                    g.FillEllipse(b, 16, 16, 12, 12);
                }

                using (Font fTitle = new Font("Segoe UI", 11.5f, FontStyle.Bold))
                using (Brush bTitle = new SolidBrush(Color.FromArgb(94, 234, 212)))
                {
                    g.DrawString("AVRoad 2026 - Bản vá mới v" + newVer, fTitle, bTitle, 36, 12);
                }

                using (Font fDesc = new Font("Segoe UI", 9f, FontStyle.Regular))
                using (Brush bDesc = new SolidBrush(Color.FromArgb(204, 251, 241)))
                {
                    string summary = "Đã có bản cập nhật vá lỗi (Hotfix) trên GitHub.";
                    if (!string.IsNullOrEmpty(changelog))
                    {
                        summary += "\n" + (changelog.Length > 85 ? changelog.Substring(0, 85) + "..." : changelog);
                    }
                    summary += "\nBạn có muốn thoát CAD để áp dụng bản vá ngay?";
                    g.DrawString(summary, fDesc, bDesc, new RectangleF(16, 42, 348, 70));
                }
            };

            // Buttons
            Button btnUpdate = new Button()
            {
                Text = "CẬP NHẬT NGAY",
                Location = new Point(16, 128),
                Size = new Size(180, 36),
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnUpdate.FlatAppearance.BorderSize = 0;
            btnUpdate.Click += (s, e) =>
            {
                this.Hide();

                // 1. Gracefully close CAD
                try
                {
                    if (acadProc != null && !acadProc.HasExited)
                    {
                        acadProc.CloseMainWindow();
                        if (!acadProc.WaitForExit(1500)) acadProc.Kill();
                    }
                }
                catch { }

                // 2. Launch AVUpdater.exe
                string updaterExe = Path.Combine(appDir, "AVUpdater.exe");
                if (File.Exists(updaterExe))
                {
                    try
                    {
                        string args = "";
                        if (!string.IsNullOrEmpty(patchUrl)) args += " \"/url:" + patchUrl + "\"";
                        if (!string.IsNullOrEmpty(newVer)) args += " \"/ver:" + newVer + "\"";

                        Process.Start(new ProcessStartInfo()
                        {
                            FileName = updaterExe,
                            Arguments = args.Trim(),
                            WorkingDirectory = appDir
                        });
                    }
                    catch { }
                }

                this.Close();
                Application.Exit();
            };
            this.Controls.Add(btnUpdate);

            Button btnSkip = new Button()
            {
                Text = "Để Sau",
                Location = new Point(206, 128),
                Size = new Size(158, 36),
                BackColor = Color.FromArgb(30, 60, 50),
                ForeColor = Color.FromArgb(204, 251, 241),
                Font = new Font("Segoe UI", 9.5f),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSkip.FlatAppearance.BorderSize = 0;
            btnSkip.Click += (s, e) =>
            {
                this.Close();
            };
            this.Controls.Add(btnSkip);
        }
    }

    public class AboutForm : Form
    {
        private Image logoImg = null;

        public AboutForm(string appDir)
        {
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(520, 360);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Text = "Giới thiệu AVRoad 2026 Solution - Hệ sinh thái AV BIM CDE";
            this.BackColor = Color.FromArgb(7, 35, 26);
            this.DoubleBuffered = true;

            string icoPath = Path.Combine(appDir, "AVRoad.ico");
            if (File.Exists(icoPath))
            {
                try { this.Icon = new Icon(icoPath); } catch { }
            }

            string logoPath = Path.Combine(appDir, "av_bim_logo_transparent.png");
            if (File.Exists(logoPath))
            {
                try { logoImg = Image.FromFile(logoPath); } catch { }
            }

            BuildUI(appDir);
        }

        private void BuildUI(string appDir)
        {
            this.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                using (LinearGradientBrush b = new LinearGradientBrush(this.ClientRectangle, Color.FromArgb(7, 35, 26), Color.FromArgb(23, 102, 79), LinearGradientMode.ForwardDiagonal))
                {
                    g.FillRectangle(b, this.ClientRectangle);
                }

                int logoY = 24;
                if (logoImg != null)
                {
                    int lw = 150;
                    int lh = (int)(logoImg.Height * (lw / (double)logoImg.Width));
                    g.DrawImage(logoImg, new Rectangle(30, logoY, lw, lh));
                }

                using (Font fTitle = new Font("Segoe UI", 18f, FontStyle.Bold))
                using (Brush brTitle = new SolidBrush(Color.FromArgb(94, 234, 212)))
                {
                    g.DrawString("AVRoad 2026 Solution", fTitle, brTitle, 200, 24);
                }

                using (Font fSub = new Font("Segoe UI", 9.5f))
                using (Brush brSub = new SolidBrush(Color.FromArgb(204, 251, 241)))
                {
                    g.DrawString("HỆ THỐNG THIẾT KẾ TUYẾN & HẠ TẦNG GIAO THÔNG", fSub, brSub, 202, 58);
                }

                using (Font fSlogan = new Font("Segoe UI", 9f, FontStyle.Bold))
                using (Brush brSlogan = new SolidBrush(Color.FromArgb(254, 240, 138)))
                {
                    g.DrawString("CHUẨN HÓA DỮ LIỆU CDE • TỐI ƯU THIẾT KẾ HẠ TẦNG BIM", fSlogan, brSlogan, 30, 120);
                }

                using (Font fBody = new Font("Segoe UI", 9f))
                using (Brush brBody = new SolidBrush(Color.FromArgb(226, 232, 240)))
                {
                    string info = "• Nhân đồ họa: AVCAD Core Engine (Portable Isolated độc lập)\n" +
                                  "• Phiên bản: v1.0 (2026 Edition)\n" +
                                  "• Tích hợp: 7 Menu nghiệp vụ AV_KS, AV_Road, San nền, Kênh...\n" +
                                  "• Bản quyền: AV Technology Ecosystem. Tất cả các quyền được bảo lưu.";
                    g.DrawString(info, fBody, brBody, 30, 150);
                }
            };

            Button btnWeb = new Button()
            {
                Text = "Trang Chủ Dự Án (GitHub)",
                Location = new Point(30, 260),
                Size = new Size(210, 38),
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnWeb.FlatAppearance.BorderSize = 0;
            btnWeb.Click += (s, e) =>
            {
                try { Process.Start("https://github.com/tienhoaa/AVRoad"); } catch { }
            };
            this.Controls.Add(btnWeb);

            Button btnClose = new Button()
            {
                Text = "Đóng",
                Location = new Point(255, 260),
                Size = new Size(120, 38),
                BackColor = Color.FromArgb(30, 60, 50),
                ForeColor = Color.FromArgb(204, 251, 241),
                Font = new Font("Segoe UI", 9.5f),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);
        }
    }

    static class Program
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowText(IntPtr hWnd, string lpString);

        [DllImport("user32.dll")]
        static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
        delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("dwmapi.dll")]
        static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        const int DWMWA_USE_IMMERSIVE_DARK_MODE_OLD = 19;

        const uint WM_SETICON = 0x80;
        const int ICON_SMALL = 0;
        const int ICON_BIG = 1;

        static SplashScreen globalSplash = null;

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string appDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
            string iniPath = Path.Combine(appDir, "AVRoad.ini");

            foreach (string a in args)
            {
                if (a.Equals("/about", StringComparison.OrdinalIgnoreCase))
                {
                    Application.Run(new AboutForm(appDir));
                    return;
                }
            }

            bool isDirect = false;
            foreach (string a in args)
            {
                if (a.Equals("/direct", StringComparison.OrdinalIgnoreCase)) isDirect = true;
            }

            bool isFirstRun = true;
            bool autoStart = false;
            bool checkUpdateOnStartup = true;
            string fontPath = "";
            string plotStylePath = Path.Combine(appDir, "Plot Styles");
            string plotterPath = Path.Combine(appDir, "Plotters");
            string argProfile = Path.Combine(appDir, "AVRoad_Profile.arg");

            if (File.Exists(iniPath))
            {
                foreach (string l in File.ReadAllLines(iniPath))
                {
                    int eq = l.IndexOf('=');
                    if (eq > 0)
                    {
                        string k = l.Substring(0, eq).Trim();
                        string v = l.Substring(eq + 1).Trim();
                        if (k == "Configured" && v == "1") isFirstRun = false;
                        if (k == "AutoStart" && v == "1") autoStart = true;
                        if (k == "CheckUpdateOnStartup" && v == "0") checkUpdateOnStartup = false;
                        if (k == "FontPath" && !string.IsNullOrEmpty(v)) fontPath = v;
                        if (k == "PlotStylePath" && !string.IsNullOrEmpty(v)) plotStylePath = v;
                        if (k == "PlotterPath" && !string.IsNullOrEmpty(v)) plotterPath = v;
                        if (k == "ArgProfilePath" && !string.IsNullOrEmpty(v)) argProfile = v;
                    }
                }
            }

            bool shiftPressed = (Control.ModifierKeys & Keys.Shift) == Keys.Shift;

            // FIRST RUN BEHAVIOR:
            if (!isDirect && (isFirstRun || shiftPressed || !autoStart))
            {
                string settingExe = Path.Combine(appDir, "AVSetting.exe");
                if (File.Exists(settingExe))
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo()
                        {
                            FileName = settingExe,
                            WorkingDirectory = appDir
                        });
                        return; // Exit launcher, AV Setting takes over!
                    }
                    catch { }
                }
            }

            // FROM THE SECOND RUN ONWARD:
            Thread splashThread = new Thread(() =>
            {
                globalSplash = new SplashScreen();
                Application.Run(globalSplash);
            });
            splashThread.SetApartmentState(ApartmentState.STA);
            splashThread.IsBackground = true;
            splashThread.Start();

            // Prepare Clean AVRoad Profile
            PrepareProfile(appDir, argProfile, fontPath, plotStylePath, plotterPath);
            LaunchAndBranding(appDir, checkUpdateOnStartup);
        }

        static void PrepareProfile(string appDir, string argProfile, string fontPath, string plotStylePath, string plotterPath)
        {
            string profilesKeyPath = @"Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409\Profiles";

            bool profileExists = false;
            try
            {
                using (RegistryKey pk = Registry.CurrentUser.OpenSubKey(profilesKeyPath + @"\AVRoad"))
                {
                    if (pk != null) profileExists = true;
                }
            }
            catch { }

            if (!profileExists && !string.IsNullOrEmpty(argProfile) && File.Exists(argProfile))
            {
                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo("reg.exe", "import \"" + argProfile + "\"");
                    psi.CreateNoWindow = true;
                    psi.UseShellExecute = false;
                    Process p = Process.Start(psi);
                    if (p != null) p.WaitForExit();
                }
                catch { }
            }

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

                        string cleanAcad = appDir + ";";
                        if (!string.IsNullOrEmpty(fontPath) && Directory.Exists(fontPath))
                        {
                            cleanAcad += fontPath + ";";
                        }
                        cleanAcad += supportPath + ";" + fontsPath + ";" + expressPath + ";" + colorPath + ";" + helpPath;

                        try
                        {
                            using (RegistryKey rk = Registry.CurrentUser.CreateSubKey(@"Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409"))
                            {
                                if (rk != null)
                                {
                                    rk.SetValue("SerialNumber", "000-00000000");
                                    rk.SetValue("Location", appDir);
                                    rk.SetValue("ProductName", "AVRoad 2026");
                                    rk.SetValue("Release", "17.0");
                                    rk.SetValue("CurVer", "ACAD-5001:409");
                                }
                            }
                        }
                        catch { }

                        try
                        {
                            using (RegistryKey rk = Registry.LocalMachine.CreateSubKey(@"Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409"))
                            {
                                if (rk != null)
                                {
                                    rk.SetValue("SerialNumber", "000-00000000");
                                    rk.SetValue("Location", appDir);
                                    rk.SetValue("ProductName", "AVRoad 2026");
                                }
                            }
                            using (RegistryKey rk64 = Registry.LocalMachine.CreateSubKey(@"Software\WOW6432Node\Autodesk\AutoCAD\R17.0\ACAD-5001:409"))
                            {
                                if (rk64 != null)
                                {
                                    rk64.SetValue("SerialNumber", "000-00000000");
                                    rk64.SetValue("Location", appDir);
                                    rk64.SetValue("ProductName", "AVRoad 2026");
                                }
                            }
                        }
                        catch { }

                        using (RegistryKey genKey = profilesKey.CreateSubKey(@"AVRoad\General"))
                        {
                            if (genKey != null)
                            {
                                genKey.SetValue("ACAD", cleanAcad);
                                if (!string.IsNullOrEmpty(plotterPath)) genKey.SetValue("PrinterConfigDir", plotterPath);
                                if (!string.IsNullOrEmpty(plotStylePath)) genKey.SetValue("PrinterStyleSheetDir", plotStylePath);
                                genKey.SetValue("PrinterDescDir", Path.Combine(plotterPath, "PMP Files"));
                                genKey.SetValue("ColorBookLocation", colorPath);
                                genKey.SetValue("ACADDRV", drvPath);
                                genKey.SetValue("ACADHELP", Path.Combine(helpPath, "acad.chm"));
                                genKey.SetValue("TemplatePath", templatePath);
                            }
                        }

                        // Configure MenuFile to point to Support\acad
                        using (RegistryKey cfgKey = profilesKey.CreateSubKey(@"AVRoad\General Configuration"))
                        {
                            if (cfgKey != null)
                            {
                                cfgKey.SetValue("MenuFile", Path.Combine(supportPath, "acad"));
                            }
                        }

                        // Configure Editor Configuration
                        using (RegistryKey edKey = profilesKey.CreateSubKey(@"AVRoad\Editor Configuration"))
                        {
                            if (edKey != null)
                            {
                                edKey.SetValue("FontMappingFile", Path.Combine(supportPath, "acad.fmp"));
                                edKey.SetValue("CustomDictionary", Path.Combine(supportPath, "sample.cus"));
                            }
                        }

                        // Permanently block Communication Center and LaunchNFW in HKCU
                        try
                        {
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
                            using (RegistryKey cmKey = Registry.CurrentUser.CreateSubKey(@"Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409\CadManagerControl\CommunicationCenter"))
                            {
                                if (cmKey != null)
                                {
                                    cmKey.SetValue("EnableCommunicationCenter", 0, RegistryValueKind.DWord);
                                    cmKey.SetValue("MaintenancePackNotification", 0, RegistryValueKind.DWord);
                                    cmKey.SetValue("EnableOnlineContent", 0, RegistryValueKind.DWord);
                                }
                            }
                            using (RegistryKey icKey = Registry.CurrentUser.CreateSubKey(@"Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409\InfoCenter"))
                            {
                                if (icKey != null)
                                {
                                    icKey.SetValue("InfoCenterOn", 0, RegistryValueKind.DWord);
                                    icKey.SetValue("BalloonNotification", 0, RegistryValueKind.DWord);
                                }
                            }
                            using (RegistryKey wsKey = Registry.CurrentUser.CreateSubKey(@"Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409\WebServices\CommunicationCenter"))
                            {
                                if (wsKey != null)
                                {
                                    wsKey.SetValue("OutOfBoxWizardCompletedStep", 47, RegistryValueKind.DWord);
                                    wsKey.SetValue("EnableBubbleNotification", 0, RegistryValueKind.DWord);
                                    wsKey.SetValue("PollingFrequency", 0, RegistryValueKind.DWord);
                                    wsKey.SetValue("DataState", 0, RegistryValueKind.DWord);
                                }
                            }
                        }
                        catch { }

                        // Configure Exact Menu Bar Order
                        using (RegistryKey menuKey = profilesKey.CreateSubKey(@"AVRoad\Menus"))
                        {
                            if (menuKey != null)
                            {
                                menuKey.SetValue("Group1", "CUSTOM custom");
                                for (int i = 1; i <= 30; i++)
                                {
                                    try { menuKey.DeleteValue("Pop" + i); } catch { }
                                }
                                string[] order = new string[] {
                                    "ACAD pop1",        // File
                                    "ACAD pop2",        // Edit
                                    "ACAD pop3",        // View
                                    "ACAD pop4",        // Insert
                                    "ACAD pop5",        // Format
                                    "ACAD pop6",        // Tools
                                    "ACAD pop7",        // Draw
                                    "ACAD pop8",        // Dimension
                                    "ACAD pop9",        // Modify
                                    "ACAD pop10",       // Window
                                    "EXPRESS pop1",     // Express Tools
                                    "KSVN POP1",        // AV_KS
                                    "VNROAD POP1",      // AV_Road
                                    "SANNEN POP1",      // AV_SanNen
                                    "KVN POP1",         // AV_Kenh
                                    "DCCT POP1",        // AV_DiaChat
                                    "TDT3D POP1",       // AV_3D
                                    "RM POP1",          // AV_VachSon
                                    "ACAD pop11"        // Help
                                };
                                for (int i = 0; i < order.Length; i++)
                                {
                                    menuKey.SetValue("Pop" + (i + 1), order[i]);
                                }
                            }
                        }
                    }
                }
            }
            catch { }
        }

        static void LaunchAndBranding(string appDir, bool checkUpdateOnStartup)
        {
            string acadExe = Path.Combine(appDir, "acad.exe");
            if (!File.Exists(acadExe))
            {
                CloseSplashSafe();
                MessageBox.Show("Không tìm thấy file nhân đồ họa AVCAD (acad.exe) để khởi động!", "AVRoad Solution", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string profilesKeyPath = @"Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409\Profiles";
            string originalProfile = "<<Unnamed Profile>>";

            try
            {
                using (RegistryKey profilesKey = Registry.CurrentUser.OpenSubKey(profilesKeyPath, true))
                {
                    if (profilesKey != null)
                    {
                        object defVal = profilesKey.GetValue("");
                        if (defVal != null && !string.IsNullOrEmpty(defVal.ToString()) && defVal.ToString() != "AVRoad")
                        {
                            originalProfile = defVal.ToString();
                        }
                    }
                }
            }
            catch { }

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = acadExe;
            psi.Arguments = "/nologo /p AVRoad";
            psi.WorkingDirectory = appDir;
            psi.UseShellExecute = false;

            Process acadProc = null;
            try
            {
                acadProc = Process.Start(psi);
            }
            catch (Exception ex)
            {
                CloseSplashSafe();
                MessageBox.Show("Lỗi khởi động AVRoad: " + ex.Message, "AVRoad Solution", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (acadProc != null)
            {
                uint targetPid = (uint)acadProc.Id;
                string icoPath = Path.Combine(appDir, "AVRoad.ico");
                IntPtr hIconSmall = IntPtr.Zero;
                IntPtr hIconBig = IntPtr.Zero;
                if (File.Exists(icoPath))
                {
                    try
                    {
                        hIconSmall = new Icon(icoPath, 16, 16).Handle;
                        hIconBig = new Icon(icoPath, 32, 32).Handle;
                    }
                    catch { }
                }

                // Monitor CAD Window Title & Icon
                Thread titleMonitorThread = new Thread(() =>
                {
                    bool closedSplash = false;
                    for (int i = 0; i < 300; i++)
                    {
                        Thread.Sleep(50);
                        if (acadProc.HasExited) break;

                        EnumWindows((hwnd, lparam) =>
                        {
                            uint pid;
                            GetWindowThreadProcessId(hwnd, out pid);
                            if (pid == targetPid && IsWindowVisible(hwnd))
                            {
                                System.Text.StringBuilder sb = new System.Text.StringBuilder(512);
                                GetWindowText(hwnd, sb, sb.Capacity);
                                string title = sb.ToString();

                                if (!string.IsNullOrEmpty(title))
                                {
                                    if (title.Contains("About AutoCAD") || title.Contains("About AVRoad"))
                                    {
                                        SetWindowText(hwnd, "Thông tin bản quyền AVRoad 2026 - AV Technology CDE");
                                    }
                                    else if (title == "AutoCAD")
                                    {
                                        SetWindowText(hwnd, "AVRoad 2026");
                                    }
                                    else if (title.Contains("AutoCAD") || title.Contains("Drawing") || title.Contains("AVRoad") || title.Contains("AVCAD"))
                                    {
                                        string newTitle = oldTitleRebrand(title);
                                        if (newTitle != title) SetWindowText(hwnd, newTitle);

                                        if (hIconSmall != IntPtr.Zero) SendMessage(hwnd, WM_SETICON, (IntPtr)ICON_SMALL, hIconSmall);
                                        if (hIconBig != IntPtr.Zero) SendMessage(hwnd, WM_SETICON, (IntPtr)ICON_BIG, hIconBig);

                                        if (!closedSplash)
                                        {
                                            closedSplash = true;
                                            CloseSplashSafe();
                                            SetForegroundWindow(hwnd);


                                        }
                                    }
                                }
                            }
                            return true;
                        }, IntPtr.Zero);
                    }

                    if (!closedSplash) CloseSplashSafe();
                });
                titleMonitorThread.IsBackground = true;
                titleMonitorThread.Start();

                // Background Update Checker (after CAD has loaded)
                if (checkUpdateOnStartup)
                {
                    Thread updateCheckThread = new Thread(() =>
                    {
                        Thread.Sleep(4500); // Wait 4.5 seconds so CAD has cleanly rendered
                        if (acadProc.HasExited) return;
                        CheckUpdateAsync(appDir, acadProc);
                    });
                    updateCheckThread.IsBackground = true;
                    updateCheckThread.Start();
                }

                // Wait for CAD to exit
                try
                {
                    acadProc.WaitForExit();
                }
                finally
                {
                    try
                    {
                        using (RegistryKey profilesKey = Registry.CurrentUser.OpenSubKey(profilesKeyPath, true))
                        {
                            if (profilesKey != null)
                            {
                                profilesKey.SetValue("", originalProfile);
                            }
                        }
                    }
                    catch { }
                }
            }
        }

        static void CheckUpdateAsync(string appDir, Process acadProc)
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

                string checkUrl = "https://raw.githubusercontent.com/tienhoaa/AVRoad/main/version.json";
                string iniPath = Path.Combine(appDir, "AVRoad.ini");
                if (File.Exists(iniPath))
                {
                    foreach (string line in File.ReadAllLines(iniPath))
                    {
                        if (line.StartsWith("UpdateUrl=", StringComparison.OrdinalIgnoreCase))
                        {
                            checkUrl = line.Substring(10).Trim();
                        }
                    }
                }

                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; // TLS 1.2
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(checkUrl);
                req.UserAgent = "AVRoad-Client";
                req.Timeout = 4000;
                req.ReadWriteTimeout = 4000;

                string remoteJson = "";
                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                {
                    remoteJson = sr.ReadToEnd();
                }

                Match mRemote = Regex.Match(remoteJson, "\"version\"\\s*:\\s*\"([^\"]+)\"");
                if (!mRemote.Success) return;

                string remoteVer = mRemote.Groups[1].Value;

                if (IsNewerVersion(remoteVer, localVer))
                {
                    string changelog = "";
                    Match mChange = Regex.Match(remoteJson, "\"changelog\"\\s*:\\s*\"([^\"]+)\"");
                    if (mChange.Success) changelog = mChange.Groups[1].Value.Replace("\\n", "\n");

                    string patchUrl = "";
                    Match mUrl = Regex.Match(remoteJson, "\"patch_download_url\"\\s*:\\s*\"([^\"]+)\"");
                    if (!mUrl.Success) mUrl = Regex.Match(remoteJson, "\"download_url\"\\s*:\\s*\"([^\"]+)\"");
                    if (mUrl.Success) patchUrl = mUrl.Groups[1].Value;

                    // Show Toast Popup
                    Application.Run(new UpdateToastForm(remoteVer, changelog, patchUrl, acadProc, appDir));
                }
            }
            catch
            {
                // Silent fail if offline or unreachable
            }
        }

        static bool IsNewerVersion(string remote, string local)
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
                    if (i < rParts.Length) int.TryParse(rParts[i], out rv);
                    if (i < lParts.Length) int.TryParse(lParts[i], out lv);
                    if (rv > lv) return true;
                    if (rv < lv) return false;
                }
            }
            catch { }
            return false;
        }

        static string oldTitleRebrand(string title)
        {
            if (title.Contains("AutoCAD 2007"))
            {
                return title.Replace("AutoCAD 2007", "AVRoad 2026 Solution - Powered by AVCAD Engine");
            }
            if (title.Contains("AutoCAD"))
            {
                return title.Replace("AutoCAD", "AVRoad 2026");
            }
            if (!title.Contains("AVRoad"))
            {
                return "AVRoad 2026 Solution - " + title + " [AVCAD Engine]";
            }
            return title;
        }

        static void CloseSplashSafe()
        {
            if (globalSplash != null)
            {
                try
                {
                    globalSplash.BeginInvoke((Action)(() =>
                    {
                        globalSplash.Close();
                        globalSplash.Dispose();
                    }));
                }
                catch { }
            }
        }
    }
}
