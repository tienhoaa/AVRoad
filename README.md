# AVRoad 2026 Solution - Powered by AVCAD Engine 🚀

> **HỆ THỐNG THIẾT KẾ TUYẾN & HẠ TẦNG GIAO THÔNG CHUYÊN NGHIỆP**  
> **Hệ sinh thái:** AV BIM - Common Data Environment (CDE)  
> **Slogan:** *CHUẨN HÓA DỮ LIỆU CDE • TỐI ƯU THIẾT KẾ HẠ TẦNG BIM*

---

## 🌟 Giới Thiệu
**AVRoad 2026 Solution** là giải pháp phần mềm thiết kế hạ tầng giao thông độc lập hoàn toàn, tích hợp toàn diện quy trình khảo sát, thiết kế tuyến đường, san nền, thủy lợi kênh mương, địa chất công trình và mô hình hóa hạ tầng BIM 3D.

---

## ⚡ Các Tính Năng Nổi Bật
1. **Khởi động siêu tốc (< 1-2 giây)**: Loại bỏ độ trễ khởi động, vô hiệu hóa hoàn toàn Communication Center và các lỗi Resource DLL.
2. **Hệ thống 7 Menu Chuyên Ngành Đồng Bộ**:
   - `AV_KS`: Khảo sát địa hình, xử lý điểm đo, mô hình địa hình tự nhiên.
   - `AV_Road`: Thiết kế tuyến đường, trắc dọc, trắc ngang, tính toán khối lượng đào đắp.
   - `AV_SanNen`: Tính toán đào đắp san nền theo ô lưới, thiết kế taluy.
   - `AV_Kenh`: Thiết kế kênh mương thủy lợi, mặt cắt trắc dọc trắc ngang kênh.
   - `AV_DiaChat`: Khai báo lỗ khoan, địa chất công trình, vẽ mặt cắt trắc dọc địa chất.
   - `AV_3D`: Mô hình hóa tuyến đường 3D, cắt mặt Face 3D, xuất phối cảnh view 3D.
   - `AV_VachSon`: Thiết kế tổ chức giao thông, vạch sơn đường bộ, biển báo 3D.
3. **Thanh Công Cụ AVRoad 2026 Độc Lập**: Gồm 22 biểu tượng thiết kế mới (16x16 & 24x24) hiện đại, trực quan, khác biệt hoàn toàn với CAD truyền thống.
4. **Cơ Chế Cập Nhật Bản Vá Trực Tuyến Qua GitHub (Hotfix)**:
   - **GitHub chỉ đóng vai trò phân phối bản vá nhẹ** (< 1-5 MB), tuân thủ 100% giới hạn gói miễn phí của GitHub.
   - Sau khi khởi động CAD, hệ thống tự động kiểm tra bản vá ở chế độ ngầm (không ảnh hưởng tốc độ vẽ).
   - Nếu có bản vá mới, một thông báo Toast hiện đại xuất hiện ở **góc dưới bên phải màn hình**.
   - Bấm **"CẬP NHẬT NGAY"**: CAD tự động đóng an toàn, bộ cập nhật `AVUpdater.exe` bên ngoài sẽ chạy, tải bản vá và khởi động lại AVRoad trong tích tắc.
5. **Trung Tâm Cấu Hình Hệ Thống (`AVSetting.exe`)**:
   - Tự động mở ở lần chạy đầu tiên để thiết lập Font CAD, Nét in CTB, Máy in PC3.
   - Tối ưu hóa hiệu năng và dọn dẹp bộ đệm cache.

---

## 📦 Phát Hành & Phân Phối (Distribution Model)

| Phiên bản | Định dạng | Dung lượng | Kênh phân phối |
| :--- | :--- | :--- | :--- |
| **Bản Cài Đặt (Installer)** | `Setup_AVRoad_2026.exe` | ~107 MB | Google Drive (Trình cài đặt tự động tạo Desktop & Start Menu Shortcut) |
| **Bản Portable** | `AVRoad_2026_Portable.zip` | ~107 MB | Google Drive (Giải nén là chạy ngay, không cần cài đặt) |
| **Bản Vá Lỗi (Hotfixes)** | `patch_vX.X.zip` (VD: `patch_1.0.zip`) | < 1 - 5 MB | **GitHub Releases** (Tự động thông báo và tải qua `AVUpdater.exe`) |

---

## 🏷️ Quy Tắc Đặt Phiên Bản (Versioning Scheme)
- **Bắt đầu:** `v1.0` (Phiên bản phát hành chính thức đầu tiên)
- **Cập nhật nhẹ (Sửa lỗi, icon, lisp, giao diện):** `v1.1`, `v1.2`, `v1.3`...
- **Cập nhật lớn (Thêm menu, nâng cấp tính năng lớn):** `v2.0`, `v3.0`...

---

## 💻 Hướng Dẫn Sử Dụng
- **Khởi động phần mềm**: Chạy file `AVRoad.exe`.
- **Mở bảng cấu hình**: Chạy file `AVSetting.exe` (hoặc giữ phím `SHIFT` khi click `AVRoad.exe`).
- **Cập nhật thủ công**: Chạy file `AVUpdater.exe` trong thư mục phần mềm.
- **Tạo bản vá mới để đẩy lên GitHub**: Chạy file `create_patch.bat`.
