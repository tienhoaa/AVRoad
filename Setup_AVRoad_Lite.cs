using AVRoadDeployment;
using AVRoadLauncher;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace AVRoadInstaller
{
    public class SetupLiteWizardForm : Form
    {
        private Panel pnlHeader;
        private Panel pnlContent;
        private Panel pnlBottom;
        private Button btnBack;
        private Button btnNext;
        private Button btnCancel;

        // 0: Welcome, 1: Install Dir, 2: AutoCAD 2007 Host, 3: Shortcuts, 4: Installing, 5: Finished
        private int currentStep = 0;

        // Persisted wizard state across Back/Next navigation
        private string installDir = @"C:\AVRoad_2026";
        private string cadExePath = "";
        private bool createDesktopShortcut = true;
        private bool createStartMenuShortcut = true;

        // Step 1 controls
        private TextBox txtInstallDir;

        // Step 2 controls
        private TextBox txtCadExePath;
        private Label lblCadStatus;

        // Step 3 controls
        private CheckBox chkDesktopShortcut;
        private CheckBox chkStartMenuShortcut;

        // Step 4 controls
        private ProgressBar progressBar;
        private Label lblExtractStatus;
        private Label lblExtractFile;

        // Step 5 controls
        private CheckBox chkLaunchNow;

        public SetupLiteWizardForm()
        {
            this.Text = "Cài đặt AVRoad 2026 (TDT 9.1) - Bản Lite (Dành cho máy đã cài sẵn AutoCAD 2007)";
            this.Size = new Size(650, 480);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Font = new Font("Segoe UI", 9.5f);

            try
            {
                this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch { }

            cadExePath = AutoDetectAutoCAD2007Exe() ?? "";

            BuildLayout();
            ShowStep(0);
        }

        private void BuildLayout()
        {
            pnlHeader = new Panel()
            {
                Dock = DockStyle.Top,
                Height = 82,
                BackColor = Color.FromArgb(7, 35, 26)
            };
            pnlHeader.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (LinearGradientBrush b = new LinearGradientBrush(
                    pnlHeader.ClientRectangle,
                    Color.FromArgb(7, 35, 26),
                    Color.FromArgb(17, 85, 65),
                    LinearGradientMode.Horizontal))
                {
                    g.FillRectangle(b, pnlHeader.ClientRectangle);
                }

                using (Font f1 = new Font("Segoe UI", 13.5f, FontStyle.Bold))
                using (Brush br1 = new SolidBrush(Color.FromArgb(94, 234, 212)))
                {
                    g.DrawString("CÀI ĐẶT AVROAD 2026 (TDT 9.1) - BẢN LITE", f1, br1, 20, 14);
                }
                using (Font f2 = new Font("Segoe UI", 9f))
                using (Brush br2 = new SolidBrush(Color.FromArgb(204, 251, 241)))
                {
                    g.DrawString("Dành cho máy đã cài sẵn AutoCAD 2007 • Giữ nguyên cấu hình & profile gốc", f2, br2, 20, 46);
                }
            };
            this.Controls.Add(pnlHeader);

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
                Location = new Point(536, 12),
                Size = new Size(88, 34),
                BackColor = Color.FromArgb(226, 232, 240),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f)
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => { if (currentStep != 4) this.Close(); };
            pnlBottom.Controls.Add(btnCancel);

            btnNext = new Button()
            {
                Text = "Tiếp Tục >",
                Location = new Point(422, 12),
                Size = new Size(106, 34),
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
                Location = new Point(320, 12),
                Size = new Size(95, 34),
                BackColor = Color.FromArgb(226, 232, 240),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f),
                Enabled = false
            };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Click += (s, e) => PrevStep();
            pnlBottom.Controls.Add(btnBack);

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

            btnBack.Enabled = (step > 0 && step < 4);
            btnCancel.Enabled = (step != 4);
            btnNext.Enabled = true;

            if (step == 0) // Welcome
            {
                btnNext.Text = "Bắt Đầu >";

                Label lblWelcome = new Label()
                {
                    Text = "Chào mừng đến với AVRoad 2026 (TDT 9.1) - Bản Lite!",
                    Location = new Point(20, 18),
                    Size = new Size(580, 30),
                    Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(15, 23, 42)
                };

                Label lblDesc = new Label()
                {
                    Text = "Phiên bản Lite được thiết kế gọn nhẹ (~36 MB) dành riêng cho máy tính đã cài đặt sẵn AutoCAD 2007.\n\n" +
                           "• Tận dụng nhân đồ họa AutoCAD 2007 sẵn có trên máy, không cài trùng lặp file hệ thống.\n" +
                           "• Tích hợp đầy đủ 17 module ARX TDT 9.1, 7 Menu chuyên ngành, bộ font & dữ liệu chuẩn.\n" +
                           "• Tạo profile 'AVRoad' hoàn toàn tách biệt — không làm thay đổi profile hay registry gốc của AutoCAD.\n" +
                           "• Thanh công cụ AVRoad SinglePanel 21 & cơ chế cập nhật bản vá trực tuyến (Hotfix).\n\n" +
                           "Nhấn 'Bắt Đầu >' để chọn thư mục cài đặt.",
                    Location = new Point(20, 60),
                    Size = new Size(580, 220),
                    Font = new Font("Segoe UI", 9.5f),
                    ForeColor = Color.FromArgb(51, 65, 85)
                };

                pnlContent.Controls.AddRange(new Control[] { lblWelcome, lblDesc });
            }
            else if (step == 1) // Install Directory
            {
                btnNext.Text = "Tiếp Tục >";

                Label lblTitle = new Label()
                {
                    Text = "Bước 1/3: Chọn Thư Mục Cài Đặt AVRoad 2026",
                    Location = new Point(20, 18),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(15, 23, 42)
                };

                Label lblSub = new Label()
                {
                    Text = "Chọn thư mục chứa dữ liệu & module AVRoad / TDT 9.1 (nên đặt tách riêng khỏi thư mục AutoCAD 2007 gốc, mặc định C:\\AVRoad_2026).",
                    Location = new Point(20, 50),
                    Size = new Size(580, 38),
                    Font = new Font("Segoe UI", 9f),
                    ForeColor = Color.FromArgb(100, 116, 139)
                };

                txtInstallDir = new TextBox()
                {
                    Text = string.IsNullOrEmpty(installDir) ? @"C:\AVRoad_2026" : installDir,
                    Location = new Point(20, 98),
                    Size = new Size(460, 26),
                    Font = new Font("Segoe UI", 9.5f)
                };
                txtInstallDir.TextChanged += (s, e) => { installDir = txtInstallDir.Text.Trim(); };

                Button btnBrowse = new Button()
                {
                    Text = "Duyệt...",
                    Location = new Point(488, 96),
                    Size = new Size(100, 28),
                    BackColor = Color.FromArgb(226, 232, 240),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9f)
                };
                btnBrowse.FlatAppearance.BorderSize = 0;
                btnBrowse.Click += (s, e) =>
                {
                    using (FolderBrowserDialog fbd = new FolderBrowserDialog())
                    {
                        fbd.Description = "Chọn thư mục cài đặt AVRoad 2026 (Bản Lite)";
                        if (Directory.Exists(txtInstallDir.Text)) fbd.SelectedPath = txtInstallDir.Text;
                        if (fbd.ShowDialog() == DialogResult.OK)
                        {
                            txtInstallDir.Text = fbd.SelectedPath;
                        }
                    }
                };

                Label lblSpace = new Label()
                {
                    Text = "Dung lượng yêu cầu: ~100 MB (giảm hơn 75% so với bản Full)\nBao gồm đầy đủ ARX, DBX, Data, CTTK, Lib, Blocks, DCCT, Font TDT và VC++ 2005 Runtime.",
                    Location = new Point(20, 146),
                    Size = new Size(570, 48),
                    Font = new Font("Segoe UI", 9f),
                    ForeColor = Color.FromArgb(71, 85, 105)
                };

                pnlContent.Controls.AddRange(new Control[] { lblTitle, lblSub, txtInstallDir, btnBrowse, lblSpace });
            }
            else if (step == 2) // Select / Verify AutoCAD 2007 Host
            {
                btnNext.Text = "Tiếp Tục >";

                Label lblTitle = new Label()
                {
                    Text = "Bước 2/3: Xác Nhận Đường Dẫn AutoCAD 2007",
                    Location = new Point(20, 18),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(15, 23, 42)
                };

                Label lblSub = new Label()
                {
                    Text = "Trình cài đặt đã tự động dò tìm AutoCAD 2007 (acad.exe) qua Registry và các thư mục cài đặt tiêu chuẩn.\nNếu bạn cài AutoCAD 2007 ở vị trí khác hoặc dùng bản Portable, hãy bấm 'Duyệt (Browse...)' để chọn file acad.exe.",
                    Location = new Point(20, 48),
                    Size = new Size(585, 42),
                    Font = new Font("Segoe UI", 9f),
                    ForeColor = Color.FromArgb(100, 116, 139)
                };

                txtCadExePath = new TextBox()
                {
                    Text = cadExePath ?? "",
                    Location = new Point(20, 102),
                    Size = new Size(430, 26),
                    Font = new Font("Segoe UI", 9.5f)
                };
                txtCadExePath.TextChanged += (s, e) => ValidateCadExeUI();

                Button btnBrowseCad = new Button()
                {
                    Text = "Duyệt (Browse...)",
                    Location = new Point(458, 100),
                    Size = new Size(135, 28),
                    BackColor = Color.FromArgb(226, 232, 240),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9f),
                    Cursor = Cursors.Hand
                };
                btnBrowseCad.FlatAppearance.BorderSize = 0;
                btnBrowseCad.Click += (s, e) =>
                {
                    using (OpenFileDialog ofd = new OpenFileDialog())
                    {
                        ofd.Title = "Chọn file acad.exe của AutoCAD 2007";
                        ofd.Filter = "AutoCAD 2007 (acad.exe)|acad.exe|Executable (*.exe)|*.exe";
                        ofd.CheckFileExists = true;
                        string cur = ResolveAcadExePath(txtCadExePath.Text);
                        if (!string.IsNullOrEmpty(cur))
                        {
                            ofd.InitialDirectory = Path.GetDirectoryName(cur);
                            ofd.FileName = Path.GetFileName(cur);
                        }
                        if (ofd.ShowDialog() == DialogResult.OK)
                        {
                            txtCadExePath.Text = ofd.FileName;
                        }
                    }
                };

                lblCadStatus = new Label()
                {
                    Location = new Point(20, 145),
                    Size = new Size(575, 65),
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
                };

                Label lblNote = new Label()
                {
                    Text = "Lưu ý: Trình cài đặt chỉ tạo profile 'AVRoad' và sao lưu acad.exe.config (nếu có) thành acad.exe.config.avroad.bak, hoàn toàn không chỉnh sửa các file nhân hay profile gốc của AutoCAD 2007.",
                    Location = new Point(20, 222),
                    Size = new Size(575, 45),
                    Font = new Font("Segoe UI", 8.5f),
                    ForeColor = Color.FromArgb(100, 116, 139)
                };

                pnlContent.Controls.AddRange(new Control[] { lblTitle, lblSub, txtCadExePath, btnBrowseCad, lblCadStatus, lblNote });
                ValidateCadExeUI();
            }
            else if (step == 3) // Shortcut Options
            {
                btnNext.Text = "Cài Đặt Ngay";

                Label lblTitle = new Label()
                {
                    Text = "Bước 3/3: Tùy Chọn Phím Tắt & Xác Nhận",
                    Location = new Point(20, 18),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(15, 23, 42)
                };

                chkDesktopShortcut = new CheckBox()
                {
                    Text = "Tạo biểu tượng AVRoad 2026 trên màn hình Desktop",
                    Location = new Point(24, 62),
                    AutoSize = true,
                    Checked = createDesktopShortcut,
                    Font = new Font("Segoe UI", 9.5f)
                };
                chkDesktopShortcut.CheckedChanged += (s, e) => { createDesktopShortcut = chkDesktopShortcut.Checked; };

                chkStartMenuShortcut = new CheckBox()
                {
                    Text = "Tạo nhóm phím tắt trong Start Menu",
                    Location = new Point(24, 98),
                    AutoSize = true,
                    Checked = createStartMenuShortcut,
                    Font = new Font("Segoe UI", 9.5f)
                };
                chkStartMenuShortcut.CheckedChanged += (s, e) => { createStartMenuShortcut = chkStartMenuShortcut.Checked; };

                Label lblSummary = new Label()
                {
                    Text = "Thông tin cài đặt:\n" +
                           "• Thư mục AVRoad 2026: " + installDir + "\n" +
                           "• AutoCAD 2007 Host: " + cadExePath + "\n" +
                           "• Profile khởi tạo: AVRoad (độc lập với profile mặc định của AutoCAD 2007)",
                    Location = new Point(24, 150),
                    Size = new Size(570, 95),
                    Font = new Font("Segoe UI", 9f),
                    ForeColor = Color.FromArgb(71, 85, 105)
                };

                pnlContent.Controls.AddRange(new Control[] { lblTitle, chkDesktopShortcut, chkStartMenuShortcut, lblSummary });
            }
            else if (step == 4) // Installing
            {
                btnBack.Enabled = false;
                btnNext.Enabled = false;
                btnCancel.Enabled = false;

                Label lblTitle = new Label()
                {
                    Text = "Đang Tiến Hành Cài Đặt Bản Lite...",
                    Location = new Point(20, 20),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(15, 23, 42)
                };

                lblExtractStatus = new Label()
                {
                    Text = "Đang giải nén các module AVRoad / TDT 9.1...",
                    Location = new Point(20, 60),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9.5f),
                    ForeColor = Color.FromArgb(51, 65, 85)
                };

                progressBar = new ProgressBar()
                {
                    Location = new Point(20, 92),
                    Size = new Size(575, 24),
                    Style = ProgressBarStyle.Continuous
                };

                lblExtractFile = new Label()
                {
                    Text = "Vui lòng đợi...",
                    Location = new Point(20, 126),
                    Size = new Size(575, 24),
                    Font = new Font("Consolas", 8.5f),
                    ForeColor = Color.FromArgb(100, 116, 139)
                };

                pnlContent.Controls.AddRange(new Control[] { lblTitle, lblExtractStatus, progressBar, lblExtractFile });

                StartInstallation();
            }
            else if (step == 5) // Finished
            {
                btnBack.Enabled = false;
                btnCancel.Enabled = false;
                btnNext.Enabled = true;
                btnNext.Text = "HOÀN TẤT";

                Label lblTitle = new Label()
                {
                    Text = "Cài Đặt AVRoad 2026 (Bản Lite) Thành Công!",
                    Location = new Point(20, 20),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 12.5f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(16, 185, 129)
                };

                Label lblDesc = new Label()
                {
                    Text = "AVRoad 2026 (TDT 9.1) đã được cài đặt và liên kết thành công với AutoCAD 2007.\n\n" +
                           "• Thư mục AVRoad: " + installDir + "\n" +
                           "• AutoCAD 2007 Host: " + cadExePath + "\n" +
                           "• Profile 'AVRoad' đã được thiết lập sạch sẽ mà không làm thay đổi AutoCAD gốc.",
                    Location = new Point(20, 60),
                    Size = new Size(575, 115),
                    Font = new Font("Segoe UI", 9.5f),
                    ForeColor = Color.FromArgb(51, 65, 85)
                };

                chkLaunchNow = new CheckBox()
                {
                    Text = "Khởi chạy AVRoad 2026 ngay",
                    Location = new Point(24, 190),
                    AutoSize = true,
                    Checked = true,
                    Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(15, 23, 42)
                };

                pnlContent.Controls.AddRange(new Control[] { lblTitle, lblDesc, chkLaunchNow });
            }
        }

        private void ValidateCadExeUI()
        {
            if (txtCadExePath == null || lblCadStatus == null) return;
            string resolved = ResolveAcadExePath(txtCadExePath.Text);
            if (!string.IsNullOrEmpty(resolved))
            {
                cadExePath = resolved;
                lblCadStatus.ForeColor = Color.FromArgb(5, 150, 105);
                lblCadStatus.Text = "✓ Đã tìm thấy AutoCAD 2007: " + resolved;
                btnNext.Enabled = true;
            }
            else
            {
                lblCadStatus.ForeColor = Color.FromArgb(220, 38, 38);
                lblCadStatus.Text = "✕ Chưa tìm thấy file acad.exe hợp lệ tại đường dẫn này.\nVui lòng bấm 'Duyệt (Browse...)' để chọn file acad.exe của AutoCAD 2007.";
                btnNext.Enabled = false;
            }
        }

        private void NextStep()
        {
            if (currentStep == 1)
            {
                string dir = txtInstallDir != null ? txtInstallDir.Text.Trim() : installDir;
                if (string.IsNullOrEmpty(dir))
                {
                    MessageBox.Show("Vui lòng chọn thư mục cài đặt AVRoad 2026!", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                installDir = Path.GetFullPath(dir).TrimEnd('\\');
            }
            else if (currentStep == 2)
            {
                string resolved = ResolveAcadExePath(txtCadExePath != null ? txtCadExePath.Text : cadExePath);
                if (string.IsNullOrEmpty(resolved))
                {
                    MessageBox.Show("Đường dẫn AutoCAD 2007 không hợp lệ hoặc không tồn tại file acad.exe!\nVui lòng chọn đúng file acad.exe trước khi tiếp tục.", "Không tìm thấy acad.exe", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ValidateCadExeUI();
                    return;
                }
                cadExePath = resolved;
            }
            else if (currentStep == 5)
            {
                if (chkLaunchNow != null && chkLaunchNow.Checked)
                {
                    string targetExe = Path.Combine(installDir, "AVRoad.exe");
                    if (File.Exists(targetExe))
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo()
                            {
                                FileName = targetExe,
                                WorkingDirectory = installDir
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
            if (currentStep > 0 && currentStep < 4)
            {
                ShowStep(currentStep - 1);
            }
        }

        private static string ResolveAcadExePath(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            try
            {
                string expanded = Environment.ExpandEnvironmentVariables(raw).Trim().Trim('"');
                if (string.IsNullOrEmpty(expanded)) return null;
                if (File.Exists(expanded) &&
                    string.Equals(Path.GetFileName(expanded), "acad.exe", StringComparison.OrdinalIgnoreCase))
                {
                    return Path.GetFullPath(expanded);
                }
                if (Directory.Exists(expanded))
                {
                    string cand = Path.Combine(expanded, "acad.exe");
                    if (File.Exists(cand)) return Path.GetFullPath(cand);
                }
            }
            catch { }
            return null;
        }

        private static string ProbeAutoCAD2007Hive(RegistryHive hive, RegistryView view)
        {
            try
            {
                using (RegistryKey root = RegistryKey.OpenBaseKey(hive, view))
                using (RegistryKey r17 = root.OpenSubKey(@"Software\Autodesk\AutoCAD\R17.0", false))
                {
                    if (r17 == null) return null;
                    List<string> subNames = new List<string>();
                    string curVer = r17.GetValue("CurVer") as string;
                    if (!string.IsNullOrEmpty(curVer)) subNames.Add(curVer);
                    if (!subNames.Contains("ACAD-5001:409")) subNames.Add("ACAD-5001:409");
                    foreach (string s in r17.GetSubKeyNames())
                    {
                        if (!subNames.Contains(s)) subNames.Add(s);
                    }
                    foreach (string sub in subNames)
                    {
                        using (RegistryKey pk = r17.OpenSubKey(sub, false))
                        {
                            if (pk == null) continue;
                            foreach (string valName in new string[] { "Location", "AcadLocation" })
                            {
                                string cand = ResolveAcadExePath(pk.GetValue(valName) as string);
                                if (!string.IsNullOrEmpty(cand)) return cand;
                            }
                        }
                    }
                }
            }
            catch { }
            return null;
        }

        public static string AutoDetectAutoCAD2007Exe()
        {
            // 1. HKLM 32-bit & 64-bit
            string cand = ProbeAutoCAD2007Hive(RegistryHive.LocalMachine, RegistryView.Registry32);
            if (!string.IsNullOrEmpty(cand)) return cand;

            cand = ProbeAutoCAD2007Hive(RegistryHive.LocalMachine, RegistryView.Registry64);
            if (!string.IsNullOrEmpty(cand)) return cand;

            // 2. HKCU
            cand = ProbeAutoCAD2007Hive(RegistryHive.CurrentUser, RegistryView.Registry32);
            if (!string.IsNullOrEmpty(cand)) return cand;

            cand = ProbeAutoCAD2007Hive(RegistryHive.CurrentUser, RegistryView.Default);
            if (!string.IsNullOrEmpty(cand)) return cand;

            // 3. Default directories
            foreach (string dir in new string[] {
                @"C:\Program Files (x86)\AutoCAD 2007",
                @"C:\Program Files\AutoCAD 2007",
                @"D:\AutoCAD 2007"
            })
            {
                cand = ResolveAcadExePath(dir);
                if (!string.IsNullOrEmpty(cand)) return cand;
            }

            return null;
        }

        private void UpdateStatusSafe(string text)
        {
            if (this.IsDisposed) return;
            try
            {
                this.BeginInvoke((Action)(() =>
                {
                    if (lblExtractStatus != null && !lblExtractStatus.IsDisposed)
                        lblExtractStatus.Text = text;
                }));
            }
            catch { }
        }

        private void StartInstallation()
        {
            string targetDir = Path.GetFullPath(installDir.Trim()).TrimEnd('\\');
            string resolvedExe = ResolveAcadExePath(cadExePath);
            if (string.IsNullOrEmpty(resolvedExe))
            {
                MessageBox.Show("Không tìm thấy file acad.exe của AutoCAD 2007!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ShowStep(2);
                return;
            }
            string cadHostDir = Path.GetDirectoryName(resolvedExe).TrimEnd('\\');

            if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);

            Thread t = new Thread(() =>
            {
                try
                {
                    // 1. Open embedded payload_lite.zip (resource name: payload.zip)
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
                        foreach (string cand in new string[] {
                            Path.Combine(baseDir, "payload_lite.zip"),
                            Path.Combine(baseDir, "payload.zip"),
                            @"C:\TDT_RESEARCH\payload_lite.zip"
                        })
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
                        this.BeginInvoke((Action)(() =>
                        {
                            MessageBox.Show("Không tìm thấy tệp tài nguyên payload.zip bên trong hoặc cạnh trình cài đặt!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }));
                        return;
                    }

                    // 2. Extract payload_lite.zip into targetDir (registerCom=false: Lite mode does not overwrite Host CAD COM)
                    using (zipStream)
                    {
                        string journal = PatchTransaction.InstallEmbedded(zipStream, targetDir, false, (percent, name) =>
                        {
                            this.BeginInvoke((Action)(() =>
                            {
                                if (progressBar != null && !progressBar.IsDisposed) progressBar.Value = Math.Min(100, Math.Max(0, percent));
                                if (lblExtractFile != null && !lblExtractFile.IsDisposed) lblExtractFile.Text = name;
                            }));
                        });
                        File.WriteAllText(Path.Combine(targetDir, "last-install-rollback.txt"), journal);
                    }

                    // 3. Configure AVRoad.ini with AutoCADPath=<cadHostDir> and valid Lite paths
                    UpdateStatusSafe("Đang cấu hình AVRoad.ini và liên kết AutoCAD 2007...");
                    WriteLiteIni(targetDir, cadHostDir);

                    // 4. Place acad.exe.config in cadHostDir (backup existing as acad.exe.config.avroad.bak)
                    UpdateStatusSafe("Đang cấu hình acad.exe.config cho .NET CLR 2.0/4.0...");
                    CADRegistryConfig.EnsureAcadExeConfig(cadHostDir, true);

                    // 5. Install VC++ 2005 SP1 x86 runtime if present
                    string vcRedist = Path.Combine(targetDir, "vcredist_x86.exe");
                    if (!RegistryScope.IsLimited && File.Exists(vcRedist))
                    {
                        try
                        {
                            UpdateStatusSafe("Đang cài đặt/tối ưu thư viện Microsoft Visual C++ 2005 Runtime...");
                            ProcessStartInfo psiVc = new ProcessStartInfo(vcRedist, "/q:a /c:\"vcredist.msi /qn\"");
                            psiVc.UseShellExecute = false;
                            psiVc.CreateNoWindow = true;
                            using (Process pVc = Process.Start(psiVc))
                            {
                                if (pVc != null) pVc.WaitForExit(45000);
                            }
                        }
                        catch { }
                    }

                    // 6. Apply clean Lite Registry profile (AVRoad profile only, preserves Host AutoCAD profiles & keys)
                    UpdateStatusSafe("Đang tạo profile AVRoad độc lập trong Registry...");
                    CADRegistryConfig.ApplyLiteConfig(targetDir, cadHostDir);

                    // 7. Create shortcuts & Register Uninstall
                    UpdateStatusSafe("Đang tạo biểu tượng khởi chạy và hoàn tất đăng ký...");
                    CreateShortcuts(targetDir);
                    RegisterWindowsUninstall(targetDir);

                    this.BeginInvoke((Action)(() => ShowStep(5)));
                }
                catch (Exception ex)
                {
                    this.BeginInvoke((Action)(() =>
                    {
                        MessageBox.Show("Lỗi trong quá trình cài đặt: " + ex.Message, "Lỗi Cài Đặt", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        btnBack.Enabled = true;
                        btnCancel.Enabled = true;
                    }));
                }
            });
            t.IsBackground = true;
            t.Start();
        }

        private static void WriteLiteIni(string targetDir, string cadHostDir)
        {
            string iniPath = Path.Combine(targetDir, "AVRoad.ini");
            string hostFonts = Path.Combine(cadHostDir, "Fonts");
            string fontPath = Directory.Exists(hostFonts) ? hostFonts : Path.Combine(targetDir, "Fonts");
            string hostPlotStyles = Path.Combine(cadHostDir, "Plot Styles");
            string plotStylePath = Directory.Exists(hostPlotStyles) ? hostPlotStyles : Path.Combine(targetDir, "Plot Styles");
            string hostPlotters = Path.Combine(cadHostDir, "Plotters");
            string plotterPath = Directory.Exists(hostPlotters) ? hostPlotters : Path.Combine(targetDir, "Plotters");
            string argPath = Path.Combine(targetDir, "AVRoad_Profile.arg");

            List<string> lines = File.Exists(iniPath)
                ? new List<string>(File.ReadAllLines(iniPath, Encoding.UTF8))
                : new List<string>(new string[] {
                    "[AVSetting]",
                    "Configured=1",
                    "AutoStart=1",
                    "CheckUpdateOnStartup=1",
                    "UpdateUrl=https://raw.githubusercontent.com/tienhoaa/AVRoad/main/version.json",
                    "VTEnable=1",
                    "ZoomFactor=80",
                    "WhipArc=1",
                    "SelectionPreview=2",
                    "BgColor=BIM"
                });

            SetOrAppendIniKey(lines, "FontPath", fontPath);
            SetOrAppendIniKey(lines, "PlotStylePath", plotStylePath);
            SetOrAppendIniKey(lines, "PlotterPath", plotterPath);
            SetOrAppendIniKey(lines, "ArgProfilePath", argPath);
            SetOrAppendIniKey(lines, "AutoCADPath", cadHostDir);

            File.WriteAllLines(iniPath, lines.ToArray(), Encoding.UTF8);
        }

        private static void SetOrAppendIniKey(List<string> lines, string key, string value)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                string trimmed = lines[i].Trim();
                if (trimmed.StartsWith(key + "=", StringComparison.OrdinalIgnoreCase))
                {
                    lines[i] = key + "=" + value;
                    return;
                }
            }
            lines.Add(key + "=" + value);
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
                string uninstExe = Path.Combine(targetDir, "Uninstall.exe");
                string icoPath = Path.Combine(targetDir, "AVRoad.ico");

                if (createDesktopShortcut && File.Exists(mainExe))
                {
                    dynamic sc = shell.CreateShortcut(Path.Combine(desktop, "AVRoad 2026.lnk"));
                    sc.TargetPath = mainExe;
                    sc.WorkingDirectory = targetDir;
                    sc.Description = "AVRoad 2026 Solution (Lite) - Hệ sinh thái AV BIM CDE";
                    if (File.Exists(icoPath)) sc.IconLocation = icoPath + ",0";
                    sc.Save();
                }

                if (createStartMenuShortcut)
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
            string uninstExe = Path.Combine(targetDir, "Uninstall.exe");
            string uninstallSubKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\AVRoad2026";

            bool registeredInMachine = false;
            try
            {
                using (RegistryKey lm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).CreateSubKey(uninstallSubKey))
                {
                    if (lm != null)
                    {
                        lm.SetValue("DisplayName", "AVRoad 2026 Solution (Lite)");
                        lm.SetValue("DisplayVersion", "2.2.2.0");
                        lm.SetValue("Publisher", "AV Technology CDE");
                        lm.SetValue("UninstallString", "\"" + uninstExe + "\"");
                        lm.SetValue("QuietUninstallString", "\"" + uninstExe + "\"");
                        lm.SetValue("DisplayIcon", Path.Combine(targetDir, "AVRoad.ico"));
                        lm.SetValue("InstallLocation", targetDir);
                        lm.SetValue("EstimatedSize", 102000, RegistryValueKind.DWord);
                        lm.SetValue("NoModify", 1, RegistryValueKind.DWord);
                        lm.SetValue("NoRepair", 1, RegistryValueKind.DWord);
                        registeredInMachine = true;
                    }
                }
            }
            catch { }

            if (registeredInMachine)
            {
                try { Registry.CurrentUser.DeleteSubKeyTree(uninstallSubKey, false); } catch { }
            }
            else
            {
                try
                {
                    using (RegistryKey uKey = Registry.CurrentUser.CreateSubKey(uninstallSubKey))
                    {
                        if (uKey != null)
                        {
                            uKey.SetValue("DisplayName", "AVRoad 2026 Solution (Lite)");
                            uKey.SetValue("DisplayVersion", "2.2.2.0");
                            uKey.SetValue("Publisher", "AV Technology CDE");
                            uKey.SetValue("UninstallString", "\"" + uninstExe + "\"");
                            uKey.SetValue("QuietUninstallString", "\"" + uninstExe + "\"");
                            uKey.SetValue("DisplayIcon", Path.Combine(targetDir, "AVRoad.ico"));
                            uKey.SetValue("InstallLocation", targetDir);
                            uKey.SetValue("EstimatedSize", 102000, RegistryValueKind.DWord);
                            uKey.SetValue("NoModify", 1, RegistryValueKind.DWord);
                            uKey.SetValue("NoRepair", 1, RegistryValueKind.DWord);
                        }
                    }
                }
                catch { }
            }
        }

        [STAThread]
        public static void Main(string[] args)
        {
            if (args.Length == 2 && args[0] == "/extract-only")
            {
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
                    bool skipElevate = (args != null && args.Length > 0 && args[0] == "/elevated");
                    if (!skipElevate && !p.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator))
                    {
                        ProcessStartInfo psi = new ProcessStartInfo();
                        psi.FileName = Application.ExecutablePath;
                        psi.Arguments = "/elevated";
                        psi.Verb = "runas";
                        psi.UseShellExecute = true;
                        try
                        {
                            Process proc = Process.Start(psi);
                            if (proc != null) return;
                        }
                        catch { }
                    }
                }
            }
            catch { }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SetupLiteWizardForm());
        }
    }
}
