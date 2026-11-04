using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace AVRoadLauncher
{
    public class AVStartupForm : Form
    {
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private Image bgImage = null;
        private System.Windows.Forms.Timer animTimer;
        private int progress = 0;
        private string statusText = "Đang tối ưu nhân đồ họa AVCAD Engine & Direct3D...";
        private bool isReady = false;

        private string appDir;
        private string iniPath;
        private string fontPath = "";
        private string plotStylePath;
        private string plotterPath;
        private string argProfile;
        private bool checkUpdateOnStartup = true;

        private int hoveredBtn = -1;      // 0: Start, 1: Open, 2: Recent, 3: Setting, 4: Info
        private int hoveredRecent = -1;   // 0, 1, 2
        private int hoveredNews = -1;     // 0, 1, 2, 3
        private int hoveredWin = -1;      // 0: Min, 1: Max, 2: Close

        public class RecentItem
        {
            public string FileName;
            public string FilePath;
            public string FullPath;
            public string TimeText;
            public bool IsReal;
        }

        private List<RecentItem> recentItems = new List<RecentItem>();

        public AVStartupForm(string appDir, string iniPath, string fontPath, string plotStylePath, string plotterPath, string argProfile, bool checkUpdateOnStartup)
        {
            this.appDir = appDir;
            this.iniPath = iniPath;
            this.fontPath = fontPath;
            this.plotStylePath = plotStylePath;
            this.plotterPath = plotterPath;
            this.argProfile = argProfile;
            this.checkUpdateOnStartup = checkUpdateOnStartup;

            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(1024, 576);
            this.DoubleBuffered = true;
            this.BackColor = Color.FromArgb(15, 23, 42);

            string icoPath = Path.Combine(appDir, "AVRoad.ico");
            if (File.Exists(icoPath))
            {
                try { this.Icon = new Icon(icoPath); } catch { }
            }

            // Load Clean Background Image
            string bgPath = Path.Combine(appDir, "startup_bg_clean.png");
            if (!File.Exists(bgPath)) bgPath = Path.Combine(appDir, "startup_bg.png");
            if (File.Exists(bgPath))
            {
                try {
                    byte[] bytes = File.ReadAllBytes(bgPath);
                    using (MemoryStream ms = new MemoryStream(bytes))
                    {
                        bgImage = new Bitmap(ms);
                    }
                } catch { }
            }
            if (bgImage == null)
            {
                try
                {
                    Assembly asm = Assembly.GetExecutingAssembly();
                    Stream s = asm.GetManifestResourceStream("startup_bg.png");
                    if (s != null) bgImage = Image.FromStream(s);
                }
                catch { }
            }

            // Load Recent Files from AutoCAD Registry MRU
            LoadRecentFiles();

            // Progress Animation Timer
            animTimer = new System.Windows.Forms.Timer();
            animTimer.Interval = 25;
            animTimer.Tick += (s, e) =>
            {
                if (progress < 95)
                {
                    progress += 3;
                    if (progress < 35) statusText = "Đang tối ưu nhân đồ họa AVCAD Engine & Direct3D...";
                    else if (progress < 70) statusText = "Đang kết nối không gian dữ liệu CDE...";
                    else statusText = "Đang nạp bộ menu chuẩn hóa AV_Road, AV_Khảo sát...";
                }
                else if (isReady)
                {
                    progress = 100;
                    statusText = "Sẵn sàng làm việc!";
                    animTimer.Stop();
                }
                this.Invalidate();
            };
            animTimer.Start();

            // Background Preparation Thread
            Thread prepThread = new Thread(() =>
            {
                try
                {
                    Program.ConfigureHardwareAccelerationAndSmoothDrafting(appDir);
                    Program.PrepareProfile(appDir, argProfile, fontPath, plotStylePath, plotterPath);
                }
                catch { }

                isReady = true;
                this.BeginInvoke((Action)(() =>
                {
                    progress = 100;
                    statusText = "Sẵn sàng làm việc!";
                    this.Invalidate();
                }));
            });
            prepThread.IsBackground = true;
            prepThread.Start();
        }

        private void LoadRecentFiles()
        {
            recentItems.Clear();
            try
            {
                using (RegistryKey rk = Registry.CurrentUser.OpenSubKey(@"Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409\Recent File List"))
                {
                    if (rk != null)
                    {
                        for (int i = 1; i <= 9; i++)
                        {
                            object val = rk.GetValue("File" + i);
                            if (val != null && !string.IsNullOrEmpty(val.ToString()))
                            {
                                string full = val.ToString().Trim('"');
                                if (File.Exists(full))
                                {
                                    recentItems.Add(new RecentItem()
                                    {
                                        FileName = Path.GetFileName(full),
                                        FilePath = Path.GetDirectoryName(full) + "\\",
                                        FullPath = full,
                                        TimeText = File.GetLastWriteTime(full).ToString("dd/MM/yyyy HH:mm"),
                                        IsReal = true
                                    });
                                    if (recentItems.Count >= 3) break;
                                }
                            }
                        }
                    }
                }
            }
            catch { }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Draw Clean Background Artwork
            if (bgImage != null)
            {
                g.DrawImage(bgImage, 0, 0, this.Width, this.Height);
            }
            else
            {
                using (LinearGradientBrush brush = new LinearGradientBrush(this.ClientRectangle, Color.FromArgb(7, 35, 26), Color.FromArgb(17, 85, 65), LinearGradientMode.ForwardDiagonal))
                {
                    g.FillRectangle(brush, this.ClientRectangle);
                }
            }

            // Top Window Controls: Min (956, 8), Close (988, 8)
            if (hoveredWin == 0) // Min
            {
                using (SolidBrush b = new SolidBrush(Color.FromArgb(50, 255, 255, 255)))
                    g.FillRectangle(b, new Rectangle(956, 8, 28, 24));
            }
            else if (hoveredWin == 1) // Close
            {
                using (SolidBrush b = new SolidBrush(Color.FromArgb(220, 239, 68, 68)))
                    g.FillRectangle(b, new Rectangle(988, 8, 28, 24));
            }
            using (Pen pWin = new Pen(Color.FromArgb(230, 255, 255, 255), 1.6f))
            {
                g.DrawLine(pWin, 963, 21, 977, 21);
                g.DrawLine(pWin, 996, 14, 1007, 25);
                g.DrawLine(pWin, 1007, 14, 996, 25);
            }

            // 1. Draw Card 1 (Left - Menu Điều Khiển)
            Rectangle card1 = new Rectangle(34, 275, 213, 225);
            using (SolidBrush bCard = new SolidBrush(Color.FromArgb(250, 13, 27, 40)))
            using (Pen pCard = new Pen(Color.FromArgb(180, 28, 62, 90), 1f))
            {
                GraphicsPath gp = CreateRoundedRect(card1, 8);
                g.FillPath(bCard, gp);
                g.DrawPath(pCard, gp);
            }

            // Buttons in Card 1
            Rectangle[] btnRects = new Rectangle[] {
                new Rectangle(42, 285, 197, 36), // 0: Start
                new Rectangle(42, 327, 197, 32), // 1: Open
                new Rectangle(42, 365, 197, 32), // 2: Recent
                new Rectangle(42, 403, 197, 32), // 3: Setting
                new Rectangle(42, 441, 197, 32)  // 4: Info
            };

            // Button 0: Bắt đầu công việc (Hero CTA Button)
            using (LinearGradientBrush bHero = new LinearGradientBrush(btnRects[0], Color.FromArgb(2, 132, 199), Color.FromArgb(13, 148, 136), LinearGradientMode.Horizontal))
            {
                GraphicsPath gp0 = CreateRoundedRect(btnRects[0], 6);
                g.FillPath(bHero, gp0);
                if (hoveredBtn == 0)
                {
                    using (Pen pHover0 = new Pen(Color.FromArgb(255, 94, 234, 212), 2f))
                        g.DrawPath(pHover0, gp0);
                }
            }
            using (Font fHero = new Font("Segoe UI", 9.5f, FontStyle.Bold))
            using (Brush bWhite = new SolidBrush(Color.White))
            {
                g.DrawString("⌂", new Font("Segoe UI", 12f, FontStyle.Bold), bWhite, 52, 290);
                g.DrawString("Bắt đầu công việc", fHero, bWhite, 75, 294);
            }

            // Buttons 1-4
            string[] btnLabels = new string[] { "Mở bản vẽ...", "Dự án gần đây", "Cài đặt AVRoad", "Thông tin & Cập nhật" };
            string[] btnIcons = new string[] { "📁", "🕒", "⚙", "ℹ" };

            using (Font fBtn = new Font("Segoe UI", 9f, FontStyle.Regular))
            {
                for (int i = 1; i <= 4; i++)
                {
                    Rectangle rBtn = btnRects[i];
                    GraphicsPath gpBtn = CreateRoundedRect(rBtn, 6);

                    if (hoveredBtn == i)
                    {
                        using (SolidBrush bHov = new SolidBrush(Color.FromArgb(45, 45, 212, 191)))
                        using (Pen pHov = new Pen(Color.FromArgb(190, 45, 212, 191), 1.5f))
                        {
                            g.FillPath(bHov, gpBtn);
                            g.DrawPath(pHov, gpBtn);
                        }
                    }
                    else
                    {
                        using (SolidBrush bNorm = new SolidBrush(Color.FromArgb(140, 18, 38, 54)))
                        using (Pen pNorm = new Pen(Color.FromArgb(90, 35, 70, 98), 1f))
                        {
                            g.FillPath(bNorm, gpBtn);
                            g.DrawPath(pNorm, gpBtn);
                        }
                    }

                    Color cText = (hoveredBtn == i) ? Color.FromArgb(240, 253, 250) : Color.FromArgb(226, 232, 240);
                    using (Brush bText = new SolidBrush(cText))
                    {
                        g.DrawString(btnIcons[i - 1], new Font("Segoe UI", 8.5f), bText, 54, rBtn.Y + 7);
                        g.DrawString(btnLabels[i - 1], fBtn, bText, 76, rBtn.Y + 7);
                    }
                }
            }

            // 2. Draw Card 2 (Center - Dự án gần đây)
            Rectangle card2 = new Rectangle(262, 275, 365, 225);
            using (SolidBrush bCard = new SolidBrush(Color.FromArgb(250, 13, 27, 40)))
            using (Pen pCard = new Pen(Color.FromArgb(180, 28, 62, 90), 1f))
            {
                GraphicsPath gp = CreateRoundedRect(card2, 8);
                g.FillPath(bCard, gp);
                g.DrawPath(pCard, gp);
            }

            // Card 2 Header
            using (Font fHeader = new Font("Segoe UI", 10f, FontStyle.Bold))
            using (Brush bHeader = new SolidBrush(Color.FromArgb(241, 245, 249)))
            {
                g.DrawString("📁  Dự án gần đây", fHeader, bHeader, 276, 286);
            }

            // Card 2 Recent Items
            Rectangle[] recentRects = new Rectangle[] {
                new Rectangle(270, 316, 348, 46),
                new Rectangle(270, 368, 348, 46),
                new Rectangle(270, 420, 348, 46)
            };

            if (recentItems.Count > 0)
            {
                for (int i = 0; i < recentItems.Count && i < 3; i++)
                {
                    Rectangle r = recentRects[i];
                    RecentItem item = recentItems[i];
                    GraphicsPath gpItem = CreateRoundedRect(r, 6);

                    if (hoveredRecent == i)
                    {
                        using (SolidBrush bHov = new SolidBrush(Color.FromArgb(40, 45, 212, 191)))
                        using (Pen pHov = new Pen(Color.FromArgb(180, 45, 212, 191), 1f))
                        {
                            g.FillPath(bHov, gpItem);
                            g.DrawPath(pHov, gpItem);
                        }
                    }
                    else
                    {
                        using (SolidBrush bNorm = new SolidBrush(Color.FromArgb(100, 18, 38, 54)))
                        {
                            g.FillPath(bNorm, gpItem);
                        }
                    }

                    // DWG Icon Badge (Red)
                    Rectangle rBadge = new Rectangle(r.X + 8, r.Y + 8, 30, 30);
                    using (SolidBrush bBadge = new SolidBrush(Color.FromArgb(220, 38, 38)))
                    {
                        GraphicsPath gpBadge = CreateRoundedRect(rBadge, 4);
                        g.FillPath(bBadge, gpBadge);
                    }
                    using (Font fDwg = new Font("Arial", 6.5f, FontStyle.Bold))
                    using (Brush bDwg = new SolidBrush(Color.White))
                    {
                        g.DrawString("DWG", fDwg, bDwg, r.X + 10, r.Y + 16);
                    }

                    // File Name
                    using (Font fName = new Font("Segoe UI", 9f, FontStyle.Bold))
                    using (Brush bName = new SolidBrush(Color.FromArgb(241, 245, 249)))
                    {
                        g.DrawString(item.FileName, fName, bName, r.X + 44, r.Y + 7);
                    }

                    // Path
                    using (Font fPath = new Font("Segoe UI", 7.5f, FontStyle.Regular))
                    using (Brush bPath = new SolidBrush(Color.FromArgb(148, 163, 184)))
                    {
                        string p = item.FilePath;
                        if (p.Length > 32) p = p.Substring(0, 30) + "...";
                        g.DrawString(p, fPath, bPath, r.X + 44, r.Y + 25);
                    }

                    // Date
                    using (Font fTime = new Font("Segoe UI", 7.5f, FontStyle.Regular))
                    using (Brush bTime = new SolidBrush(Color.FromArgb(100, 116, 139)))
                    {
                        g.DrawString(item.TimeText, fTime, bTime, r.Right - 95, r.Y + 8);
                    }
                }
            }
            else
            {
                using (Font fEmpty = new Font("Segoe UI", 9f, FontStyle.Regular))
                using (Font fSub = new Font("Segoe UI", 8f, FontStyle.Italic))
                using (Brush bEmpty = new SolidBrush(Color.FromArgb(148, 163, 184)))
                using (Brush bSub = new SolidBrush(Color.FromArgb(100, 116, 139)))
                {
                    g.DrawString("Chưa có dự án bản vẽ mở gần đây.", fEmpty, bEmpty, 290, 360);
                    g.DrawString("Bấm 'Mở bản vẽ...' hoặc 'Bắt đầu công việc' để thiết kế.", fSub, bSub, 280, 385);
                }
            }

            // 3. Draw Card 3 (Right - Tin tức & Cập nhật)
            Rectangle card3 = new Rectangle(642, 275, 350, 225);
            using (SolidBrush bCard = new SolidBrush(Color.FromArgb(250, 13, 27, 40)))
            using (Pen pCard = new Pen(Color.FromArgb(180, 28, 62, 90), 1f))
            {
                GraphicsPath gp = CreateRoundedRect(card3, 8);
                g.FillPath(bCard, gp);
                g.DrawPath(pCard, gp);
            }

            // Card 3 Header
            using (Font fHeader = new Font("Segoe UI", 10f, FontStyle.Bold))
            using (Brush bHeader = new SolidBrush(Color.FromArgb(241, 245, 249)))
            {
                g.DrawString("📢  Tin tức & Cập nhật", fHeader, bHeader, 656, 286);
            }

            // Card 3 News Items
            Rectangle[] newsRects = new Rectangle[] {
                new Rectangle(650, 316, 334, 34),
                new Rectangle(650, 354, 334, 32),
                new Rectangle(650, 390, 334, 32),
                new Rectangle(650, 426, 334, 32)
            };

            for (int i = 0; i < newsRects.Length; i++)
            {
                Rectangle rN = newsRects[i];
                if (hoveredNews == i)
                {
                    using (SolidBrush bHov = new SolidBrush(Color.FromArgb(30, 255, 255, 255)))
                    {
                        GraphicsPath gpHov = CreateRoundedRect(rN, 4);
                        g.FillPath(bHov, gpHov);
                    }
                }

                if (i == 0)
                {
                    using (SolidBrush bPill = new SolidBrush(Color.FromArgb(16, 185, 129)))
                    {
                        GraphicsPath gpPill = CreateRoundedRect(new Rectangle(rN.X + 6, rN.Y + 8, 32, 18), 3);
                        g.FillPath(bPill, gpPill);
                    }
                    using (Font fPill = new Font("Segoe UI", 7f, FontStyle.Bold))
                    using (Brush bWhite = new SolidBrush(Color.White))
                    {
                        g.DrawString("Mới", fPill, bWhite, rN.X + 11, rN.Y + 10);
                    }
                    using (Font fNews = new Font("Segoe UI", 8.5f, FontStyle.Bold))
                    using (Brush bNews = new SolidBrush(Color.FromArgb(241, 245, 249)))
                    {
                        g.DrawString("AVRoad 2026 – Nâng cấp hiệu năng & CDE", fNews, bNews, rN.X + 44, rN.Y + 9);
                    }
                }
                else
                {
                    using (SolidBrush bDot = new SolidBrush(Color.FromArgb(45, 212, 191)))
                    {
                        g.FillEllipse(bDot, rN.X + 16, rN.Y + 12, 6, 6);
                    }
                    string newsText = "";
                    if (i == 1) newsText = "Chuẩn hóa toàn diện hệ thống menu AV_";
                    else if (i == 2) newsText = "Đồ họa Direct3D 9 & Khử răng cưa nét vẽ mượt";
                    else if (i == 3) newsText = "Tích hợp trung tâm AV Setting ngay trên Hub";

                    using (Font fNews = new Font("Segoe UI", 8.5f, FontStyle.Regular))
                    using (Brush bNews = new SolidBrush(Color.FromArgb(203, 213, 225)))
                    {
                        g.DrawString(newsText, fNews, bNews, rN.X + 32, rN.Y + 8);
                    }
                }
            }

            // 4. Draw Bottom Status Text
            using (Font fStatus = new Font("Segoe UI", 9f, FontStyle.Regular))
            using (Brush bStatus = new SolidBrush(isReady ? Color.FromArgb(45, 212, 191) : Color.FromArgb(203, 213, 225)))
            {
                g.DrawString(statusText, fStatus, bStatus, 34, 514);
            }

            // 5. Draw Bottom Progress Bar Track
            int pbX = 34, pbY = 536, pbW = 290, pbH = 6;
            using (SolidBrush bTrack = new SolidBrush(Color.FromArgb(15, 30, 45)))
            {
                GraphicsPath trackPath = CreateRoundedRect(new Rectangle(pbX, pbY, pbW, pbH), 3);
                g.FillPath(bTrack, trackPath);
            }

            // Progress Bar Fill
            int fillW = (int)(pbW * (progress / 100.0));
            if (fillW > 4)
            {
                using (LinearGradientBrush bFill = new LinearGradientBrush(new Rectangle(pbX, pbY, fillW, pbH), Color.FromArgb(6, 182, 212), Color.FromArgb(45, 212, 191), LinearGradientMode.Horizontal))
                {
                    GraphicsPath fillPath = CreateRoundedRect(new Rectangle(pbX, pbY, fillW, pbH), 3);
                    g.FillPath(bFill, fillPath);
                }
            }

            // 6. Draw Version Label
            using (Font fVer = new Font("Segoe UI", 8.5f, FontStyle.Regular))
            using (Brush bVer = new SolidBrush(Color.FromArgb(148, 163, 184)))
            {
                g.DrawString("Phiên bản: AVRoad 2026 v1.4", fVer, bVer, 840, 526);
            }
        }

        private GraphicsPath CreateRoundedRect(Rectangle r, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int prevBtn = hoveredBtn;
            int prevRecent = hoveredRecent;
            int prevNews = hoveredNews;
            int prevWin = hoveredWin;

            hoveredBtn = -1;
            hoveredRecent = -1;
            hoveredNews = -1;
            hoveredWin = -1;

            // Window Buttons
            if (new Rectangle(956, 8, 28, 24).Contains(e.Location)) hoveredWin = 0;
            else if (new Rectangle(988, 8, 28, 24).Contains(e.Location)) hoveredWin = 1;

            // Left Buttons (Exactly matching btnRects)
            Rectangle[] btnRects = new Rectangle[] {
                new Rectangle(42, 285, 197, 36),
                new Rectangle(42, 327, 197, 32),
                new Rectangle(42, 365, 197, 32),
                new Rectangle(42, 403, 197, 32),
                new Rectangle(42, 441, 197, 32)
            };
            for (int i = 0; i < btnRects.Length; i++)
            {
                if (btnRects[i].Contains(e.Location)) { hoveredBtn = i; break; }
            }

            // Recent Projects
            Rectangle[] recentRects = new Rectangle[] {
                new Rectangle(270, 316, 348, 46),
                new Rectangle(270, 368, 348, 46),
                new Rectangle(270, 420, 348, 46)
            };
            for (int i = 0; i < recentRects.Length; i++)
            {
                if (recentRects[i].Contains(e.Location)) { hoveredRecent = i; break; }
            }

            // News Items
            Rectangle[] newsRects = new Rectangle[] {
                new Rectangle(650, 316, 334, 34),
                new Rectangle(650, 354, 334, 32),
                new Rectangle(650, 390, 334, 32),
                new Rectangle(650, 426, 334, 32)
            };
            for (int i = 0; i < newsRects.Length; i++)
            {
                if (newsRects[i].Contains(e.Location)) { hoveredNews = i; break; }
            }

            if (hoveredBtn != -1 || hoveredRecent != -1 || hoveredNews != -1 || hoveredWin != -1)
                this.Cursor = Cursors.Hand;
            else
                this.Cursor = Cursors.Default;

            if (hoveredBtn != prevBtn || hoveredRecent != prevRecent || hoveredNews != prevNews || hoveredWin != prevWin)
            {
                this.Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            // Drag window by header
            if (e.Button == MouseButtons.Left && e.Y < 50 && e.X < 920)
            {
                ReleaseCapture();
                SendMessage(this.Handle, 0xA1, 2, 0);
                return;
            }

            if (e.Button == MouseButtons.Left)
            {
                // Window Buttons
                if (hoveredWin == 0)
                {
                    this.WindowState = FormWindowState.Minimized;
                    return;
                }
                else if (hoveredWin == 1)
                {
                    this.Close();
                    Application.Exit();
                    return;
                }

                // Left Navigation Buttons
                if (hoveredBtn == 0) // Bắt đầu công việc
                {
                    LaunchCAD("");
                }
                else if (hoveredBtn == 1) // Mở bản vẽ...
                {
                    using (OpenFileDialog ofd = new OpenFileDialog())
                    {
                        ofd.Title = "Chọn Bản Vẽ Thiết Kế Hạ Tầng - AVRoad 2026";
                        ofd.Filter = "AutoCAD Drawing (*.dwg)|*.dwg|DXF Files (*.dxf)|*.dxf|Tất cả tệp (*.*)|*.*";
                        if (ofd.ShowDialog(this) == DialogResult.OK)
                        {
                            LaunchCAD(ofd.FileName);
                        }
                    }
                }
                else if (hoveredBtn == 2) // Dự án gần đây
                {
                    if (recentItems.Count > 0 && File.Exists(recentItems[0].FullPath))
                    {
                        LaunchCAD(recentItems[0].FullPath);
                    }
                    else
                    {
                        LaunchCAD("");
                    }
                }
                else if (hoveredBtn == 3) // Cài đặt AVRoad
                {
                    OpenSettingsDialog();
                }
                else if (hoveredBtn == 4) // Thông tin & Cập nhật
                {
                    OpenAboutOrUpdate();
                }

                // Recent Projects Click
                if (hoveredRecent >= 0 && hoveredRecent < recentItems.Count)
                {
                    RecentItem it = recentItems[hoveredRecent];
                    if (it.IsReal && File.Exists(it.FullPath))
                    {
                        LaunchCAD(it.FullPath);
                    }
                    else
                    {
                        LaunchCAD("");
                    }
                }

                // News Click
                if (hoveredNews >= 0)
                {
                    OpenAboutOrUpdate();
                }
            }
        }

        private void LaunchCAD(string dwgPath)
        {
            this.Hide();
            Program.LaunchAutoCAD(appDir, checkUpdateOnStartup, dwgPath);
            this.Close();
        }

        private void OpenSettingsDialog()
        {
            string settingExe = Path.Combine(appDir, "AVSetting.exe");
            if (File.Exists(settingExe))
            {
                try
                {
                    using (Process p = Process.Start(new ProcessStartInfo()
                    {
                        FileName = settingExe,
                        WorkingDirectory = appDir
                    }))
                    {
                        if (p != null) p.WaitForExit();
                    }
                }
                catch { }
            }
            else
            {
                MessageBox.Show("Trung tâm cấu hình AVRoad Settings đã được tích hợp hoàn chỉnh.", "AVRoad 2026 Solution", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void OpenAboutOrUpdate()
        {
            try
            {
                Program.CheckUpdateAsync(appDir, null, true);
            }
            catch
            {
                MessageBox.Show("AVRoad 2026 Solution - Hệ sinh thái AV BIM CDE\nPhiên bản 2026.0.0 (Unlocked Engine)\n\nChuẩn hóa dữ liệu thiết kế hạ tầng giao thông.", "AVRoad 2026 Solution", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        static Form globalSplash = null;

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
                    MessageBox.Show("AVRoad 2026 Solution - Hệ sinh thái AV BIM CDE\nPhiên bản 2026.0.0 (Unlocked Engine)\n\nChuẩn hóa dữ liệu thiết kế hạ tầng giao thông.", "AVRoad 2026 Solution", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                if (a.Equals("/check-update", StringComparison.OrdinalIgnoreCase) || a.Equals("--check-update-toast", StringComparison.OrdinalIgnoreCase))
                {
                    CheckUpdateAsync(appDir, null, true);
                    return;
                }
            }

            string fontPath = "";
            string plotStylePath = Path.Combine(appDir, "Plot Styles");
            string plotterPath = Path.Combine(appDir, "Plotters");
            string argProfile = Path.Combine(appDir, "AVRoad_Profile.arg");
            bool checkUpdateOnStartup = true;

            if (File.Exists(iniPath))
            {
                foreach (string l in File.ReadAllLines(iniPath))
                {
                    int eq = l.IndexOf('=');
                    if (eq > 0)
                    {
                        string k = l.Substring(0, eq).Trim();
                        string v = l.Substring(eq + 1).Trim();
                        if (k == "CheckUpdateOnStartup" && v == "0") checkUpdateOnStartup = false;
                        if (k == "FontPath" && !string.IsNullOrEmpty(v)) fontPath = v;
                        if (k == "PlotStylePath" && !string.IsNullOrEmpty(v)) plotStylePath = v;
                        if (k == "PlotterPath" && !string.IsNullOrEmpty(v)) plotterPath = v;
                        if (k == "ArgProfilePath" && !string.IsNullOrEmpty(v)) argProfile = v;
                    }
                }
            }

            // If a DWG file was passed directly (via file association)
            string directFile = "";
            foreach (string a in args)
            {
                if (!a.StartsWith("/") && !a.StartsWith("-") && File.Exists(a) && (a.EndsWith(".dwg", StringComparison.OrdinalIgnoreCase) || a.EndsWith(".dxf", StringComparison.OrdinalIgnoreCase)))
                {
                    directFile = a;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(directFile))
            {
                ConfigureHardwareAccelerationAndSmoothDrafting(appDir);
                PrepareProfile(appDir, argProfile, fontPath, plotStylePath, plotterPath);
                LaunchAutoCAD(appDir, checkUpdateOnStartup, directFile);
                return;
            }

            // Launch Modern Startup Screen Hub
            Application.Run(new AVStartupForm(appDir, iniPath, fontPath, plotStylePath, plotterPath, argProfile, checkUpdateOnStartup));
        }

        public static void ConfigureHardwareAccelerationAndSmoothDrafting(string appDir)
        {
            try
            {
                string drvPath = Path.Combine(appDir, "Drv");
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
                        prk.SetValue("MaxTessellations", 16, RegistryValueKind.DWord);
                        prk.SetValue("SurfaceTessellationTol", 92, RegistryValueKind.DWord);
                        prk.SetValue("CurveTessellationTol", 87, RegistryValueKind.DWord);
                    }
                }

                string key3DGS = @"Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409\3DGS Configuration\GSHEIDI10";
                using (RegistryKey rk = Registry.CurrentUser.CreateSubKey(key3DGS))
                {
                    if (rk != null)
                    {
                        rk.SetValue("Help", Path.Combine(appDir, "acad.chm"), RegistryValueKind.String);
                        rk.SetValue("ContextHelp", "", RegistryValueKind.String);
                        rk.SetValue("FirstRun", 1, RegistryValueKind.DWord);
                        rk.SetValue("Flags", 4721767, RegistryValueKind.DWord); // HW Accel + Smooth lines + Geometry
                        rk.SetValue("CustomHeidiDriver", "direct3d9.hdi", RegistryValueKind.String);
                        rk.SetValue("CustomHeidiDriverPath", drvPath, RegistryValueKind.String);
                        rk.SetValue("DriverName", "direct3d9.hdi", RegistryValueKind.String);
                        rk.SetValue("FramesPerSecond", 5, RegistryValueKind.DWord);
                        rk.SetValue("MaxTessellations", 16, RegistryValueKind.DWord); // Fixes 1-60 dialog error
                        rk.SetValue("SurfaceTessellationTol", 92, RegistryValueKind.DWord);
                        rk.SetValue("CurveTessellationTol", 87, RegistryValueKind.DWord);
                        rk.SetValue("PTCardAvaliable", 1, RegistryValueKind.DWord); // Enables HW checkbox
                        rk.SetValue("PTCardRecomended", 1, RegistryValueKind.DWord);
                        rk.SetValue("PTCardCertified", 1, RegistryValueKind.DWord);
                        rk.SetValue("PTUsingD3D", 1, RegistryValueKind.DWord);
                        rk.SetValue("PTUsingOGL", 0, RegistryValueKind.DWord);
                        rk.SetValue("PTWorkArounds", "0:", RegistryValueKind.String);
                        rk.SetValue("SNAPSHOTFLAGS", 103, RegistryValueKind.DWord);
                        rk.SetValue("SNAPSHOTCACHETESS", 16, RegistryValueKind.DWord);
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

                // Attempt to write HKLM if running with elevation
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
                    }
                }
                using (RegistryKey edKey = Registry.CurrentUser.CreateSubKey(@"Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409\Profiles\AVRoad\Editor Configuration"))
                {
                    if (edKey != null)
                    {
                        edKey.SetValue("ZoomFactor", 60, RegistryValueKind.DWord);
                    }
                }
            }
            catch { }
        }

        public static void PrepareProfile(string appDir, string argProfile, string fontPath, string plotStylePath, string plotterPath)
        {
            // Configure Hardware Acceleration, Direct3D, Smooth Line Display, and Tessellations
            ConfigureHardwareAccelerationAndSmoothDrafting(appDir);

            string profilesKeyPath = @"Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409\Profiles";

            // 1. Configure Command Line Window font to Arial across ALL profiles
            try
            {
                using (RegistryKey pk = Registry.CurrentUser.OpenSubKey(profilesKeyPath, true))
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

            // 2. Remove acmgd.dll from TabExtensions to stop "invalid ARX file" errors
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

// 3. TDTObjectsWrapper is loaded natively via ObjectDBX registry keys


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
                        cleanAcad += supportPath + ";" + fontsPath + ";" + expressPath + ";" + colorPath + ";" + helpPath + ";" +
                                     Path.Combine(appDir, "Data") + ";" +
                                     Path.Combine(appDir, @"Data\Symbols") + ";" +
                                     Path.Combine(appDir, @"Data\Bien bao") + ";" +
                                     Path.Combine(appDir, @"Data\Color") + ";" +
                                     Path.Combine(appDir, @"Data\Ltype") + ";" +
                                     Path.Combine(appDir, "Lib") + ";" +
                                     Path.Combine(appDir, "CTTK") + ";" +
                                     Path.Combine(appDir, "DCCT") + ";" +
                                     Path.Combine(appDir, "Blocks") + ";" +
                                     Path.Combine(appDir, "Template");

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
                                string chmTarget = Path.Combine(helpPath, "acad.chm");
                                if (!File.Exists(chmTarget)) chmTarget = Path.Combine(appDir, "acad.chm");
                                genKey.SetValue("ACADHELP", chmTarget);
                                genKey.SetValue("HelpFilePath", helpPath);
                                try {
                                    if (!Directory.Exists(helpPath)) Directory.CreateDirectory(helpPath);
                                    if (!File.Exists(Path.Combine(helpPath, "acad.chm")) && File.Exists(Path.Combine(appDir, "acad.chm"))) {
                                        File.Copy(Path.Combine(appDir, "acad.chm"), Path.Combine(helpPath, "acad.chm"), true);
                                    }
                                } catch { }
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
                            using (RegistryKey opmKey = Registry.CurrentUser.CreateSubKey(@"Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409\Applications\AcOpm"))
                            {
                                if (opmKey != null)
                                {
                                    opmKey.SetValue("LOADCTRLS", 14, RegistryValueKind.DWord);
                                    opmKey.SetValue("LOADER", Path.Combine(appDir, "acopm.arx"), RegistryValueKind.String);
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
                            using (RegistryKey tdtObjKey1 = Registry.CurrentUser.CreateSubKey(@"Software\Autodesk\ObjectDBX\R17.0\Applications\TDTObjects"))
                            {
                                if (tdtObjKey1 != null)
                                {
                                    tdtObjKey1.SetValue("LOADCTRLS", 9, RegistryValueKind.DWord);
                                    tdtObjKey1.SetValue("LOADER", Path.Combine(appDir, "TDTObjects17.dbx"), RegistryValueKind.String);
                                    tdtObjKey1.SetValue("DESCRIPTION", "TDT Objects version 1.0", RegistryValueKind.String);
                                }
                            }
                            using (RegistryKey tdtObjKey2 = Registry.CurrentUser.CreateSubKey(@"Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409\Applications\TDTObjects"))
                            {
                                if (tdtObjKey2 != null)
                                {
                                    tdtObjKey2.SetValue("LOADCTRLS", 9, RegistryValueKind.DWord);
                                    tdtObjKey2.SetValue("LOADER", Path.Combine(appDir, "TDTObjects17.dbx"), RegistryValueKind.String);
                                    tdtObjKey2.SetValue("DESCRIPTION", "TDT Objects version 1.0", RegistryValueKind.String);
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
                                menuKey.SetValue("Group2", "EXPRESS express");
                                menuKey.SetValue("Group3", "KSVN KSVN");
                                menuKey.SetValue("Group4", "VNROAD VNROAD");
                                menuKey.SetValue("Group5", "SANNEN SANNEN");
                                menuKey.SetValue("Group6", "KVN KVN");
                                menuKey.SetValue("Group7", "DCCT DCCT");
                                menuKey.SetValue("Group8", "TDT3D TDT3D");
                                menuKey.SetValue("Group9", "RM RM");
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

        public static void LaunchAutoCAD(string appDir, bool checkUpdateOnStartup, string dwgPath = "")
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
            if (string.IsNullOrEmpty(dwgPath))
                psi.Arguments = "/nologo /p AVRoad";
            else
                psi.Arguments = "/nologo /p AVRoad \"" + dwgPath + "\"";
            psi.WorkingDirectory = appDir;
            psi.UseShellExecute = false;

            Process acadProc = null;
            try
            {
                acadProc = Process.Start(psi);
            }
            catch (Exception ex)
            {
                string vcRedist = Path.Combine(appDir, "vcredist_x86.exe");
                if (File.Exists(vcRedist) && (ex.Message.IndexOf("side-by-side", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("14001") >= 0))
                {
                    try
                    {
                        ProcessStartInfo psiVc = new ProcessStartInfo(vcRedist, "/q:a /c:\"vcredist.msi /qn\"");
                        psiVc.Verb = "runas";
                        psiVc.UseShellExecute = true;
                        Process pVc = Process.Start(psiVc);
                        if (pVc != null) pVc.WaitForExit(45000);
                        acadProc = Process.Start(psi);
                    }
                    catch { }
                }

                if (acadProc == null)
                {
                    CloseSplashSafe();
                    MessageBox.Show("Lỗi khởi động AVRoad: " + ex.Message + "\n\n(Nếu gặp lỗi side-by-side configuration, vui lòng chạy file vcredist_x86.exe trong thư mục cài đặt để bổ sung Visual C++ 2005 Runtime)", "AVRoad Solution", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
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
                        CheckUpdateAsync(appDir, acadProc, false);
                    });
                    updateCheckThread.SetApartmentState(ApartmentState.STA);
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

        public static void CheckUpdateAsync(string appDir, Process acadProc, bool isManual)
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
                if (!mRemote.Success)
                {
                    if (isManual) MessageBox.Show("Không thể đọc thông tin phiên bản mới từ máy chủ.", "AVRoad 2026", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

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
                else
                {
                    if (isManual)
                    {
                        MessageBox.Show("AVRoad 2026 hiện đang là phiên bản mới nhất (" + localVer + ").\nHệ thống đã được cập nhật đầy đủ và ổn định.", "AVRoad 2026 Solution", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                if (isManual)
                {
                    MessageBox.Show("Lỗi kết nối máy chủ cập nhật: " + ex.Message, "AVRoad 2026", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                try { File.AppendAllText(Path.Combine(appDir, "update-check.log"), DateTime.Now.ToString("s") + " Update check failed: " + ex.Message + Environment.NewLine); } catch { }
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


}