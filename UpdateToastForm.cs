using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace AVRoadLauncher
{
    public class UpdateToastForm : Form
    {
        public UpdateToastForm(string version, string changelog, string patchUrl, Process cad, string appDir, string patchSha = "")
        {
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(500, 275);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = true;
            this.TopMost = true;
            this.BackColor = Color.FromArgb(15, 23, 42); // Deep Slate Navy
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 9F);
            this.Text = "AVRoad 2026 - B\u1ea3n C\u1eadp Nh\u1eadt M\u1edbi";

            string icoPath = Path.Combine(appDir, "AVRoad.ico");
            if (File.Exists(icoPath))
            {
                try { this.Icon = new Icon(icoPath); } catch { }
            }

            // Top accent bar Mint Neon
            Panel topBar = new Panel
            {
                BackColor = Color.FromArgb(45, 212, 191),
                Dock = DockStyle.Top,
                Height = 4
            };
            this.Controls.Add(topBar);

            // Category tag
            Label lblTag = new Label
            {
                Text = "AVROAD 2026 \u2022 H\u1ec6 SINH TH\u00c1I THI\u1ebeT K\u1ebe AV BIM CDE",
                ForeColor = Color.FromArgb(45, 212, 191),
                Location = new Point(20, 16),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.25F, FontStyle.Bold)
            };
            this.Controls.Add(lblTag);

            // Title
            Label lblTitle = new Label
            {
                Text = "Ph\u00e1t hi\u1ec7n b\u1ea3n n\u00e2ng c\u1ea5p m\u1edbi v" + version.TrimStart('v', 'V'),
                ForeColor = Color.White,
                Location = new Point(20, 38),
                Size = new Size(450, 26),
                Font = new Font("Segoe UI", 11.5F, FontStyle.Bold)
            };
            this.Controls.Add(lblTitle);

            // Changelog preview
            string displayChangelog = string.IsNullOrWhiteSpace(changelog)
                ? "B\u1ea3n c\u1eadp nh\u1eadt t\u1ed1i \u01b0u hi\u1ec7u n\u0103ng, giao di\u1ec7n v\u00e0 \u1ed5n \u0111\u1ecbnh h\u1ec7 th\u1ed1ng."
                : changelog;

            TextBox txtChangelog = new TextBox
            {
                Text = displayChangelog,
                ForeColor = Color.FromArgb(203, 213, 225),
                BackColor = Color.FromArgb(24, 34, 52),
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(20, 68),
                Size = new Size(445, 105),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 8.75F)
            };
            this.Controls.Add(txtChangelog);

            // Buttons
            Button btnLater = new Button
            {
                Text = "\u0110\u1ec3 sau",
                Bounds = new Rectangle(20, 186, 110, 36),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(42, 53, 63),
                ForeColor = Color.FromArgb(200, 210, 220),
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            btnLater.FlatAppearance.BorderSize = 0;
            btnLater.Click += delegate { this.Close(); };
            this.Controls.Add(btnLater);

            Button btnUpdate = new Button
            {
                Text = "C\u1eacP NH\u1eacT NGAY",
                Bounds = new Rectangle(145, 186, 320, 36),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(45, 212, 191),
                ForeColor = Color.FromArgb(10, 37, 35),
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
            };
            btnUpdate.FlatAppearance.BorderSize = 0;
            btnUpdate.Click += delegate
            {
                string exe = Path.Combine(appDir, "AVUpdater.v2.exe");
                if (!File.Exists(exe)) exe = Path.Combine(appDir, "AVUpdater.exe");
                if (!File.Exists(exe))
                {
                    MessageBox.Show(this, "Kh\u00f4ng t\u00ecm th\u1ea5y file AVUpdater.exe trong th\u01b0 m\u1ee5c ph\u1ea7n m\u1ec1m.", "AVRoad 2026", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                try
                {
                    string updArgs = "\"/url:" + patchUrl + "\" \"/ver:" + version + "\" \"/sha:" + patchSha + "\"";
                    Process.Start(new ProcessStartInfo(exe, updArgs)
                    {
                        WorkingDirectory = appDir
                    });
                    this.Close();
                    // Terminate launcher so updater can safely replace files
                    Environment.Exit(0);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Kh\u00f4ng th\u1ec3 kh\u1edfi ch\u1ea1y tr\u00ecnh c\u1eadp nh\u1eadt: " + ex.Message, "L\u1ed7i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            this.Controls.Add(btnUpdate);

            try { System.Media.SystemSounds.Asterisk.Play(); } catch { }
        }
    }
}
