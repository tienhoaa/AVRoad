using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AVRoadLauncher
{
    public class UpdateToastForm : Form
    {
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_SHOWWINDOW = 0x0040;
        private const uint SWP_NOACTIVATE = 0x0010;

        private readonly Timer keepOnTopTimer = new Timer();
        private int keepOnTopTicks = 0;

        public UpdateToastForm(string version, string changelog, string patchUrl, Process cad, string appDir)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.Size = new Size(440, 200);
            this.ShowInTaskbar = true;
            this.TopMost = true;
            this.BackColor = Color.FromArgb(23, 31, 37);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 9F);
            this.DoubleBuffered = true;

            string icoPath = Path.Combine(appDir, "AVRoad.ico");
            if (File.Exists(icoPath))
            {
                try { this.Icon = new Icon(icoPath); } catch { }
            }

            // Top accent bar
            Controls.Add(new Panel { BackColor = Color.FromArgb(45, 212, 191), Dock = DockStyle.Top, Height = 4 });

            // Category tag
            Label lblTag = new Label
            {
                Text = "AVROAD 2026 • HỆ SINH THÁI AV BIM",
                ForeColor = Color.FromArgb(45, 212, 191),
                Location = new Point(20, 16),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold)
            };
            Controls.Add(lblTag);

            // Title
            Label lblTitle = new Label
            {
                Text = "Phát hiện bản cập nhật mới v" + version.TrimStart('v', 'V'),
                ForeColor = Color.White,
                Location = new Point(20, 40),
                Size = new Size(400, 26),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold)
            };
            Controls.Add(lblTitle);

            // Changelog preview
            string displayChangelog = string.IsNullOrWhiteSpace(changelog) 
                ? "Bản cập nhật tối ưu độ ổn định và sửa các lỗi nghiệp vụ." 
                : changelog.Replace("\r", " ").Replace("\n", " • ");
            Label lblDesc = new Label
            {
                Text = displayChangelog,
                ForeColor = Color.FromArgb(181, 194, 203),
                Location = new Point(20, 72),
                Size = new Size(400, 42),
                AutoEllipsis = true,
                Font = new Font("Segoe UI", 9F)
            };
            Controls.Add(lblDesc);

            // Note
            Label lblNote = new Label
            {
                Text = "⚡ Tự động tải và áp dụng bản vá trong tích tắc.",
                ForeColor = Color.FromArgb(143, 161, 173),
                Location = new Point(20, 120),
                Size = new Size(400, 20),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Italic)
            };
            Controls.Add(lblNote);

            // Buttons
            Button btnLater = new Button
            {
                Text = "Để sau",
                Bounds = new Rectangle(20, 150, 110, 34),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(42, 53, 63),
                ForeColor = Color.FromArgb(200, 210, 220),
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            btnLater.FlatAppearance.BorderSize = 0;
            btnLater.Click += delegate { this.Close(); };
            Controls.Add(btnLater);

            Button btnUpdate = new Button
            {
                Text = "CẬP NHẬT NGAY",
                Bounds = new Rectangle(142, 150, 278, 34),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(45, 212, 191),
                ForeColor = Color.FromArgb(10, 37, 35),
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
            };
            btnUpdate.FlatAppearance.BorderSize = 0;
            btnUpdate.Click += delegate
            {
                string exe = Path.Combine(appDir, "AVUpdater.exe");
                if (!File.Exists(exe))
                {
                    MessageBox.Show(this, "Không tìm thấy file AVUpdater.exe trong thư mục phần mềm.", "AVRoad 2026", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                try
                {
                    Process.Start(new ProcessStartInfo(exe, "\"/url:" + patchUrl + "\" \"/ver:" + version + "\"")
                    {
                        WorkingDirectory = appDir
                    });
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Không thể khởi chạy trình cập nhật: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            Controls.Add(btnUpdate);

            // Position at bottom-right corner of working area
            PositionBottomRight();

            // Sound alert
            try { System.Media.SystemSounds.Asterisk.Play(); } catch { }

            // Timer to keep window on top above CAD for the first 10 seconds
            keepOnTopTimer.Interval = 500;
            keepOnTopTimer.Tick += delegate
            {
                keepOnTopTicks++;
                SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
                if (keepOnTopTicks > 20) // 10 seconds
                {
                    keepOnTopTimer.Stop();
                }
            };
            keepOnTopTimer.Start();
        }

        private void PositionBottomRight()
        {
            Rectangle wa = Screen.PrimaryScreen.WorkingArea;
            int margin = 20;
            this.Location = new Point(wa.Right - this.Width - margin, wa.Bottom - this.Height - margin);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // Draw subtle border
            using (Pen p = new Pen(Color.FromArgb(45, 212, 191), 1))
            {
                e.Graphics.DrawRectangle(p, 0, 0, this.Width - 1, this.Height - 1);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            PositionBottomRight();
            SetWindowPos(this.Handle, HWND_TOPMOST, this.Location.X, this.Location.Y, this.Width, this.Height, SWP_SHOWWINDOW);
            this.BringToFront();
            this.Activate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                keepOnTopTimer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
