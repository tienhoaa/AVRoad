using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace AVRoad.Security
{
    /// <summary>
    /// Giao diện WinForms siêu tinh gọn quản lý và kích hoạt bản quyền AVRoad 2026 - TDT Solution 9.1.
    /// Tương thích 100% .NET 2.0 / 3.5 / 4.0+.
    /// </summary>
    public class LicenseDialog : Form
    {
        private static bool _isShowingDialog = false;

        /// <summary>
        /// Hiển thị hộp thoại GetKey / Kích hoạt bản quyền dưới dạng modal độc lập.
        /// Tự động chặn mở nhiều hộp thoại trùng lặp.
        /// </summary>
        public static void ShowActivationDialog()
        {
            ShowActivationDialog(null);
        }

        /// <summary>
        /// Hiển thị hộp thoại GetKey / Kích hoạt bản quyền gắn với cửa sổ cha.
        /// </summary>
        public static void ShowActivationDialog(IWin32Window owner)
        {
            if (_isShowingDialog) return;
            _isShowingDialog = true;
            try
            {
                using (LicenseDialog dlg = new LicenseDialog())
                {
                    dlg.StartPosition = (owner != null) ? FormStartPosition.CenterParent : FormStartPosition.CenterScreen;
                    if (owner != null)
                    {
                        dlg.ShowDialog(owner);
                    }
                    else
                    {
                        dlg.TopMost = true;
                        dlg.ShowDialog();
                    }
                }
            }
            catch { }
            finally
            {
                _isShowingDialog = false;
            }
        }

        private TextBox txtHwid;
        private Button btnCopyHwid;

        private TextBox txtCustomerName;
        private Button btnExportRequest;

        private Panel panelStatusBadge;
        private Label lblStatusTitle;
        private Label lblStatusDesc;
        private Label lblDetailsCustomer;
        private Label lblDetailsExpiry;
        private Button btnImportLicense;

        private Button btnRefresh;
        private Button btnClose;

        public LicenseDialog()
        {
            InitializeComponent();
            RefreshStatusDisplay();
        }

        private void InitializeComponent()
        {
            this.Text = "AVRoad 2026 - TDT Solution 9.1 • Bản quyền phần mềm";
            this.Size = new Size(520, 560);
            this.MinimumSize = new Size(520, 560);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Font = new Font("Segoe UI", 9.25f, FontStyle.Regular);

            try
            {
                string iconPath = @"C:\AVRoad_2026\AVRoad.ico";
                if (!File.Exists(iconPath)) iconPath = @"C:\TDT_RESEARCH\AVRoad.ico";
                if (File.Exists(iconPath))
                {
                    this.Icon = new Icon(iconPath);
                }
            }
            catch { }

            // ================= 1. HEADER (MODERN DEEP NAVY) =================
            Panel headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 72;
            headerPanel.BackColor = Color.FromArgb(15, 23, 42); // Slate 900

            Panel accentLine = new Panel();
            accentLine.Dock = DockStyle.Bottom;
            accentLine.Height = 3;
            accentLine.BackColor = Color.FromArgb(37, 99, 235); // Blue 600
            headerPanel.Controls.Add(accentLine);

            Label lblHeaderTitle = new Label();
            lblHeaderTitle.Text = "AVRoad 2026 - TDT Solution 9.1";
            lblHeaderTitle.ForeColor = Color.White;
            lblHeaderTitle.Font = new Font("Segoe UI", 12.5f, FontStyle.Bold);
            lblHeaderTitle.Location = new Point(20, 14);
            lblHeaderTitle.AutoSize = true;

            Label lblHeaderSub = new Label();
            lblHeaderSub.Text = "Đăng ký & Kích hoạt bản quyền thiết bị (Offline 100%)";
            lblHeaderSub.ForeColor = Color.FromArgb(148, 163, 184); // Slate 400
            lblHeaderSub.Font = new Font("Segoe UI", 9f, FontStyle.Regular);
            lblHeaderSub.Location = new Point(21, 40);
            lblHeaderSub.AutoSize = true;

            headerPanel.Controls.Add(lblHeaderTitle);
            headerPanel.Controls.Add(lblHeaderSub);
            this.Controls.Add(headerPanel);

            // ================= BODY CONTAINER =================
            Panel bodyPanel = new Panel();
            bodyPanel.Location = new Point(18, 82);
            bodyPanel.Size = new Size(470, 410);
            bodyPanel.BackColor = Color.Transparent;
            this.Controls.Add(bodyPanel);

            int curY = 4;

            // ================= 2. DEVICE INFO (HWID ONLY) =================
            GroupBox grpDevice = new GroupBox();
            grpDevice.Text = " 1. Mã nhận diện máy tính (HWID) ";
            grpDevice.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            grpDevice.ForeColor = Color.FromArgb(30, 41, 59);
            grpDevice.Location = new Point(0, curY);
            grpDevice.Size = new Size(470, 72);
            grpDevice.BackColor = Color.White;

            txtHwid = new TextBox();
            txtHwid.Text = HardwareIdentifier.GetHardwareId();
            txtHwid.ReadOnly = true;
            txtHwid.BackColor = Color.FromArgb(241, 245, 249);
            txtHwid.BorderStyle = BorderStyle.FixedSingle;
            txtHwid.Font = new Font("Consolas", 10.5f, FontStyle.Bold);
            txtHwid.ForeColor = Color.FromArgb(15, 23, 42);
            txtHwid.Location = new Point(16, 28);
            txtHwid.Size = new Size(320, 26);

            btnCopyHwid = new Button();
            btnCopyHwid.Text = "📋 Sao chép";
            btnCopyHwid.Font = new Font("Segoe UI", 8.75f, FontStyle.Regular);
            btnCopyHwid.Location = new Point(344, 26);
            btnCopyHwid.Size = new Size(110, 28);
            btnCopyHwid.FlatStyle = FlatStyle.Flat;
            btnCopyHwid.BackColor = Color.FromArgb(241, 245, 249);
            btnCopyHwid.ForeColor = Color.FromArgb(30, 41, 59);
            btnCopyHwid.Cursor = Cursors.Hand;
            btnCopyHwid.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnCopyHwid.Click += delegate
            {
                try
                {
                    Clipboard.SetText(txtHwid.Text);
                    btnCopyHwid.Text = "✓ Đã chép";
                    btnCopyHwid.BackColor = Color.FromArgb(220, 252, 231);
                    Timer timer = new Timer();
                    timer.Interval = 2000;
                    timer.Tick += delegate
                    {
                        btnCopyHwid.Text = "📋 Sao chép";
                        btnCopyHwid.BackColor = Color.FromArgb(241, 245, 249);
                        timer.Stop();
                        timer.Dispose();
                    };
                    timer.Start();
                }
                catch { }
            };

            grpDevice.Controls.Add(txtHwid);
            grpDevice.Controls.Add(btnCopyHwid);
            bodyPanel.Controls.Add(grpDevice);

            curY += 80;

            // ================= 3. REGISTRATION (ONLY FULL NAME) =================
            GroupBox grpReq = new GroupBox();
            grpReq.Text = " 2. Đăng ký thông tin người sử dụng ";
            grpReq.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            grpReq.ForeColor = Color.FromArgb(30, 41, 59);
            grpReq.Location = new Point(0, curY);
            grpReq.Size = new Size(470, 118);
            grpReq.BackColor = Color.White;

            Label lblCust = new Label();
            lblCust.Text = "Họ và tên người dùng (*):";
            lblCust.Font = new Font("Segoe UI", 8.75f, FontStyle.Regular);
            lblCust.ForeColor = Color.FromArgb(71, 85, 105);
            lblCust.Location = new Point(14, 24);
            lblCust.Size = new Size(200, 18);

            txtCustomerName = new TextBox();
            txtCustomerName.Font = new Font("Segoe UI", 10f);
            txtCustomerName.Location = new Point(16, 44);
            txtCustomerName.Size = new Size(438, 27);
            txtCustomerName.BorderStyle = BorderStyle.FixedSingle;
            // Pre-fill with system user if empty
            txtCustomerName.Text = Environment.UserName;

            btnExportRequest = new Button();
            btnExportRequest.Text = "📤 Xuất yêu cầu cấp bản quyền (.avreq)";
            btnExportRequest.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            btnExportRequest.Location = new Point(16, 78);
            btnExportRequest.Size = new Size(438, 32);
            btnExportRequest.FlatStyle = FlatStyle.Flat;
            btnExportRequest.BackColor = Color.FromArgb(30, 64, 175); // Blue 700
            btnExportRequest.ForeColor = Color.White;
            btnExportRequest.Cursor = Cursors.Hand;
            btnExportRequest.FlatAppearance.BorderSize = 0;
            btnExportRequest.Click += BtnExportRequest_Click;

            grpReq.Controls.Add(lblCust);
            grpReq.Controls.Add(txtCustomerName);
            grpReq.Controls.Add(btnExportRequest);
            bodyPanel.Controls.Add(grpReq);

            curY += 126;

            // ================= 4. STATUS & ACTIVATION =================
            GroupBox grpStatus = new GroupBox();
            grpStatus.Text = " 3. Trạng thái bản quyền & Kích hoạt ";
            grpStatus.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            grpStatus.ForeColor = Color.FromArgb(30, 41, 59);
            grpStatus.Location = new Point(0, curY);
            grpStatus.Size = new Size(470, 180);
            grpStatus.BackColor = Color.White;

            panelStatusBadge = new Panel();
            panelStatusBadge.Location = new Point(16, 24);
            panelStatusBadge.Size = new Size(438, 54);
            panelStatusBadge.BackColor = Color.FromArgb(241, 245, 249);

            lblStatusTitle = new Label();
            lblStatusTitle.Text = "● ĐANG KIỂM TRA TRẠNG THÁI...";
            lblStatusTitle.Font = new Font("Segoe UI", 9.75f, FontStyle.Bold);
            lblStatusTitle.Location = new Point(10, 8);
            lblStatusTitle.AutoSize = true;

            lblStatusDesc = new Label();
            lblStatusDesc.Text = "Vui lòng chờ giây lát...";
            lblStatusDesc.Font = new Font("Segoe UI", 8.5f);
            lblStatusDesc.ForeColor = Color.FromArgb(100, 116, 139);
            lblStatusDesc.Location = new Point(11, 28);
            lblStatusDesc.Size = new Size(415, 20);

            panelStatusBadge.Controls.Add(lblStatusTitle);
            panelStatusBadge.Controls.Add(lblStatusDesc);
            grpStatus.Controls.Add(panelStatusBadge);

            lblDetailsCustomer = new Label();
            lblDetailsCustomer.Location = new Point(16, 82);
            lblDetailsCustomer.Size = new Size(438, 18);
            lblDetailsCustomer.Font = new Font("Segoe UI", 8.75f);
            lblDetailsCustomer.ForeColor = Color.FromArgb(51, 65, 85);

            lblDetailsExpiry = new Label();
            lblDetailsExpiry.Location = new Point(16, 102);
            lblDetailsExpiry.Size = new Size(438, 18);
            lblDetailsExpiry.Font = new Font("Segoe UI", 8.75f);
            lblDetailsExpiry.ForeColor = Color.FromArgb(51, 65, 85);

            btnImportLicense = new Button();
            btnImportLicense.Text = "🔑 Kích hoạt bằng file bản quyền (.avlic)";
            btnImportLicense.Font = new Font("Segoe UI", 9.25f, FontStyle.Bold);
            btnImportLicense.Location = new Point(16, 128);
            btnImportLicense.Size = new Size(438, 38);
            btnImportLicense.FlatStyle = FlatStyle.Flat;
            btnImportLicense.BackColor = Color.FromArgb(16, 185, 129); // Emerald 500
            btnImportLicense.ForeColor = Color.White;
            btnImportLicense.Cursor = Cursors.Hand;
            btnImportLicense.FlatAppearance.BorderSize = 0;
            btnImportLicense.Click += BtnImportLicense_Click;

            grpStatus.Controls.Add(lblDetailsCustomer);
            grpStatus.Controls.Add(lblDetailsExpiry);
            grpStatus.Controls.Add(btnImportLicense);
            bodyPanel.Controls.Add(grpStatus);

            // ================= FOOTER BUTTONS =================
            Panel footerPanel = new Panel();
            footerPanel.Dock = DockStyle.Bottom;
            footerPanel.Height = 48;
            footerPanel.BackColor = Color.FromArgb(241, 245, 249);

            btnRefresh = new Button();
            btnRefresh.Text = "🔄 Làm mới";
            btnRefresh.Font = new Font("Segoe UI", 8.5f);
            btnRefresh.Location = new Point(18, 10);
            btnRefresh.Size = new Size(95, 28);
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.BackColor = Color.White;
            btnRefresh.ForeColor = Color.FromArgb(30, 41, 59);
            btnRefresh.Cursor = Cursors.Hand;
            btnRefresh.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnRefresh.Click += delegate { RefreshStatusDisplay(); };

            btnClose = new Button();
            btnClose.Text = "Đóng";
            btnClose.Font = new Font("Segoe UI", 8.5f);
            btnClose.Location = new Point(393, 10);
            btnClose.Size = new Size(95, 28);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.BackColor = Color.White;
            btnClose.ForeColor = Color.FromArgb(30, 41, 59);
            btnClose.Cursor = Cursors.Hand;
            btnClose.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnClose.Click += delegate { this.Close(); };

            footerPanel.Controls.Add(btnRefresh);
            footerPanel.Controls.Add(btnClose);
            this.Controls.Add(footerPanel);
        }

        private void BtnExportRequest_Click(object sender, EventArgs e)
        {
            string name = (txtCustomerName.Text != null) ? txtCustomerName.Text.Trim() : string.Empty;
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Vui lòng nhập Họ và tên trước khi xuất file yêu cầu.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCustomerName.Focus();
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "File yêu cầu bản quyền AVRoad (*.avreq)|*.avreq|All files (*.*)|*.*";
                sfd.Title = "Lưu file yêu cầu cấp bản quyền AVRoad 2026 - TDT Solution 9.1";
                sfd.FileName = string.Format("AVRoad_Request_{0}.avreq", HardwareIdentifier.GetMachineName());

                if (sfd.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        LicenseManager.ExportRequest(
                            sfd.FileName,
                            txtCustomerName.Text,
                            Environment.UserDomainName,
                            string.Empty,
                            string.Empty);

                        // Tự động sao chép chuỗi JSON vào Clipboard để gửi Zalo nhanh
                        try
                        {
                            if (File.Exists(sfd.FileName))
                            {
                                string jsonContent = File.ReadAllText(sfd.FileName);
                                Clipboard.SetText(jsonContent);
                            }
                        }
                        catch { }

                        MessageBox.Show(
                            string.Format("Đã xuất file yêu cầu thành công:\n{0}\n\n✓ Đồng thời mã yêu cầu đã được sao chép vào bộ nhớ tạm (Clipboard).\nBạn có thể gửi file hoặc dán trực tiếp (Ctrl+V) vào Zalo cho Quản trị viên để nhận file bản quyền (.avlic).", sfd.FileName),
                            "Xuất file thành công",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi ghi file yêu cầu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnImportLicense_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "File bản quyền AVRoad (*.avlic)|*.avlic|All files (*.*)|*.*";
                ofd.Title = "Chọn file bản quyền để kích hoạt AVRoad 2026";

                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    string msg;
                    bool ok = LicenseManager.ImportLicense(ofd.FileName, out msg);
                    if (ok)
                    {
                        this.DialogResult = DialogResult.OK;
                        MessageBox.Show(msg, "Kích hoạt thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show(msg, "Kích hoạt chưa thành công", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    RefreshStatusDisplay();
                }
            }
        }

        public void RefreshStatusDisplay()
        {
            LicenseValidationResult result = LicenseManager.ValidateLicense(true);

            if (result.IsValid && result.Data != null)
            {
                panelStatusBadge.BackColor = Color.FromArgb(220, 252, 231);
                lblStatusTitle.Text = "● BẢN QUYỀN ĐÃ KÍCH HOẠT HỢP LỆ";
                lblStatusTitle.ForeColor = Color.FromArgb(21, 128, 61);

                if (result.Data.IsPerpetual)
                {
                    lblStatusDesc.Text = "Gói bản quyền Vĩnh viễn (Perpetual License) - Hoạt động offline 100%.";
                }
                else
                {
                    lblStatusDesc.Text = string.Format("Hạn dùng đến {0:dd/MM/yyyy} (còn {1} ngày).", result.Data.ExpiryDate, result.DaysRemaining);
                }

                lblDetailsCustomer.Text = string.Format("Khách hàng: {0}", result.Data.CustomerName);
                lblDetailsExpiry.Text = string.Format("Thời hạn: {0} | Sản phẩm: {1}",
                    result.Data.IsPerpetual ? "Vĩnh viễn" : string.Format("{0:dd/MM/yyyy}", result.Data.ExpiryDate),
                    string.IsNullOrEmpty(result.Data.Product) ? "AVRoad 2026 (TDT 9.1)" : result.Data.Product);

                if (string.IsNullOrEmpty(txtCustomerName.Text) || txtCustomerName.Text == Environment.UserName)
                {
                    if (!string.IsNullOrEmpty(result.Data.CustomerName) && result.Data.CustomerName != "Người dùng AVRoad")
                    {
                        txtCustomerName.Text = result.Data.CustomerName;
                    }
                }

                if (btnClose != null)
                {
                    btnClose.Text = "Hoàn tất";
                    btnClose.BackColor = Color.FromArgb(16, 185, 129);
                    btnClose.ForeColor = Color.White;
                    btnClose.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                    btnClose.FlatAppearance.BorderSize = 0;
                }
            }
            else
            {
                panelStatusBadge.BackColor = Color.FromArgb(254, 242, 242);
                lblStatusTitle.ForeColor = Color.FromArgb(185, 28, 28);

                if (LicenseManager.IsRevoked)
                {
                    lblStatusTitle.Text = "● CẦN CẬP NHẬT PHIÊN BẢN MỚI";
                    lblStatusDesc.Text = "Phần mềm đã cũ, bạn vui lòng cập nhật phiên bản mới để tiếp tục sử dụng.";
                    lblDetailsCustomer.Text = "Thiết bị nằm trong danh sách cần nâng cấp.";
                    lblDetailsExpiry.Text = "Trạng thái: Cần cập nhật";
                }
                else
                {
                    lblStatusTitle.Text = "● CHƯA KÍCH HOẠT BẢN QUYỀN";
                    lblStatusDesc.Text = "Phần mềm chưa được kích hoạt bản quyền. Vui lòng đăng ký mã máy.";
                    lblDetailsCustomer.Text = "Nhập Họ tên, bấm [Xuất yêu cầu (.avreq)] gửi Zalo Admin nhận file key.";
                    lblDetailsExpiry.Text = "Sau khi nhận key, bấm nút [Kích hoạt bằng file (.avlic)] bên dưới.";
                }

                if (btnClose != null)
                {
                    btnClose.Text = "Đóng";
                    btnClose.BackColor = Color.White;
                    btnClose.ForeColor = Color.FromArgb(30, 41, 59);
                    btnClose.Font = new Font("Segoe UI", 8.5f, FontStyle.Regular);
                    btnClose.FlatAppearance.BorderSize = 1;
                }
            }
        }
    }
}