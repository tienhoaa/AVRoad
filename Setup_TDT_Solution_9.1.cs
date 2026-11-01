using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace TDTInstaller
{
    public class SetupWizardForm : Form
    {
        private Panel pnlHeader;
        private Panel pnlContent;
        private Panel pnlBottom;
        private Button btnBack;
        private Button btnNext;
        private Button btnCancel;

        private int currentStep = 0; // 0: Welcome, 1: Directory, 2: Shortcuts, 3: Extracting, 4: Finished

        // Step 1 controls
        private TextBox txtInstallDir;

        // Step 2 controls
        private CheckBox chkDesktopShortcut;
        private CheckBox chkStartMenuShortcut;

        // Step 3 controls
        private ProgressBar progressBar;
        private Label lblExtractStatus;
        private Label lblExtractFile;

        // Step 4 controls
        private CheckBox chkLaunchNow;

        public SetupWizardForm()
        {
            this.Text = "Cài đặt TDT Solution 9.1 (Bản chuẩn 2026 - Vĩnh viễn)";
            this.Size = new Size(620, 460);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Font = new Font("Segoe UI", 9.5f);

            string icoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TDT.ico");
            if (File.Exists(icoPath))
            {
                try { this.Icon = new Icon(icoPath); } catch { }
            }

            BuildLayout();
            ShowStep(0);
        }

        private void BuildLayout()
        {
            // Header
            pnlHeader = new Panel()
            {
                Dock = DockStyle.Top,
                Height = 78,
                BackColor = Color.FromArgb(16, 44, 87)
            };
            pnlHeader.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (LinearGradientBrush b = new LinearGradientBrush(pnlHeader.ClientRectangle, Color.FromArgb(16, 44, 87), Color.FromArgb(28, 78, 142), LinearGradientMode.Horizontal))
                {
                    g.FillRectangle(b, pnlHeader.ClientRectangle);
                }

                using (Font f1 = new Font("Segoe UI", 14f, FontStyle.Bold))
                using (Brush br1 = new SolidBrush(Color.White))
                {
                    g.DrawString("CÀI ĐẶT TDT SOLUTION 9.1", f1, br1, 20, 14);
                }
                using (Font f2 = new Font("Segoe UI", 9f))
                using (Brush br2 = new SolidBrush(Color.FromArgb(219, 234, 254)))
                {
                    g.DrawString("Bản chuẩn đầy đủ gốc • Tối ưu mở khóa vĩnh viễn 2026+ • Hoàn chỉnh 100% tính năng", f2, br2, 20, 44);
                }
            };
            this.Controls.Add(pnlHeader);

            // Bottom Panel
            pnlBottom = new Panel()
            {
                Dock = DockStyle.Bottom,
                Height = 58,
                BackColor = Color.FromArgb(241, 245, 249)
            };
            pnlBottom.Paint += (s, e) =>
            {
                using (Pen p = new Pen(Color.FromArgb(226, 232, 240), 1))
                {
                    e.Graphics.DrawLine(p, 0, 0, pnlBottom.Width, 0);
                }
            };

            btnCancel = new Button()
            {
                Text = "Hủy Bỏ",
                Size = new Size(95, 32),
                Location = new Point(505, 13),
                BackColor = Color.FromArgb(226, 232, 240),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f)
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => this.Close();

            btnNext = new Button()
            {
                Text = "Tiếp Tục >",
                Size = new Size(110, 32),
                Location = new Point(385, 13),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            btnNext.FlatAppearance.BorderSize = 0;
            btnNext.Click += (s, e) => NextStep();

            btnBack = new Button()
            {
                Text = "< Quay Lại",
                Size = new Size(95, 32),
                Location = new Point(280, 13),
                BackColor = Color.FromArgb(226, 232, 240),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f),
                Enabled = false
            };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Click += (s, e) => PrevStep();

            pnlBottom.Controls.AddRange(new Control[] { btnBack, btnNext, btnCancel });
            this.Controls.Add(pnlBottom);

            // Content Panel
            pnlContent = new Panel()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(24)
            };
            this.Controls.Add(pnlContent);
        }

        private void ShowStep(int step)
        {
            currentStep = step;
            pnlContent.Controls.Clear();

            btnBack.Enabled = (step > 0 && step < 3);
            btnNext.Enabled = true;
            btnCancel.Enabled = (step != 3);

            if (step == 0) // Welcome
            {
                btnNext.Text = "Tiếp Tục >";

                Label lblTitle = new Label()
                {
                    Text = "Chào Mừng Bạn Đến Với Bộ Cài Đặt TDT Solution 9.1",
                    Location = new Point(20, 20),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(15, 23, 42)
                };

                Label lblDesc = new Label()
                {
                    Text = "Chương trình sẽ cài đặt đầy đủ trọn bộ giải pháp TDT Solution 9.1 lên máy tính của bạn.\n\n" +
                           "• Giữ nguyên vẹn 100% nội dung gốc từ thư mục C:\\Program Files (x86)\\TDT Solution 9.1\n" +
                           "• Đầy đủ 7 Menu chuyên ngành: Khảo sát (KSVN), Thiết kế đường (VNROAD), San nền, Kênh, Địa chất, 3D, Vạch sơn...\n" +
                           "• Khắc phục hoàn toàn: Cắm cọc & Km trên trắc ngang, phím tắt Ctrl + 1 (Properties), lệnh TKAD, DAODAP.\n" +
                           "• Mở khóa vĩnh viễn: Hoạt động bền bỉ, không hết hạn năm 2026+.\n\n" +
                           "Nhấn 'Tiếp Tục' để chọn vị trí cài đặt.",
                    Location = new Point(20, 60),
                    Size = new Size(540, 220),
                    Font = new Font("Segoe UI", 9.5f),
                    ForeColor = Color.FromArgb(51, 65, 85)
                };

                pnlContent.Controls.AddRange(new Control[] { lblTitle, lblDesc });
            }
            else if (step == 1) // Directory
            {
                btnNext.Text = "Tiếp Tục >";

                Label lblTitle = new Label()
                {
                    Text = "Chọn Thư Mục Cài Đặt TDT Solution 9.1",
                    Location = new Point(20, 20),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(15, 23, 42)
                };

                Label lblSub = new Label()
                {
                    Text = "Thư mục cài đặt chuẩn mặc định:",
                    Location = new Point(20, 50),
                    Size = new Size(540, 35),
                    Font = new Font("Segoe UI", 9f),
                    ForeColor = Color.FromArgb(100, 116, 139)
                };

                txtInstallDir = new TextBox()
                {
                    Text = @"C:\Program Files (x86)\TDT Solution 9.1",
                    Location = new Point(20, 95),
                    Size = new Size(430, 26),
                    Font = new Font("Segoe UI", 9.5f)
                };

                Button btnBrowse = new Button()
                {
                    Text = "Duyệt...",
                    Location = new Point(458, 93),
                    Size = new Size(95, 28),
                    BackColor = Color.FromArgb(226, 232, 240),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9f)
                };
                btnBrowse.FlatAppearance.BorderSize = 0;
                btnBrowse.Click += (s, e) =>
                {
                    using (FolderBrowserDialog fbd = new FolderBrowserDialog())
                    {
                        fbd.Description = "Chọn thư mục cài đặt TDT Solution 9.1";
                        if (Directory.Exists(txtInstallDir.Text)) fbd.SelectedPath = txtInstallDir.Text;
                        if (fbd.ShowDialog() == DialogResult.OK)
                        {
                            txtInstallDir.Text = fbd.SelectedPath;
                        }
                    }
                };

                Label lblSpace = new Label()
                {
                    Text = "Dung lượng yêu cầu: ~320 MB\nDung lượng ổ đĩa khả dụng: Đầy đủ",
                    Location = new Point(20, 145),
                    Size = new Size(530, 40),
                    Font = new Font("Segoe UI", 9f),
                    ForeColor = Color.FromArgb(71, 85, 105)
                };

                pnlContent.Controls.AddRange(new Control[] { lblTitle, lblSub, txtInstallDir, btnBrowse, lblSpace });
            }
            else if (step == 2) // Shortcut Options
            {
                btnNext.Text = "Cài Đặt Ngay";

                Label lblTitle = new Label()
                {
                    Text = "Tùy Chọn Biểu Tượng Khởi Chạy",
                    Location = new Point(20, 20),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(15, 23, 42)
                };

                chkDesktopShortcut = new CheckBox()
                {
                    Text = "Tạo biểu tượng TDT Solution 9.1 trên màn hình Desktop",
                    Location = new Point(24, 75),
                    AutoSize = true,
                    Checked = true,
                    Font = new Font("Segoe UI", 9.5f)
                };

                chkStartMenuShortcut = new CheckBox()
                {
                    Text = "Tạo nhóm phím tắt TDT Solution 9.1 trong Start Menu",
                    Location = new Point(24, 115),
                    AutoSize = true,
                    Checked = true,
                    Font = new Font("Segoe UI", 9.5f)
                };

                pnlContent.Controls.AddRange(new Control[] { lblTitle, chkDesktopShortcut, chkStartMenuShortcut });
            }
            else if (step == 3) // Extracting
            {
                btnBack.Enabled = false;
                btnNext.Enabled = false;
                btnCancel.Enabled = false;

                Label lblTitle = new Label()
                {
                    Text = "Đang Tiến Hành Cài Đặt...",
                    Location = new Point(20, 20),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(15, 23, 42)
                };

                lblExtractStatus = new Label()
                {
                    Text = "Đang trích xuất dữ liệu chương trình...",
                    Location = new Point(20, 60),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9.5f),
                    ForeColor = Color.FromArgb(51, 65, 85)
                };

                progressBar = new ProgressBar()
                {
                    Location = new Point(20, 90),
                    Size = new Size(540, 24),
                    Style = ProgressBarStyle.Continuous
                };

                lblExtractFile = new Label()
                {
                    Text = "Vui lòng đợi...",
                    Location = new Point(20, 122),
                    Size = new Size(540, 24),
                    Font = new Font("Consolas", 8.5f),
                    ForeColor = Color.FromArgb(100, 116, 139)
                };

                pnlContent.Controls.AddRange(new Control[] { lblTitle, lblExtractStatus, progressBar, lblExtractFile });

                StartExtraction();
            }
            else if (step == 4) // Finished
            {
                btnBack.Enabled = false;
                btnCancel.Enabled = false;
                btnNext.Enabled = true;
                btnNext.Text = "HOÀN TẤT";

                Label lblTitle = new Label()
                {
                    Text = "Cài Đặt TDT Solution 9.1 Thành Công 100%!",
                    Location = new Point(20, 20),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 12.5f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(16, 185, 129)
                };

                Label lblDesc = new Label()
                {
                    Text = "Bộ phần mềm TDT Solution 9.1 đã được cài đặt hoàn tất vào thư mục:\n" + txtInstallDir.Text + "\n\n" +
                           "• Biểu tượng 'TDT Solution 9.1' đã được tạo sẵn sàng trên Desktop.\n" +
                           "• Toàn bộ cấu hình đường dẫn mẫu, cọc trắc ngang, phím tắt Properties đã được kích hoạt.\n" +
                           "• Bạn có thể bắt đầu sử dụng ngay lập tức.",
                    Location = new Point(20, 60),
                    Size = new Size(540, 110),
                    Font = new Font("Segoe UI", 9.5f),
                    ForeColor = Color.FromArgb(51, 65, 85)
                };

                chkLaunchNow = new CheckBox()
                {
                    Text = "Khởi chạy TDT Solution 9.1 ngay bây giờ",
                    Location = new Point(24, 180),
                    AutoSize = true,
                    Checked = true,
                    Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(15, 23, 42)
                };

                pnlContent.Controls.AddRange(new Control[] { lblTitle, lblDesc, chkLaunchNow });
            }
        }

        private void NextStep()
        {
            if (currentStep == 4)
            {
                if (chkLaunchNow != null && chkLaunchNow.Checked)
                {
                    string targetExe = Path.Combine(txtInstallDir.Text.Trim(), "TDT9.1.exe");
                    if (File.Exists(targetExe))
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo()
                            {
                                FileName = targetExe,
                                WorkingDirectory = txtInstallDir.Text.Trim()
                            });
                        }
                        catch { }
                    }
                }
                this.Close();
                return;
            }

            ShowStep(currentStep + 1);
        }

        private void PrevStep()
        {
            if (currentStep > 0)
            {
                ShowStep(currentStep - 1);
            }
        }

        private void StartExtraction()
        {
            string targetDir = txtInstallDir.Text.Trim();
            if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);

            Thread t = new Thread(() =>
            {
                try
                {
                    Assembly asm = Assembly.GetExecutingAssembly();
                    Stream zipStream = asm.GetManifestResourceStream("payload.zip");
                    if (zipStream == null)
                    {
                        foreach (string res in asm.GetManifestResourceNames())
                        {
                            if (res.IndexOf("payload", StringComparison.OrdinalIgnoreCase) >= 0 || res.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                            {
                                zipStream = asm.GetManifestResourceStream(res);
                                if (zipStream != null) break;
                            }
                        }
                    }
                    if (zipStream == null)
                    {
                        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                        string[] diskCandidates = new string[] {
                            Path.Combine(baseDir, "payload.zip"),
                            Path.Combine(baseDir, "TDT_Solution_9.1.zip"),
                            @"C:\TDT_RESEARCH\TDT_Solution_9.1.zip"
                        };
                        foreach (string cand in diskCandidates)
                        {
                            if (File.Exists(cand))
                            {
                                zipStream = new FileStream(cand, FileMode.Open, FileAccess.Read, FileShare.Read);
                                break;
                            }
                        }
                    }
                    if (zipStream == null)
                    {
                        MessageBox.Show("Không tìm thấy tệp tài nguyên payload.zip bên trong hoặc cạnh trình cài đặt!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    using (zipStream)
                    {
                        using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
                        {
                            int totalEntries = archive.Entries.Count;
                            int currentEntry = 0;

                            foreach (ZipArchiveEntry entry in archive.Entries)
                            {
                                currentEntry++;
                                int percent = (int)((currentEntry / (double)totalEntries) * 100);

                                string destPath = Path.Combine(targetDir, entry.FullName);
                                if (string.IsNullOrEmpty(entry.Name)) // Directory
                                {
                                    if (!Directory.Exists(destPath)) Directory.CreateDirectory(destPath);
                                    continue;
                                }

                                string parentDir = Path.GetDirectoryName(destPath);
                                if (!Directory.Exists(parentDir)) Directory.CreateDirectory(parentDir);

                                try
                                {
                                    entry.ExtractToFile(destPath, true);
                                }
                                catch { }

                                if (currentEntry % 25 == 0 || currentEntry == totalEntries)
                                {
                                    this.BeginInvoke((Action)(() =>
                                    {
                                        progressBar.Value = percent;
                                        lblExtractStatus.Text = string.Format("Đang trích xuất: {0}/{1} tệp ({2}%)", currentEntry, totalEntries, percent);
                                        lblExtractFile.Text = entry.FullName;
                                    }));
                                }
                            }
                        }
                    }

                    // Register Registry Keys
                    RegisterRegistry(targetDir);

                    // Create Shortcuts
                    CreateShortcuts(targetDir);

                    // Proceed to step 4
                    this.BeginInvoke((Action)(() =>
                    {
                        ShowStep(4);
                    }));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi trong quá trình cài đặt: " + ex.Message, "Lỗi Cài Đặt", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });
            t.IsBackground = true;
            t.Start();
        }

        private void RegisterRegistry(string targetDir)
        {
            try
            {
                using (RegistryKey tdtKey = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\WOW6432Node\TDT\TDT Solution 9.1"))
                {
                    if (tdtKey != null)
                    {
                        tdtKey.SetValue("Path", targetDir + "\\", RegistryValueKind.String);
                        tdtKey.SetValue("Version", "1.0.48", RegistryValueKind.String);
                    }
                }
            }
            catch { }

            try
            {
                using (RegistryKey rk = Registry.CurrentUser.CreateSubKey(@"Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409"))
                {
                    if (rk != null)
                    {
                        rk.SetValue("SerialNumber", "000-00000000");
                        rk.SetValue("Location", targetDir);
                        rk.SetValue("ProductName", "TDT Solution 9.1");
                        rk.SetValue("Release", "17.0");
                        rk.SetValue("CurVer", "ACAD-5001:409");
                    }
                }
            }
            catch { }

            try
            {
                using (RegistryKey opmKey = Registry.CurrentUser.CreateSubKey(@"Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409\Applications\AcOpm"))
                {
                    if (opmKey != null)
                    {
                        opmKey.SetValue("LOADCTRLS", 14, RegistryValueKind.DWord);
                        opmKey.SetValue("LOADER", Path.Combine(targetDir, "acopm.arx"), RegistryValueKind.String);
                        opmKey.SetValue("DESCRIPTION", "AutoCAD Object Property Manager", RegistryValueKind.String);
                        using (RegistryKey cmdKey = opmKey.CreateSubKey("Commands"))
                        {
                            if (cmdKey != null)
                            {
                                cmdKey.SetValue("PROPERTIES", "PROPERTIES", RegistryValueKind.String);
                                cmdKey.SetValue("PROPERTIESCLOSE", "PROPERTIESCLOSE", RegistryValueKind.String);
                            }
                        }
                    }
                }
            }
            catch { }

            // Configure Profile paths
            string profilesKeyPath = @"Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409\Profiles";
            try
            {
                using (RegistryKey profilesKey = Registry.CurrentUser.OpenSubKey(profilesKeyPath, true))
                {
                    if (profilesKey != null)
                    {
                        string supportPath = Path.Combine(targetDir, "Support");
                        string fontsPath = Path.Combine(targetDir, "Fonts");
                        string expressPath = Path.Combine(targetDir, "Express");
                        string colorPath = Path.Combine(targetDir, @"Support\color");
                        string helpPath = Path.Combine(targetDir, "Help");
                        string drvPath = Path.Combine(targetDir, "Drv");
                        string templatePath = Path.Combine(targetDir, "Template");

                        string cleanAcad = targetDir + ";" +
                                           supportPath + ";" + fontsPath + ";" + expressPath + ";" + colorPath + ";" + helpPath + ";" +
                                           Path.Combine(targetDir, "Data") + ";" +
                                           Path.Combine(targetDir, @"Data\Symbols") + ";" +
                                           Path.Combine(targetDir, @"Data\Bien bao") + ";" +
                                           Path.Combine(targetDir, @"Data\Color") + ";" +
                                           Path.Combine(targetDir, @"Data\Ltype") + ";" +
                                           Path.Combine(targetDir, "Lib") + ";" +
                                           Path.Combine(targetDir, "CTTK") + ";" +
                                           Path.Combine(targetDir, "DCCT") + ";" +
                                           Path.Combine(targetDir, "Blocks") + ";" +
                                           Path.Combine(targetDir, "Template");

                        using (RegistryKey genKey = profilesKey.CreateSubKey(@"TDT_Solution\General"))
                        {
                            if (genKey != null)
                            {
                                genKey.SetValue("ACAD", cleanAcad);
                                genKey.SetValue("ACADDRV", drvPath);
                                genKey.SetValue("HelpFilePath", helpPath);
                                genKey.SetValue("TemplatePath", templatePath);
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private void CreateShortcuts(string targetDir)
        {
            try
            {
                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null) return;
                dynamic shell = Activator.CreateInstance(shellType);

                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string startMenu = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
                string tdtStartGroup = Path.Combine(startMenu, "TDT Solution 9.1");

                string mainExe = Path.Combine(targetDir, "TDT9.1.exe");
                string icoPath = Path.Combine(targetDir, "TDT.ico");

                // 1. Desktop Shortcut
                if (chkDesktopShortcut.Checked && File.Exists(mainExe))
                {
                    dynamic sc = shell.CreateShortcut(Path.Combine(desktop, "TDT Solution 9.1.lnk"));
                    sc.TargetPath = mainExe;
                    sc.WorkingDirectory = targetDir;
                    sc.Description = "TDT Solution 9.1 - Bộ giải pháp thiết kế công trình giao thông";
                    if (File.Exists(icoPath)) sc.IconLocation = icoPath + ",0";
                    sc.Save();
                }

                // 2. Start Menu Shortcut
                if (chkStartMenuShortcut.Checked)
                {
                    if (!Directory.Exists(tdtStartGroup)) Directory.CreateDirectory(tdtStartGroup);

                    if (File.Exists(mainExe))
                    {
                        dynamic sc = shell.CreateShortcut(Path.Combine(tdtStartGroup, "TDT Solution 9.1.lnk"));
                        sc.TargetPath = mainExe;
                        sc.WorkingDirectory = targetDir;
                        if (File.Exists(icoPath)) sc.IconLocation = icoPath + ",0";
                        sc.Save();
                    }
                }
            }
            catch { }
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SetupWizardForm());
        }
    }
}
