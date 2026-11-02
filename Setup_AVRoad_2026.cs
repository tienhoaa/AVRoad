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

namespace AVRoadInstaller
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
        private CheckBox chkSettingShortcut;
        private CheckBox chkStartMenuShortcut;

        // Step 3 controls
        private ProgressBar progressBar;
        private Label lblExtractStatus;
        private Label lblExtractFile;

        // Step 4 controls
        private CheckBox chkLaunchNow;

        private Image logoImg = null;

        public SetupWizardForm()
        {
            this.Text = "Cài Đặt AVRoad 2026 Solution - Hệ Sinh Thái AV BIM";
            this.Size = new Size(620, 460);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Font = new Font("Segoe UI", 9.5f);

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
                BackColor = Color.FromArgb(7, 35, 26)
            };
            pnlHeader.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (LinearGradientBrush b = new LinearGradientBrush(pnlHeader.ClientRectangle, Color.FromArgb(7, 35, 26), Color.FromArgb(17, 85, 65), LinearGradientMode.Horizontal))
                {
                    g.FillRectangle(b, pnlHeader.ClientRectangle);
                }

                using (Font f1 = new Font("Segoe UI", 14f, FontStyle.Bold))
                using (Brush br1 = new SolidBrush(Color.FromArgb(94, 234, 212)))
                {
                    g.DrawString("BỘ CÀI ĐẶT AVROAD 2026 SOLUTION", f1, br1, 20, 14);
                }
                using (Font f2 = new Font("Segoe UI", 9f))
                using (Brush br2 = new SolidBrush(Color.FromArgb(204, 251, 241)))
                {
                    g.DrawString("Chuẩn hóa dữ liệu CDE • Tối ưu thiết kế hạ tầng giao thông BIM", f2, br2, 20, 44);
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
            this.Controls.Add(pnlBottom);

            btnCancel = new Button()
            {
                Text = "Hủy Bỏ",
                Location = new Point(510, 12),
                Size = new Size(88, 34),
                BackColor = Color.FromArgb(226, 232, 240),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f)
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => { if (currentStep != 3) this.Close(); };
            pnlBottom.Controls.Add(btnCancel);

            btnNext = new Button()
            {
                Text = "Tiếp Tục >",
                Location = new Point(410, 12),
                Size = new Size(95, 34),
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnNext.FlatAppearance.BorderSize = 0;
            btnNext.Click += (s, e) => NextStep();
            pnlBottom.Controls.Add(btnNext);

            btnBack = new Button()
            {
                Text = "< Quay Lại",
                Location = new Point(310, 12),
                Size = new Size(95, 34),
                BackColor = Color.FromArgb(226, 232, 240),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f),
                Enabled = false
            };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Click += (s, e) => PrevStep();
            pnlBottom.Controls.Add(btnBack);

            // Content Panel
            pnlContent = new Panel()
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24, 18, 24, 18)
            };
            this.Controls.Add(pnlContent);
            pnlContent.BringToFront();
        }

        private void ShowStep(int step)
        {
            currentStep = step;
            pnlContent.Controls.Clear();

            btnBack.Enabled = (step > 0 && step < 3);
            btnCancel.Enabled = (step != 3);

            if (step == 0) // Welcome
            {
                btnNext.Text = "Bắt Đầu >";

                Label lblWelcome = new Label()
                {
                    Text = "Chào mừng bạn đến với trình cài đặt AVRoad 2026 Solution!",
                    Location = new Point(20, 20),
                    Size = new Size(540, 30),
                    Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(15, 23, 42)
                };

                Label lblDesc = new Label()
                {
                    Text = "Chương trình sẽ tự động thiết lập bộ công cụ AVRoad 2026 Solution trên máy tính của bạn.\n\n" +
                           "• Nhân đồ họa AVCAD Engine độc lập, siêu tốc độ.\n" +
                           "• Đầy đủ 7 Menu thiết kế: Khảo sát (AV_KS), Tuyến (AV_Road), San nền, Kênh, Địa chất, 3D tuyến, Vạch sơn.\n" +
                           "• Thanh công cụ chuyên nghiệp với 22 icon nhận diện mới.\n" +
                           "• Cơ chế cập nhật bản vá trực tuyến (Hotfix) thông minh từ GitHub.\n\n" +
                           "Nhấn 'Bắt Đầu' để tiến hành cài đặt.",
                    Location = new Point(20, 60),
                    Size = new Size(540, 200),
                    Font = new Font("Segoe UI", 9.5f),
                    ForeColor = Color.FromArgb(51, 65, 85)
                };

                pnlContent.Controls.AddRange(new Control[] { lblWelcome, lblDesc });
            }
            else if (step == 1) // Choose Destination
            {
                btnNext.Text = "Tiếp Tục >";

                Label lblTitle = new Label()
                {
                    Text = "Chọn Thư Mục Cài Đặt",
                    Location = new Point(20, 20),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(15, 23, 42)
                };

                Label lblSub = new Label()
                {
                    Text = "Thư mục mặc định được khuyến nghị là C:\\AVRoad_2026 để tối ưu tốc độ và quyền truy cập.",
                    Location = new Point(20, 50),
                    Size = new Size(540, 35),
                    Font = new Font("Segoe UI", 9f),
                    ForeColor = Color.FromArgb(100, 116, 139)
                };

                txtInstallDir = new TextBox()
                {
                    Text = @"C:\AVRoad_2026",
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
                        fbd.Description = "Chọn thư mục cài đặt AVRoad 2026";
                        if (Directory.Exists(txtInstallDir.Text)) fbd.SelectedPath = txtInstallDir.Text;
                        if (fbd.ShowDialog() == DialogResult.OK)
                        {
                            txtInstallDir.Text = fbd.SelectedPath;
                        }
                    }
                };

                Label lblSpace = new Label()
                {
                    Text = "Dung lượng yêu cầu: ~310 MB\nDung lượng ổ đĩa khả dụng: Đầy đủ",
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
                    Text = "Tùy Chọn Phím Tắt & Khởi Chạy",
                    Location = new Point(20, 20),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(15, 23, 42)
                };

                chkDesktopShortcut = new CheckBox()
                {
                    Text = "Tạo biểu tượng AVRoad 2026 trên màn hình Desktop",
                    Location = new Point(24, 65),
                    AutoSize = true,
                    Checked = true,
                    Font = new Font("Segoe UI", 9.5f)
                };

                chkSettingShortcut = new CheckBox()
                {
                    Text = "Tạo biểu tượng AVRoad Settings (Cấu hình) trên Desktop",
                    Location = new Point(24, 105),
                    AutoSize = true,
                    Checked = true,
                    Font = new Font("Segoe UI", 9.5f)
                };

                chkStartMenuShortcut = new CheckBox()
                {
                    Text = "Tạo nhóm phím tắt trong Start Menu",
                    Location = new Point(24, 145),
                    AutoSize = true,
                    Checked = true,
                    Font = new Font("Segoe UI", 9.5f)
                };

                pnlContent.Controls.AddRange(new Control[] { lblTitle, chkDesktopShortcut, chkSettingShortcut, chkStartMenuShortcut });
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

                // Launch background extraction thread
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
                    Text = "Cài Đặt AVRoad 2026 Thành Công!",
                    Location = new Point(20, 20),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 12.5f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(16, 185, 129)
                };

                Label lblDesc = new Label()
                {
                    Text = "Bộ phần mềm AVRoad 2026 Solution đã được cài đặt hoàn tất vào thư mục:\n" + txtInstallDir.Text + "\n\n" +
                           "Các biểu tượng khởi chạy đã được tạo thành công trên Desktop và Start Menu.\n" +
                           "Bạn có thể bắt đầu sử dụng ngay lập tức.",
                    Location = new Point(20, 60),
                    Size = new Size(540, 110),
                    Font = new Font("Segoe UI", 9.5f),
                    ForeColor = Color.FromArgb(51, 65, 85)
                };

                chkLaunchNow = new CheckBox()
                {
                    Text = "Khởi chạy AVRoad 2026 ngay bây giờ",
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
                    string targetExe = Path.Combine(txtInstallDir.Text.Trim(), "AVRoad.exe");
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
                            Path.Combine(baseDir, "AVRoad_2026_Portable.zip"),
                            @"C:\TDT_RESEARCH\AVRoad_2026_Portable.zip"
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

                                if (currentEntry % 20 == 0 || currentEntry == totalEntries)
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

                    // Fix paths in targetDir\AVRoad.ini
                    string iniPath = Path.Combine(targetDir, "AVRoad.ini");
                    if (File.Exists(iniPath))
                    {
                        try
                        {
                            string[] lines = File.ReadAllLines(iniPath);
                            for (int i = 0; i < lines.Length; i++)
                            {
                                if (lines[i].StartsWith("PlotStylePath=")) lines[i] = "PlotStylePath=" + Path.Combine(targetDir, "Plot Styles");
                                if (lines[i].StartsWith("PlotterPath=")) lines[i] = "PlotterPath=" + Path.Combine(targetDir, "Plotters");
                                if (lines[i].StartsWith("ArgProfilePath=")) lines[i] = "ArgProfilePath=" + Path.Combine(targetDir, "AVRoad_Profile.arg");
                            }
                            File.WriteAllLines(iniPath, lines);
                        }
                        catch { }
                    }

                    // Auto-install VC++ 2005 runtime if needed (fixes Side-by-Side error on clean machines)
                    string vcRedist = Path.Combine(targetDir, "vcredist_x86.exe");
                    if (File.Exists(vcRedist))
                    {
                        try
                        {
                            this.BeginInvoke((Action)(() =>
                            {
                                if (lblExtractStatus != null) lblExtractStatus.Text = "Đang tối ưu cấu hình thư viện Microsoft C++ Runtime...";
                            }));
                            ProcessStartInfo psiVc = new ProcessStartInfo(vcRedist, "/q");
                            psiVc.UseShellExecute = false;
                            psiVc.CreateNoWindow = true;
                            Process pVc = Process.Start(psiVc);
                            if (pVc != null) pVc.WaitForExit(30000);
                        }
                        catch { }
                    }

                    // Create Shortcuts and Register Uninstall
                    CreateShortcuts(targetDir);
                    RegisterWindowsUninstall(targetDir);

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

        private void CreateShortcuts(string targetDir)
        {
            try
            {
                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null) return;
                dynamic shell = Activator.CreateInstance(shellType);

                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string startMenu = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
                string avStartGroup = Path.Combine(startMenu, "AVRoad 2026 Solution");

                string mainExe = Path.Combine(targetDir, "AVRoad.exe");
                string settingExe = Path.Combine(targetDir, "AVSetting.exe");
                string icoPath = Path.Combine(targetDir, "AVRoad.ico");

                // 1. Desktop Shortcuts
                if (chkDesktopShortcut.Checked && File.Exists(mainExe))
                {
                    dynamic sc = shell.CreateShortcut(Path.Combine(desktop, "AVRoad 2026.lnk"));
                    sc.TargetPath = mainExe;
                    sc.WorkingDirectory = targetDir;
                    sc.Description = "AVRoad 2026 Solution - Hệ sinh thái AV BIM CDE";
                    if (File.Exists(icoPath)) sc.IconLocation = icoPath + ",0";
                    sc.Save();
                }

                if (chkSettingShortcut.Checked && File.Exists(settingExe))
                {
                    dynamic sc = shell.CreateShortcut(Path.Combine(desktop, "AVRoad Settings.lnk"));
                    sc.TargetPath = settingExe;
                    sc.WorkingDirectory = targetDir;
                    sc.Description = "AVRoad 2026 Settings - Trung tâm cấu hình";
                    if (File.Exists(icoPath)) sc.IconLocation = icoPath + ",0";
                    sc.Save();
                }

                // 2. Start Menu Shortcuts
                if (chkStartMenuShortcut.Checked)
                {
                    if (!Directory.Exists(avStartGroup)) Directory.CreateDirectory(avStartGroup);

                    if (File.Exists(mainExe))
                    {
                        dynamic sc = shell.CreateShortcut(Path.Combine(avStartGroup, "AVRoad 2026.lnk"));
                        sc.TargetPath = mainExe;
                        sc.WorkingDirectory = targetDir;
                        if (File.Exists(icoPath)) sc.IconLocation = icoPath + ",0";
                        sc.Save();
                    }

                    if (File.Exists(settingExe))
                    {
                        dynamic sc = shell.CreateShortcut(Path.Combine(avStartGroup, "AVRoad Settings.lnk"));
                        sc.TargetPath = settingExe;
                        sc.WorkingDirectory = targetDir;
                        if (File.Exists(icoPath)) sc.IconLocation = icoPath + ",0";
                        sc.Save();
                    }

                    string uninstExe = Path.Combine(targetDir, "Uninstall.exe");
                    if (File.Exists(uninstExe))
                    {
                        dynamic sc = shell.CreateShortcut(Path.Combine(avStartGroup, "Gỡ Cài Đặt AVRoad 2026.lnk"));
                        sc.TargetPath = uninstExe;
                        sc.WorkingDirectory = targetDir;
                        if (File.Exists(icoPath)) sc.IconLocation = icoPath + ",0";
                        sc.Save();
                    }
                }
            }
            catch { }
        }

        private void RegisterWindowsUninstall(string targetDir)
        {
            try
            {
                string uninstExe = Path.Combine(targetDir, "Uninstall.exe");
                using (RegistryKey uKey = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\AVRoad2026"))
                {
                    if (uKey != null)
                    {
                        uKey.SetValue("DisplayName", "AVRoad 2026 Solution");
                        uKey.SetValue("DisplayVersion", "1.0");
                        uKey.SetValue("Publisher", "AV Technology CDE");
                        uKey.SetValue("UninstallString", "\"" + uninstExe + "\"");
                        uKey.SetValue("DisplayIcon", Path.Combine(targetDir, "AVRoad.ico"));
                        uKey.SetValue("InstallLocation", targetDir);
                        uKey.SetValue("EstimatedSize", 314000, RegistryValueKind.DWord);
                    }
                }
            }
            catch { }
        }

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SetupWizardForm());
        }
    }
}
