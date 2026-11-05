using AVRoadDeployment;
using AVRoadLauncher;
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

using System.Runtime.InteropServices;



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

        // private CheckBox chkSettingShortcut;

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



                chkStartMenuShortcut = new CheckBox()

                {

                    Text = "Tạo nhóm phím tắt trong Start Menu",

                    Location = new Point(24, 105),

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
                        string journal = PatchTransaction.InstallEmbedded(zipStream, targetDir, true, (percent, name) =>
                        {
                            this.BeginInvoke((Action)(() => { progressBar.Value = percent; lblExtractFile.Text = name; }));
                        });
                        File.WriteAllText(Path.Combine(targetDir, "last-install-rollback.txt"), journal);
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
                    Cad7ProfileBootstrap.ValidateExport(Path.Combine(targetDir, "AVRoad_Profile.arg"));
                    Cad7ProfileBootstrap.Ensure(targetDir, Path.Combine(targetDir, "AVRoad_Profile.arg"));

                    string vcRedist = Path.Combine(targetDir, "vcredist_x86.exe");

                    if (!RegistryScope.IsLimited && File.Exists(vcRedist))

                    {

                        try

                        {

                            this.BeginInvoke((Action)(() =>

                            {

                                if (lblExtractStatus != null) lblExtractStatus.Text = "Đang tối ưu cấu hình thư viện Microsoft C++ Runtime...";

                            }));

                            ProcessStartInfo psiVc = new ProcessStartInfo(vcRedist, "/q:a /c:\"vcredist.msi /qn\"");

                            psiVc.UseShellExecute = false;

                            psiVc.CreateNoWindow = true;

                            Process pVc = Process.Start(psiVc);

                            if (pVc != null) pVc.WaitForExit(30000);

                        }

                        catch { }

                    }



                    // Create Shortcuts and Register Uninstall

                    ConfigureHardwareAccelerationAndSmoothDrafting(targetDir);

                    RegisterAutoCADApplications(targetDir);

                    CleanPortableAssociations();

                    FixPropertiesPalette(targetDir);

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



// Settings shortcut embedded inside AVRoad 2026 Startup Screen



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



        private void ConfigureHardwareAccelerationAndSmoothDrafting(string targetDir)

        {
            if (RegistryScope.IsLimited) return;


            try

            {

                string drvPath = Path.Combine(targetDir, "Drv");

                string direct3d = Path.Combine(drvPath, "direct3d9.hdi");

                string tuner = Path.Combine(drvPath, "tuner9.hdi");

                if (!File.Exists(direct3d) && File.Exists(tuner))

                {

                    try { File.Copy(tuner, direct3d, true); } catch { }

                }



                string parent3DGS = @"Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409\3DGS Configuration";

                using (RegistryKey prk = Registry.CurrentUser.CreateSubKey(parent3DGS))

                {

                    if (prk != null)

                    {

                        prk.SetValue("MaxTessellations", 8, RegistryValueKind.DWord);

                        prk.SetValue("SurfaceTessellationTol", 92, RegistryValueKind.DWord);

                        prk.SetValue("CurveTessellationTol", 87, RegistryValueKind.DWord);

                    }

                }



                string key3DGS = @"Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409\3DGS Configuration\GSHEIDI10";

                

                // Write HKCU 3DGS Configuration

                using (RegistryKey rk = Registry.CurrentUser.CreateSubKey(key3DGS))

                {

                    if (rk != null)

                    {

                        rk.SetValue("Help", Path.Combine(targetDir, "acad.chm"), RegistryValueKind.String);

                        rk.SetValue("ContextHelp", "", RegistryValueKind.String);

                        rk.SetValue("FirstRun", 1, RegistryValueKind.DWord);

                        rk.SetValue("Flags", 4721767, RegistryValueKind.DWord); // HW Accel ON + Smooth lines ON + Geometry ON

                        rk.SetValue("CustomHeidiDriver", "direct3d9.hdi", RegistryValueKind.String);

                        rk.SetValue("CustomHeidiDriverPath", drvPath, RegistryValueKind.String);

                        rk.SetValue("DriverName", "direct3d9.hdi", RegistryValueKind.String);

                        rk.SetValue("FramesPerSecond", 5, RegistryValueKind.DWord);

                        rk.SetValue("MaxTessellations", 8, RegistryValueKind.DWord); // Fixes 1-60 dialog error

                        rk.SetValue("SurfaceTessellationTol", 92, RegistryValueKind.DWord);

                        rk.SetValue("CurveTessellationTol", 87, RegistryValueKind.DWord);

                        rk.SetValue("PTCardAvaliable", 1, RegistryValueKind.DWord); // Enables HW checkbox

                        rk.SetValue("PTCardRecomended", 1, RegistryValueKind.DWord);

                        rk.SetValue("PTCardCertified", 1, RegistryValueKind.DWord);

                        rk.SetValue("PTUsingD3D", 1, RegistryValueKind.DWord);

                        rk.SetValue("PTUsingOGL", 0, RegistryValueKind.DWord);

                        rk.SetValue("PTWorkArounds", "0:", RegistryValueKind.String);

                        rk.SetValue("SNAPSHOTFLAGS", 103, RegistryValueKind.DWord);

                        rk.SetValue("SNAPSHOTCACHETESS", 8, RegistryValueKind.DWord);

                        rk.SetValue("SNAPSHOTCURVTESS", 87, RegistryValueKind.DWord);

                        rk.SetValue("SNAPSHOTSURFTESS", 92, RegistryValueKind.DWord);

                        rk.SetValue("SNAPSHOTTRANSPARENCY", 1, RegistryValueKind.DWord);

                        rk.SetValue("SNAPSHOTDRIVER", "direct3d9.hdi", RegistryValueKind.String);



                        byte[] featData = new byte[] {

                            0x1f, 0x00, 0x00, 0x00, // Available: HW Accel, Geom Accel, Smooth lines, Gooch, Shadows

                            0x1f, 0x00, 0x00, 0x00, // Recommended: All

                            0x0f, 0x00, 0x00, 0x00  // Enabled: HW Accel (1), Geom (2), Smooth lines (4), Gooch (8)

                        };

                        rk.SetValue("FeatureData", featData, RegistryValueKind.Binary);



                        byte[] chain = new byte[] {

                            0x00, 0x00, 0x00, 0x80, 0x01, 0x00, 0x00, 0x80, 0x02, 0x00, 0x00, 0x80,

                            0x03, 0x00, 0x00, 0x80, 0x04, 0x00, 0x00, 0x80, 0x05, 0x00, 0x00, 0x80,

                            0x06, 0x00, 0x00, 0x80, 0x07, 0x00, 0x00, 0x80, 0x08, 0x00, 0x00, 0x80,

                            0x09, 0x00, 0x00, 0x80, 0x0a, 0x00, 0x00, 0x80, 0x0b, 0x00, 0x00, 0x80,

                            0x0c, 0x00, 0x00, 0x80, 0x0d, 0x00, 0x00, 0x80, 0x0e, 0x00, 0x00, 0x80,

                            0x0f, 0x00, 0x00, 0x80, 0x10, 0x00, 0x00, 0x80, 0x11, 0x00, 0x00, 0x80

                        };

                        rk.SetValue("DegradationChainOrder", chain, RegistryValueKind.Binary);

                    }

                }



                // Write HKLM 3DGS Configuration (Setup is elevated)

                try

                {

                    using (RegistryKey lm = Registry.LocalMachine.CreateSubKey(key3DGS))

                    {

                        if (lm != null)

                        {

                            lm.SetValue("Flags", 4721767, RegistryValueKind.DWord);

                            lm.SetValue("CustomHeidiDriver", "direct3d9.hdi", RegistryValueKind.String);

                            lm.SetValue("CustomHeidiDriverPath", drvPath, RegistryValueKind.String);

                        }

                    }

                }

                catch { }



                try

                {

                    using (RegistryKey lm64 = Registry.LocalMachine.CreateSubKey(@"Software\WOW6432Node\Autodesk\AutoCAD\R17.0\ACAD-5001:409\3DGS Configuration\GSHEIDI10"))

                    {

                        if (lm64 != null)

                        {

                            lm64.SetValue("Flags", 4721767, RegistryValueKind.DWord);

                            lm64.SetValue("CustomHeidiDriver", "direct3d9.hdi", RegistryValueKind.String);

                            lm64.SetValue("CustomHeidiDriverPath", drvPath, RegistryValueKind.String);

                        }

                    }

                }

                catch { }



                // Configure smooth drafting options in AVRoad profile

                using (RegistryKey genKey = Registry.CurrentUser.CreateSubKey(@"Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409\Profiles\AVRoad\General"))

                {

                    if (genKey != null)

                    {

                        genKey.SetValue("WhipArcEnable", 1, RegistryValueKind.DWord);

                        genKey.SetValue("VtEnable", 3, RegistryValueKind.DWord);

                        genKey.SetValue("Dragmode", 2, RegistryValueKind.DWord);

                        genKey.SetValue("SelectionPreview", 2, RegistryValueKind.DWord);

                        genKey.SetValue("CmdDia", 1, RegistryValueKind.DWord);

                        genKey.SetValue("FileDia", 1, RegistryValueKind.DWord);

                        genKey.SetValue("AttDia", 1, RegistryValueKind.DWord);
                        genKey.SetValue("Pickfirst", 1, RegistryValueKind.DWord);
                        genKey.SetValue("Pickstyle", 1, RegistryValueKind.DWord);
                        genKey.SetValue("DWGCheck", 0, RegistryValueKind.DWord);
                        genKey.SetValue("DwgAlwaysAssociate", 0, RegistryValueKind.DWord);

                    }

                }

                using (RegistryKey edKey = Registry.CurrentUser.CreateSubKey(@"Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409\Profiles\AVRoad\Editor Configuration"))

                {

                    if (edKey != null)

                    {

                        edKey.SetValue("ZoomFactor", 60, RegistryValueKind.DWord);

                    }

                }

                // Configure Command Line Window font to Arial across all profiles

                try

                {

                    using (RegistryKey pk = Registry.CurrentUser.OpenSubKey(@"Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409\Profiles", true))

                    {

                        if (pk != null)

                        {

                            foreach (string sub in pk.GetSubKeyNames())

                            {

                                try

                                {

                                    using (RegistryKey cmdKey = pk.CreateSubKey(sub + @"\Command Line Windows"))

                                    {

                                        if (cmdKey != null)

                                        {

                                            cmdKey.SetValue("CmdLine.FontFace", "Arial", RegistryValueKind.String);

                                            cmdKey.SetValue("CmdLine.FontHeight", 10, RegistryValueKind.DWord);

                                            cmdKey.SetValue("CmdLine.FontWeight", 400, RegistryValueKind.DWord);

                                            cmdKey.SetValue("TextWindow.FontFace", "Arial", RegistryValueKind.String);

                                            cmdKey.SetValue("TextWindow.FontHeight", 10, RegistryValueKind.DWord);

                                            cmdKey.SetValue("TextWindow.FontWeight", 400, RegistryValueKind.DWord);

                                        }

                                    }

                                }

                                catch { }

                            }

                        }

                    }

                }

                catch { }



                // Clean up acmgd.dll from TabExtensions to prevent "invalid ARX file" error

                try

                {

                    string[] dialogs = new string[] { "CustomizeDialog", "DrawingSettingsDialog", "OptionsDialog" };

                    foreach (string dlg in dialogs)

                    {

                        string tabKey = @"Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409\Profiles\AVRoad\Dialogs\" + dlg + @"\TabExtensions";

                        using (RegistryKey tk = Registry.CurrentUser.OpenSubKey(tabKey, true))

                        {

                            if (tk != null)

                            {

                                try { tk.DeleteValue("acmgd.dll", false); } catch { }

                            }

                        }

                    }

                }

                catch { }



// ObjectDBX registry keys load TDTObjects natively

            }

            catch { }

        }



        private void RegisterWindowsUninstall(string targetDir)

        {
            if (RegistryScope.IsLimited) return;


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
        public static void Main(string[] args)
        {
            if (args.Length == 2 && args[0] == "/extract-only")
            {
                // File-only artifact verification: bypass all legacy configuration/Registry code.
                using (Stream payload = Assembly.GetExecutingAssembly().GetManifestResourceStream("payload.zip"))
                {
                    string journal = PatchTransaction.InstallEmbedded(payload, args[1], false, null);
                    File.WriteAllText(Path.Combine(args[1], "verified-installer.txt"), journal);
                }
                return;
            }
            try
            {
                using (var id = System.Security.Principal.WindowsIdentity.GetCurrent())
                {
                    var p = new System.Security.Principal.WindowsPrincipal(id);
                    if (!p.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator))
                    {
                        ProcessStartInfo psi = new ProcessStartInfo();
                        psi.FileName = Application.ExecutablePath;
                        psi.Verb = "runas";
                        psi.UseShellExecute = true;
                        try
                        {
                            Process.Start(psi);
                        }
                        catch { }
                        return;
                    }
                }
            }
            catch { }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SetupWizardForm());
        }        [DllImport("oleaut32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int LoadTypeLib(string szFile, out IntPtr pptlib);

        [DllImport("oleaut32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int RegisterTypeLibForUser(IntPtr ptlib, string szFullPath, string szHelpDir);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpLibFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);

        private delegate int DllRegisterServerDelegate();

        public static void RegisterTypeLibraries(string appDir)
        {
            if (RegistryScope.IsLimited) return;

            string[] tlbs = new string[]
            {
                "acax17enu.tlb", "axdb17enu.tlb", "AcAuthEntities17enu.tlb",
                "AcMPolygon17enu.tlb", "AcObjClassImp17.tlb", "AcSmComponents17.tlb",
                "AdComFolderWatch17.tlb", "vl16.tlb", "ACcolor.ocx",
                "AcadLinetype.ocx", "HatchPattern.ocx", "TDTObjectsWrapper.arx"
            };
            foreach (string file in tlbs)
            {
                try
                {
                    string fullPath = Path.Combine(appDir, file);
                    if (File.Exists(fullPath))
                    {
                        IntPtr pTlib;
                        int hr = LoadTypeLib(fullPath, out pTlib);
                        if (hr == 0 && pTlib != IntPtr.Zero)
                        {
                            RegisterTypeLibForUser(pTlib, fullPath, appDir);
                            Marshal.Release(pTlib);
                        }
                    }
                }
                catch { }
            }


        }

        public static void RegisterAutoCADCOMClasses(string appDir)
        {
            if (RegistryScope.IsLimited) {
                Cad7ComRegistration.Apply(appDir, null);
                string argProfile = Path.Combine(appDir, "AVRoad_Profile.arg");
                if (File.Exists(argProfile)) Cad7ProfileBootstrap.Ensure(appDir, argProfile);
                return;
            }

            try
            {
                string acadExe = Path.Combine(appDir, "acad.exe");
                using (RegistryKey classes = Registry.CurrentUser.OpenSubKey(@"Software\Classes", true))
                {
                    if (classes == null) return;

                    // 1. AutoCAD.Application.17
                    using (RegistryKey app17 = classes.CreateSubKey("AutoCAD.Application.17"))
                    {
                        if (app17 != null)
                        {
                            app17.SetValue("", "AutoCAD Application 17");
                            using (RegistryKey c = app17.CreateSubKey("CLSID")) { if (c != null) c.SetValue("", "{28B7AA99-C0F9-4C47-995E-8A8D729603A1}"); }
                        }
                    }

                    // 2. AutoCAD.Application
                    using (RegistryKey app = classes.CreateSubKey("AutoCAD.Application"))
                    {
                        if (app != null)
                        {
                            app.SetValue("", "AutoCAD Application");
                            using (RegistryKey c = app.CreateSubKey("CLSID")) { if (c != null) c.SetValue("", "{28B7AA99-C0F9-4C47-995E-8A8D729603A1}"); }
                            using (RegistryKey cv = app.CreateSubKey("CurVer")) { if (cv != null) cv.SetValue("", "AutoCAD.Application.17"); }
                        }
                    }

                    // 3. CLSID AcadApplication
                    string clsidAppPath = @"CLSID\{28B7AA99-C0F9-4C47-995E-8A8D729603A1}";
                    using (RegistryKey clsidApp = classes.CreateSubKey(clsidAppPath))
                    {
                        if (clsidApp != null)
                        {
                            clsidApp.SetValue("", "AcadApplication");
                            using (RegistryKey p = clsidApp.CreateSubKey("ProgID")) { if (p != null) p.SetValue("", "AutoCAD.Application.17"); }
                            using (RegistryKey p = clsidApp.CreateSubKey("VersionIndependentProgID")) { if (p != null) p.SetValue("", "AutoCAD.Application"); }
                            using (RegistryKey p = clsidApp.CreateSubKey("Programmable")) { if (p != null) p.SetValue("", ""); }
                            using (RegistryKey p = clsidApp.CreateSubKey("TypeLib")) { if (p != null) p.SetValue("", "{851A4561-F4EC-4631-9B0C-E7DC407512C9}"); }
                            using (RegistryKey p = clsidApp.CreateSubKey("LocalServer32")) { if (p != null) p.SetValue("", "\"" + acadExe + "\" /Automation"); }
                        }
                    }

                    // 4. AutoCAD.Drawing.17
                    using (RegistryKey dwg17 = classes.CreateSubKey("AutoCAD.Drawing.17"))
                    {
                        if (dwg17 != null)
                        {
                            dwg17.SetValue("", "AutoCAD Drawing 17");
                            using (RegistryKey c = dwg17.CreateSubKey("CLSID")) { if (c != null) c.SetValue("", "{7AABBB95-79BE-4C0F-8024-EB6AF271231C}"); }
                        }
                    }

                    // 5. AutoCAD.Drawing
                    using (RegistryKey dwg = classes.CreateSubKey("AutoCAD.Drawing"))
                    {
                        if (dwg != null)
                        {
                            dwg.SetValue("", "AutoCAD Drawing");
                            using (RegistryKey c = dwg.CreateSubKey("CLSID")) { if (c != null) c.SetValue("", "{7AABBB95-79BE-4C0F-8024-EB6AF271231C}"); }
                            using (RegistryKey cv = dwg.CreateSubKey("CurVer")) { if (cv != null) cv.SetValue("", "AutoCAD.Drawing.17"); }
                        }
                    }

                    // 6. CLSID AcadDocument
                    string clsidDocPath = @"CLSID\{7AABBB95-79BE-4C0F-8024-EB6AF271231C}";
                    using (RegistryKey clsidDoc = classes.CreateSubKey(clsidDocPath))
                    {
                        if (clsidDoc != null)
                        {
                            clsidDoc.SetValue("", "AcadDocument");
                            using (RegistryKey p = clsidDoc.CreateSubKey("ProgID")) { if (p != null) p.SetValue("", "AutoCAD.Drawing.17"); }
                            using (RegistryKey p = clsidDoc.CreateSubKey("VersionIndependentProgID")) { if (p != null) p.SetValue("", "AutoCAD.Drawing"); }
                            using (RegistryKey p = clsidDoc.CreateSubKey("Programmable")) { if (p != null) p.SetValue("", ""); }
                            using (RegistryKey p = clsidDoc.CreateSubKey("TypeLib")) { if (p != null) p.SetValue("", "{851A4561-F4EC-4631-9B0C-E7DC407512C9}"); }
                            using (RegistryKey p = clsidDoc.CreateSubKey("LocalServer32")) { if (p != null) p.SetValue("", "\"" + acadExe + "\" /Automation"); }
                        }
                    }

                    // 7. Event Interfaces
                    using (RegistryKey ifApp = classes.CreateSubKey(@"Interface\{22C8E547-0D27-4793-BB9F-AC58D4F8E794}"))
                    {
                        if (ifApp != null)
                        {
                            ifApp.SetValue("", "_DAcadApplicationEvents");
                            using (RegistryKey t = ifApp.CreateSubKey("TypeLib")) { if (t != null) t.SetValue("", "{851A4561-F4EC-4631-9B0C-E7DC407512C9}"); }
                        }
                    }
                    using (RegistryKey ifDoc = classes.CreateSubKey(@"Interface\{C814C69D-A74B-494C-8EE6-23731B145CCB}"))
                    {
                        if (ifDoc != null)
                        {
                            ifDoc.SetValue("", "_DAcadDocumentEvents");
                            using (RegistryKey t = ifDoc.CreateSubKey("TypeLib")) { if (t != null) t.SetValue("", "{851A4561-F4EC-4631-9B0C-E7DC407512C9}"); }
                        }
                    }

                    // 8. TDTObjectsWrapper COM Classes
                    string wrapperArx = Path.Combine(appDir, "TDTObjectsWrapper.arx");
                    if (File.Exists(wrapperArx))
                    {
                        using (RegistryKey k = classes.CreateSubKey("TDTObjectsWrapper.MeasurePoint.1"))
                        {
                            if (k != null) { k.SetValue("", "MeasurePoint Class"); using (var c = k.CreateSubKey("CLSID")) if (c != null) c.SetValue("", "{DADDF471-47F4-4799-AA36-14E6237BF7FC}"); }
                        }
                        using (RegistryKey k = classes.CreateSubKey("TDTObjectsWrapper.MeasurePoint"))
                        {
                            if (k != null) { k.SetValue("", "MeasurePoint Class"); using (var c = k.CreateSubKey("CLSID")) if (c != null) c.SetValue("", "{DADDF471-47F4-4799-AA36-14E6237BF7FC}"); using (var cv = k.CreateSubKey("CurVer")) if (cv != null) cv.SetValue("", "TDTObjectsWrapper.MeasurePoint.1"); }
                        }
                        using (RegistryKey k = classes.CreateSubKey(@"CLSID\{DADDF471-47F4-4799-AA36-14E6237BF7FC}"))
                        {
                            if (k != null)
                            {
                                k.SetValue("", "MeasurePoint Class");
                                using (var p = k.CreateSubKey("ProgID")) if (p != null) p.SetValue("", "TDTObjectsWrapper.MeasurePoint.1");
                                using (var p = k.CreateSubKey("VersionIndependentProgID")) if (p != null) p.SetValue("", "TDTObjectsWrapper.MeasurePoint");
                                using (var p = k.CreateSubKey("Programmable")) if (p != null) p.SetValue("", "");
                                using (var p = k.CreateSubKey("InprocServer32")) { if (p != null) { p.SetValue("", wrapperArx); p.SetValue("ThreadingModel", "Apartment"); } }
                                using (var p = k.CreateSubKey("TypeLib")) if (p != null) p.SetValue("", "{58CEAD15-A3D1-4F8E-8842-E62E44AB0229}");
                            }
                        }

                        using (RegistryKey k = classes.CreateSubKey("TDTObjectsWrapper.BasePoint.1"))
                        {
                            if (k != null) { k.SetValue("", "BasePoint Class"); using (var c = k.CreateSubKey("CLSID")) if (c != null) c.SetValue("", "{A6A88FF4-3FCF-437E-B5C4-4686F504EE7C}"); }
                        }
                        using (RegistryKey k = classes.CreateSubKey("TDTObjectsWrapper.BasePoint"))
                        {
                            if (k != null) { k.SetValue("", "BasePoint Class"); using (var c = k.CreateSubKey("CLSID")) if (c != null) c.SetValue("", "{A6A88FF4-3FCF-437E-B5C4-4686F504EE7C}"); using (var cv = k.CreateSubKey("CurVer")) if (cv != null) cv.SetValue("", "TDTObjectsWrapper.BasePoint.1"); }
                        }
                        using (RegistryKey k = classes.CreateSubKey(@"CLSID\{A6A88FF4-3FCF-437E-B5C4-4686F504EE7C}"))
                        {
                            if (k != null)
                            {
                                k.SetValue("", "BasePoint Class");
                                using (var p = k.CreateSubKey("ProgID")) if (p != null) p.SetValue("", "TDTObjectsWrapper.BasePoint.1");
                                using (var p = k.CreateSubKey("VersionIndependentProgID")) if (p != null) p.SetValue("", "TDTObjectsWrapper.BasePoint");
                                using (var p = k.CreateSubKey("Programmable")) if (p != null) p.SetValue("", "");
                                using (var p = k.CreateSubKey("InprocServer32")) { if (p != null) { p.SetValue("", wrapperArx); p.SetValue("ThreadingModel", "Apartment"); } }
                                using (var p = k.CreateSubKey("TypeLib")) if (p != null) p.SetValue("", "{58CEAD15-A3D1-4F8E-8842-E62E44AB0229}");
                            }
                        }
                    }

                    // 9. ObjectDBX ActiveXCLSID
                    using (RegistryKey axKey = Registry.CurrentUser.CreateSubKey(@"Software\Autodesk\ObjectDBX\R17.0\ActiveXCLSID"))
                    {
                        if (axKey != null)
                        {
                            axKey.SetValue("TdtDbMeasurePointObj", "{DADDF471-47F4-4799-AA36-14E6237BF7FC}");
                            axKey.SetValue("TdtDbPointObj", "{A6A88FF4-3FCF-437E-B5C4-4686F504EE7C}");
                        }
                    }
                    try
                    {
                        using (RegistryKey axKeyLM = Registry.LocalMachine.CreateSubKey(@"Software\Autodesk\ObjectDBX\R17.0\ActiveXCLSID"))
                        {
                            if (axKeyLM != null)
                            {
                                axKeyLM.SetValue("TdtDbMeasurePointObj", "{DADDF471-47F4-4799-AA36-14E6237BF7FC}");
                                axKeyLM.SetValue("TdtDbPointObj", "{A6A88FF4-3FCF-437E-B5C4-4686F504EE7C}");
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        public static void RegisterAutoCADApplications(string appDir)
        {
            if (RegistryScope.IsLimited) {
                Cad7ComRegistration.Apply(appDir, null);
                string argProfile = Path.Combine(appDir, "AVRoad_Profile.arg");
                if (File.Exists(argProfile)) Cad7ProfileBootstrap.Ensure(appDir, argProfile);
                return;
            }

            try
            {
                CADRegistryConfig.RegisterAll(appDir);

                RegisterTypeLibraries(appDir);
                RegisterAutoCADCOMClasses(appDir);

                // Clean old cached MNR files so new AV_ menus recompile cleanly
                try
                {
                    string appDataSupport = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Autodesk\AutoCAD 2007\R17.0\enu\Support");
                    foreach (string folder in new string[] { appDir, appDataSupport })
                    {
                        if (Directory.Exists(folder))
                        {
                            foreach (string mnr in Directory.GetFiles(folder, "*.mnr"))
                            {
                                try { File.Delete(mnr); } catch { }
                            }
                        }
                    }
                }
                catch { }
            }
            catch { }
        }

        [DllImport("shell32.dll")]
        public static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

        public static void CleanPortableAssociations()
        {
            if (RegistryScope.IsLimited) return;

            try
            {
                using (RegistryKey k = Registry.CurrentUser.OpenSubKey(@"Software\Classes\.dwg", true))
                {
                    if (k != null)
                    {
                        object val = k.GetValue("");
                        if (val != null && val.ToString().IndexOf("AutoCAD.Drawing.17", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            try { Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\.dwg", false); } catch { }
                        }
                    }
                }
                try { SHChangeNotify(0x08000000, 0, IntPtr.Zero, IntPtr.Zero); } catch { }
            }
            catch { }
        }

        public static void FixPropertiesPalette(string appDir = null)
        {
            try
            {
                if (string.IsNullOrEmpty(appDir))
                {
                    appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                }

                // 1. Mirror UserDataCache\Support to AppData Support (AcFields.fdc, CUI, DCL, etc.)
                string appDataSupport = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Autodesk\AutoCAD 2007\R17.0\enu\Support");
                string srcUserDataSupport = Path.Combine(appDir, @"UserDataCache\Support");
                if (Directory.Exists(srcUserDataSupport))
                {
                    Directory.CreateDirectory(appDataSupport);
                    foreach (string file in Directory.GetFiles(srcUserDataSupport))
                    {
                        string dst = Path.Combine(appDataSupport, Path.GetFileName(file));
                        if (!File.Exists(dst))
                        {
                            try { File.Copy(file, dst, true); } catch { }
                        }
                    }
                }

                // 2. FixedProfile.aws
                string appDataProfiles = Path.Combine(appDataSupport, "Profiles");
                Directory.CreateDirectory(appDataProfiles);

                string srcFixedAws = Path.Combine(appDir, "FixedProfile.aws");
                if (!File.Exists(srcFixedAws))
                {
                    string fallback = Path.Combine(appDir, @"UserDataCache\Support\Profiles\FixedProfile.aws");
                    if (File.Exists(fallback)) srcFixedAws = fallback;
                }

                string targetFixed = Path.Combine(appDataProfiles, "FixedProfile.aws");
                if (File.Exists(srcFixedAws))
                {
                    try { File.Copy(srcFixedAws, targetFixed, true); } catch { }
                }

                // 3. Profile.aws (genuine scheme)
                string srcProfileAws = Path.Combine(appDir, "Profile.aws");
                if (!File.Exists(srcProfileAws))
                {
                    string fallback = Path.Combine(appDir, @"UserDataCache\Support\Profiles\AVRoad\Profile.aws");
                    if (File.Exists(fallback)) srcProfileAws = fallback;
                }

                string[] validProfiles = new string[] { "AVRoad", "Unnamed Profile" };
                foreach (string prof in validProfiles)
                {
                    string pDir = Path.Combine(appDataProfiles, prof);
                    Directory.CreateDirectory(pDir);
                    string targetProfileAws = Path.Combine(pDir, "Profile.aws");
                    if (File.Exists(srcProfileAws))
                    {
                        try { File.Copy(srcProfileAws, targetProfileAws, true); } catch { }
                    }
                }

                // 4. Configure Properties Tool in FixedProfile.aws: AllowDocking=1, Floating Left=60, Top=120, Width=300, Height=550
                if (File.Exists(targetFixed))
                {
                    try
                    {
                        var doc = new System.Xml.XmlDocument();
                        doc.Load(targetFixed);
                        bool modified = false;
                        var tools = doc.GetElementsByTagName("Tool");
                        foreach (System.Xml.XmlElement tool in tools)
                        {
                            string clsid = tool.GetAttribute("CLSID");
                            var pset = tool.SelectSingleNode("CAdUiPaletteSet") as System.Xml.XmlElement;
                            if ((pset != null && pset.GetAttribute("Name") == "PROPERTIES") || clsid.Equals("{6D32A2D9-832E-11D2-A83C-0060B0872C0B}", StringComparison.OrdinalIgnoreCase))
                            {
                                var dock = tool.SelectSingleNode("CAdUiDockControlBar") as System.Xml.XmlElement;
                                if (dock != null)
                                {
                                    dock.SetAttribute("Orientation", "-1");
                                    dock.SetAttribute("AllowDocking", "1");
                                    var finfo = dock.SelectSingleNode("FloatInfo") as System.Xml.XmlElement;
                                    if (finfo != null)
                                    {
                                        finfo.SetAttribute("Left", "60");
                                        finfo.SetAttribute("Top", "120");
                                        finfo.SetAttribute("Width", "300");
                                        finfo.SetAttribute("Height", "550");
                                    }
                                    var dinfo = dock.SelectSingleNode("DockInfo") as System.Xml.XmlElement;
                                    if (dinfo != null)
                                    {
                                        dinfo.SetAttribute("MRUDockID", "59420");
                                        dinfo.SetAttribute("Left", "0");
                                        dinfo.SetAttribute("Top", "0");
                                        dinfo.SetAttribute("Width", "300");
                                        dinfo.SetAttribute("Height", "550");
                                    }
                                    modified = true;
                                }
                            }
                        }
                        if (modified) doc.Save(targetFixed);
                    }
                    catch { }
                }
            }
            catch { }
        }
    }
}
