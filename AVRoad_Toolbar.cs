using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AVRoadToolbar
{
    public class ModernHUDToolbar : Form
    {
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        const int WM_NCLBUTTONDOWN = 0xA1;
        const int HTCAPTION = 0x2;

        private FlowLayoutPanel flowPanel;
        private string appDir;
        private System.Windows.Forms.Timer cadMonitorTimer;
        private bool isMinimized = false;

        public ModernHUDToolbar()
        {
            appDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');

            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.Size = new Size(820, 48);
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.DoubleBuffered = true;
            this.BackColor = Color.FromArgb(15, 23, 20);

            // Center horizontally near top of screen
            Rectangle wa = Screen.PrimaryScreen.WorkingArea;
            this.Location = new Point((wa.Width - this.Width) / 2, 85);

            string icoPath = Path.Combine(appDir, "AVRoad.ico");
            if (File.Exists(icoPath))
            {
                try { this.Icon = new Icon(icoPath); } catch { }
            }

            BuildUI();

            // Auto-close when AutoCAD exits
            cadMonitorTimer = new System.Windows.Forms.Timer();
            cadMonitorTimer.Interval = 1500;
            cadMonitorTimer.Tick += (s, e) =>
            {
                Process[] cadProcs = Process.GetProcessesByName("acad");
                if (cadProcs == null || cadProcs.Length == 0)
                {
                    cadMonitorTimer.Stop();
                    this.Close();
                }
            };
            cadMonitorTimer.Start();
        }

        private void BuildUI()
        {
            this.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(this.Handle, WM_NCLBUTTONDOWN, (IntPtr)HTCAPTION, IntPtr.Zero);
                }
            };

            this.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                using (LinearGradientBrush brush = new LinearGradientBrush(this.ClientRectangle, Color.FromArgb(10, 30, 24), Color.FromArgb(18, 55, 42), LinearGradientMode.Horizontal))
                {
                    g.FillRectangle(brush, this.ClientRectangle);
                }

                using (Pen borderPen = new Pen(Color.FromArgb(45, 212, 191), 1.5f))
                {
                    g.DrawRectangle(borderPen, 0, 0, this.Width - 1, this.Height - 1);
                }
            };

            // Drag handle / Brand logo label
            Label lblBrand = new Label()
            {
                Text = "AVRoad",
                ForeColor = Color.FromArgb(45, 212, 191),
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                Location = new Point(12, 13),
                AutoSize = true,
                Cursor = Cursors.SizeAll
            };
            lblBrand.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(this.Handle, WM_NCLBUTTONDOWN, (IntPtr)HTCAPTION, IntPtr.Zero);
                }
            };
            this.Controls.Add(lblBrand);

            // Flow panel for modern action buttons
            flowPanel = new FlowLayoutPanel()
            {
                Location = new Point(85, 6),
                Size = new Size(690, 36),
                WrapContents = false,
                AutoScroll = false,
                BackColor = Color.Transparent
            };
            this.Controls.Add(flowPanel);

            AddToolBtn("Bình Đồ", "DLBD", "av_dlbd_16.bmp", "Nhập dữ liệu bình đồ (DLBD)");
            AddToolBtn("Tim Tuyến", "TT", "av_tt_16.bmp", "Thiết kế tim tuyến (TT)");
            AddToolBtn("Hiệu Chỉnh", "HCT", "av_hct_16.bmp", "Hiệu chỉnh tuyến (HCT)");
            AddToolBtn("Trắc Dọc", "TD", "av_td_16.bmp", "Thiết kế trắc dọc (TD)");
            AddToolBtn("Trắc Ngang", "TN", "av_tn_16.bmp", "Thiết kế trắc ngang (TN)");
            AddToolBtn("San Nền", "SN", "av_sannen_16.bmp", "Thiết kế san nền (SN)");
            AddToolBtn("Vạch Sơn", "RM", "av_vachson_16.bmp", "Vạch sơn kẻ đường (RM)");
            AddToolBtn("3D Tuyến", "3D", "av_3d_16.bmp", "Mô hình 3D tuyến (3D)");
            AddToolBtn("Chọn Nhanh", "SS", "av_ss_16.bmp", "Chọn nhanh đối tượng cùng loại (SS)");
            AddToolBtn("Nền BIM", "MAUNEN", "av_maunen_16.bmp", "Đổi màu nền bản vẽ (MAUNEN)");
            AddToolBtn("Cấu Hình", "AVSETTING", "av_setting_16.bmp", "Mở trung tâm cấu hình (AVSETTING)");

            // Close / Minimize Button
            Button btnClose = new Button()
            {
                Text = "✕",
                Location = new Point(this.Width - 36, 10),
                Size = new Size(26, 26),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(153, 246, 228),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Hide();
            this.Controls.Add(btnClose);
        }

        private void AddToolBtn(string label, string cmd, string iconFile, string tooltipText)
        {
            Button btn = new Button()
            {
                Text = " " + label,
                Height = 32,
                AutoSize = true,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(20, 48, 38),
                ForeColor = Color.FromArgb(240, 253, 250),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(2, 2, 2, 2),
                Padding = new Padding(4, 0, 6, 0),
                TextImageRelation = TextImageRelation.ImageBeforeText,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Color.FromArgb(35, 95, 75);

            string icoPath = Path.Combine(appDir, @"Support\icons\" + iconFile);
            if (File.Exists(icoPath))
            {
                try
                {
                    btn.Image = Image.FromFile(icoPath);
                }
                catch { }
            }

            btn.MouseEnter += (s, e) =>
            {
                btn.BackColor = Color.FromArgb(16, 185, 129);
                btn.ForeColor = Color.White;
                btn.FlatAppearance.BorderColor = Color.FromArgb(45, 212, 191);
            };
            btn.MouseLeave += (s, e) =>
            {
                btn.BackColor = Color.FromArgb(20, 48, 38);
                btn.ForeColor = Color.FromArgb(240, 253, 250);
                btn.FlatAppearance.BorderColor = Color.FromArgb(35, 95, 75);
            };

            ToolTip tip = new ToolTip();
            tip.SetToolTip(btn, tooltipText);

            btn.Click += (s, e) => SendCadCommand(cmd);

            flowPanel.Controls.Add(btn);
        }

        private void SendCadCommand(string cmd)
        {
            try
            {
                Type cadType = Type.GetTypeFromProgID("AutoCAD.Application");
                if (cadType != null)
                {
                    dynamic acadApp = Marshal.GetActiveObject("AutoCAD.Application");
                    if (acadApp != null && acadApp.ActiveDocument != null)
                    {
                        acadApp.ActiveDocument.SendCommand(cmd + "\n");
                        IntPtr acadHwnd = (IntPtr)acadApp.HWND;
                        if (acadHwnd != IntPtr.Zero) SetForegroundWindow(acadHwnd);
                    }
                }
            }
            catch (Exception ex)
            {
                // Fallback to Clipboard or notification
            }
        }

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ModernHUDToolbar());
        }
    }
}
