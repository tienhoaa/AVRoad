# Hướng Dẫn Deploy Cloudflare Worker Xác Thực Bản Quyền AV Civil (Miễn Phí 100%)

Hệ thống cho phép plugin Civil 3D kiểm tra trực tuyến xem giấy phép hoặc máy tính (HWID) có bị Admin thu hồi (Revoke) từ xa hay không.
Cloudflare Workers cung cấp **100.000 requests/ngày hoàn toàn miễn phí**.

---

## 3 Bước Triển Khai Nhanh (Chưa đầy 3 phút)

### Bước 1: Cài đặt Wrangler và Đăng nhập
Mở Terminal / PowerShell và chạy:
```bash
npm install -g wrangler
wrangler login
```
*(Trình duyệt sẽ mở ra để bạn đăng nhập tài khoản Cloudflare miễn phí).*

---

### Bước 2: Tạo KV Namespace lưu trữ dữ liệu
Tại thư mục `c:\Users\PC\Documents\AV_Civil\AdminPortal\cloudflare_worker\`, chạy:
```bash
wrangler kv:namespace create AV_LICENSES
```
Wrangler sẽ trả về một ID, ví dụ:
```toml
[[kv_namespaces]]
binding = "AV_LICENSES"
id = "a1b2c3d4e5f6g7h8i9j0"
```
Mở file `wrangler.toml` và dán `id` này vào mục `id = "..."`.

---

### Bước 3: Deploy lên Cloudflare
Chạy lệnh:
```bash
wrangler deploy
```
Bạn sẽ nhận được URL worker công khai, ví dụ:
`https://av-license-service.<your-subdomain>.workers.dev`

---

## Các Endpoint Hoạt Động

1. **Kiểm tra trạng thái bản quyền (C3D Plugin / Web Portal gọi)**:
   ```http
   GET https://av-license-service.<subdomain>.workers.dev/api/verify?hwid=AV-XXXX-XXXX-XXXX-XXXX
   ```
   **Trả về**:
   - Nếu còn hiệu lực: `{ "status": "active", "message": "Giấy phép HỢP LỆ..." }`
   - Nếu bị thu hồi: `{ "status": "revoked", "message": "Giấy phép đã bị THU HỒI...", "reason": "..." }`

2. **Thu hồi bản quyền từ xa (Admin Portal gọi)**:
   ```http
   POST /api/revoke
   Authorization: Bearer AVAdmin@2026_Secret
   Content-Type: application/json

   { "hwid": "AV-XXXX-XXXX-XXXX-XXXX", "reason": "Hết hạn thanh toán hợp đồng" }
   ```

3. **Kích hoạt / Khôi phục lại**:
   ```http
   POST /api/activate
   Authorization: Bearer AVAdmin@2026_Secret
   Content-Type: application/json

   { "hwid": "AV-XXXX-XXXX-XXXX-XXXX" }
   ```

---
> [!TIP]
> Bạn có thể cấu hình URL Worker này ngay trong trang Web Quản trị **AV License Manager** tại mục Cài đặt kết nối Online để tự động đồng bộ mỗi khi nhấn nút **Thu hồi** hoặc **Kích hoạt**!
