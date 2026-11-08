using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace AVRoad.Security
{
    /// <summary>
    /// Hộp thoại thông báo nhẹ nhàng góc dưới bên phải màn hình (Toast notification)
    /// khi phần mềm cần cập nhật phiên bản mới (thiết bị trong danh sách thu hồi).
    /// </summary>
    public sealed class StealthNoticeToast : Form
    {
        private static StealthNoticeToast _instance;
        private static readonly object _lock = new object();

        public static void ShowNotice()
        {
            try
            {
                // In ra dòng lệnh AutoCAD nếu đang chạy trong AutoCAD
                Type acAppType = Type.GetType("Autodesk.AutoCAD.ApplicationServices.Application, acmgd");
                if (acAppType != null)
                {
                    PropertyInfo propDocMgr = acAppType.GetProperty("DocumentManager");
                    if (propDocMgr != null)
                    {
                        object docMgr = propDocMgr.GetValue(null, null);
                        if (docMgr != null)
                        {
                            PropertyInfo propMdi = docMgr.GetType().GetProperty("MdiActiveDocument");
                            if (propMdi != null)
                            {
                                object doc = propMdi.GetValue(docMgr, null);
                                if (doc != null)
                                {
                                    PropertyInfo propEd = doc.GetType().GetProperty("Editor");
                                    if (propEd != null)
                                    {
                                        object ed = propEd.GetValue(doc, null);
                                        if (ed != null)
                                        {
                                            MethodInfo writeMsg = ed.GetType().GetMethod("WriteMessage", new Type[] { typeof(string) });
                                            if (writeMsg != null)
                                            {
                                                writeMsg.Invoke(ed, new object[] { "\n[AVRoad] Phần mềm đã cũ, bạn vui lòng cập nhật phiên bản mới để tiếp tục sử dụng.\n" });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { }

            try
            {
                lock (_lock)
                {
                    if (_instance != null && !_instance.IsDisposed)
                    {
                        if (_instance.InvokeRequired)
                        {
                            _instance.BeginInvoke(new MethodInvoker(delegate {
                                _instance.BringToFront();
                            }));
                        }
                        else
                        {
                            _instance.BringToFront();
                        }
                        return;
                    }

                    _instance = new StealthNoticeToast();
                    _instance.Show();
                }
            }
            catch
            {
                try
                {
                    MessageBox.Show(
                        "Phần mềm đã cũ, bạn vui lòng cập nhật phiên bản mới để tiếp tục sử dụng.",
                        "Thông báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch { }
            }
        }

        public StealthNoticeToast()
        {
            this.Text = "Thông báo";
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.StartPosition = FormStartPosition.Manual;
            this.ClientSize = new Size(390, 135);
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 9.25f, FontStyle.Regular);

            Rectangle wa = Screen.PrimaryScreen.WorkingArea;
            this.Left = wa.Right - this.Width - 20;
            this.Top = wa.Bottom - this.Height - 20;

            Label lblTitle = new Label();
            lblTitle.Text = "THÔNG BÁO CẬP NHẬT";
            lblTitle.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(30, 41, 59);
            lblTitle.Location = new Point(16, 14);
            lblTitle.AutoSize = true;

            Label lblMsg = new Label();
            lblMsg.Text = "Phần mềm đã cũ, bạn vui lòng cập nhật phiên bản mới để tiếp tục sử dụng.";
            lblMsg.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            lblMsg.ForeColor = Color.FromArgb(71, 85, 105);
            lblMsg.Location = new Point(16, 42);
            lblMsg.Size = new Size(355, 42);

            Button btnClose = new Button();
            btnClose.Text = "Đóng";
            btnClose.Location = new Point(292, 92);
            btnClose.Size = new Size(80, 30);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.BackColor = Color.FromArgb(241, 245, 249);
            btnClose.ForeColor = Color.FromArgb(30, 41, 59);
            btnClose.Cursor = Cursors.Hand;
            btnClose.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnClose.Click += delegate { this.Close(); };

            this.Controls.Add(lblTitle);
            this.Controls.Add(lblMsg);
            this.Controls.Add(btnClose);
        }
    }
}
