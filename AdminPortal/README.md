# AV License Manager - Cổng Quản Trị & Cấp Bản Quyền AV Civil 3D

Trang Web Quản trị Bản quyền cao cấp dành cho Kỹ sư Admin hệ thống phần mềm **AV Civil 3D**.
Giao diện **Single Page Application (SPA)** hiện đại, mượt mà, tối ưu hóa 100% trên cả **Điện thoại** (Zalo In-App Browser, Safari iOS, Chrome Mobile) và **Máy tính** (Windows / macOS).

---

## 📁 Cấu Trúc Thư Mục

```text
c:\Users\PC\Documents\AV_Civil\AdminPortal\
│
├── index.html                   # Ứng dụng Quản trị Web (Single Page App - Mở trực tiếp)
├── revoked_hwid.json            # Danh sách thiết bị bị thu hồi (Blacklist) đồng bộ lên GitHub
│
├── keys/                        # Cặp khóa số RSA-2048 bit
│   ├── private_key.pem          # Khóa riêng tư (Admin bảo mật, dùng để ký số bản quyền)
│   └── public_key.pem           # Khóa công khai (Nhúng vào mã nguồn C# Civil 3D để xác thực)
│
├── sample/                      # Dữ liệu thử nghiệm
│   └── AV_Request_Sample.avreq  # File yêu cầu cấp bản quyền mẫu (.avreq)
│
└── cloudflare_worker/           # Kịch bản Cloudflare Worker dự phòng (nếu dùng serverless KV)
    ├── worker.js                # API verify / revoke Cloudflare Worker
    ├── wrangler.toml            # File cấu hình deploy Wrangler
    └── README.md                # Hướng dẫn 3 bước deploy Cloudflare Worker
```

---

## 🚀 Tính Năng Chính

### 1. Đăng Nhập Bảo Vệ Bằng Mật Khẩu Quản Trị
- Mật khẩu mặc định: `AVAdmin@2026`
- Hỗ trợ lưu phiên đăng nhập trên thiết bị (LocalStorage).
- Cho phép đổi mật khẩu quản trị bất kỳ lúc nào tại tab **Cài Đặt**.

### 2. Tiếp Nhận File Yêu Cầu `AV_Request.avreq`
- Kéo thả file `AV_Request.avreq` hoặc bấm để tải lên.
- Tự động trích xuất:
  - **Mã thiết bị HWID**: Định dạng chuẩn `AV-XXXX-XXXX-XXXX-XXXX`
  - **Tên khách hàng / Kỹ sư**
  - **Tên máy tính (Machine Name)**
  - **Đơn vị / Công ty**, Số điện thoại, Email.
- Hỗ trợ dán văn bản JSON trực tiếp từ tin nhắn Zalo/Telegram nếu khách hàng gửi text.
- Nút **"Dùng Dữ Liệu Mẫu"** để thử nghiệm tức thì với 1 click.

### 3. Thiết Lập Bản Quyền Linh Hoạt
- **Gói tính năng (Modules)**:
  - `AV_ALL`: Kích hoạt toàn bộ bộ công cụ Civil 3D.
  - `AV_ROAD`: Bình đồ Tuyến, Trắc dọc, Trắc ngang, Tính toán khối lượng.
  - `AV_PIPE`: Thoát nước mưa, Nước thải & Mạng lưới kỹ thuật (TNM).
  - `AV_MODEL`: Mô hình 3D Solid, IFC & BIM Civil 3D.
- **Thời hạn sử dụng nhanh**:
  - `1 Tháng` (+30 ngày)
  - `3 Tháng` (+90 ngày)
  - `6 Tháng` (+180 ngày)
  - `1 Năm` (+365 ngày)
  - `Vĩnh Viễn (Permanent)`
  - Hoặc chọn ngày tùy ý từ lịch.
- **Tách riêng hạn cập nhật (Update Expiry Date)**:
  - Cho phép cấp giấy phép **Vĩnh viễn** nhưng chỉ nhận cập nhật tính năng mới trong vòng **1 năm**, sau đó khách có thể gia hạn gói cập nhật.

### 4. Ký Số Bản Quyền RSA-2048 Bit (Chuẩn Web Crypto API)
- Sử dụng thuật toán chuẩn công nghiệp: **RSASSA-PKCS1-v1_5 với băm SHA-256**.
- Ký bảo mật không qua bất kỳ máy chủ bên thứ ba nào (100% Client-side).
- Tự động kiểm tra và xác thực chữ ký số ngay trên trình duyệt trước khi xuất file.
- Định dạng xuất chuẩn: `AV_License.avlic` (Payload JSON + Chữ ký Base64).

### 5. Xuất File & Chia Sẻ Nhanh Cho Khách Hàng
- Nút **"Tải File AV_License.avlic"**: Lưu file về máy tính hoặc điện thoại.
- Nút **"Gửi Zalo / Chia sẻ"**: Kích hoạt **Web Share API** của điện thoại (iOS / Android) để gửi trực tiếp qua Zalo, Telegram, Mail mà không cần qua nhiều bước.
- Nút **"Copy JSON Base64"**: Sao chép nội dung gửi nhanh qua tin nhắn.

### 6. Quản Lý Danh Sách Đã Cấp (Registry & History)
- Tự động lưu toàn bộ giấy phép đã cấp vào bộ nhớ cục bộ.
- Thống kê: Tổng giấy phép, Đang hoạt động, Sắp hết hạn (<30 ngày), Đã thu hồi.
- Tìm kiếm tức thì theo tên khách hàng, mã thiết bị HWID, công ty hoặc mã license.
- **Gia hạn nhanh (+1 Năm)**: Nạp lại thông tin vào form và tự động gia hạn thêm 1 năm chỉ với 1 click.
- **Sao lưu / Khôi phục**: Xuất file `licenses_backup.json` để đồng bộ giữa máy tính và điện thoại.

### 7. Đồng Bộ Danh Sách Đen (Revoked HWID) Lên GitHub Repo `tienhoaa/AV_Releases`
- Khi Admin bấm **Thu hồi (Revoke)** hoặc **Mở khóa (Activate)** thiết bị:
  - Tự động gọi **GitHub REST API** để cập nhật file `revoked_hwid.json` trên repo `tienhoaa/AV_Releases` (nhánh `main`).
  - Plugin Civil 3D kiểm tra trực tiếp qua đường dẫn Raw toàn cầu:
    `https://raw.githubusercontent.com/tienhoaa/AV_Releases/main/revoked_hwid.json`
  - Nếu máy tính nằm trong danh sách này, plugin sẽ tự động từ chối chạy.

---

## 🔑 Hướng Dẫn Cấu Hình Token GitHub

1. Truy cập [GitHub Personal Access Tokens](https://github.com/settings/tokens)
2. Bấm **Generate new token (classic)**
3. Đặt tên (ví dụ: `AV_License_Admin`) và tích chọn quyền:
   - `repo` (Full control of private/public repositories)
4. Copy mã Token (bắt đầu bằng `ghp_...`).
5. Mở trang Admin Portal -> Vào tab **GitHub & Cài Đặt**:
   - Dán Token vào ô **GitHub Personal Access Token (PAT)**.
   - Bấm **"Kiểm Tra Quyền Token"** và **"Lưu Cấu Hình"**.
6. Từ nay, mỗi khi bạn bấm nút Thu hồi bản quyền của khách hàng, danh sách đen sẽ tự động được đồng bộ lên GitHub trong 1 giây!

---

## 💻 Mã Nguồn C# Tích Hợp Xác Thực Trong Civil 3D

Dán đoạn mã sau vào DLL của plugin Civil 3D (`AVTools.dll`):

```csharp
using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public static class AVLicenseValidator
{
    // Public Key RSA-2048 trích xuất từ file public_key.pem
    private const string PUBLIC_KEY_PEM = @"-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAi9xQ1EpUK+sQCd+zVTAb
Ct2HdGFWcPgjPRCvREdZ7204aTLJ2lOKYi+y7/fQku2AdYSt/ueEiYzfMQn0GOw5
T3b8dGzKjCrdG2hHSK8tFlCYn/b9Jj28rU3Qjun4u5GXG1USTAT7J1CW/34Rtq56
OFC3zeCiCBfGfMJz9MUf7m+lZd6UQ1XEBMEcMwc3l5tV9rqT2EUwgAbgtDLtV9yJ
knaz6bHaTtqM4LUKV1TT2jXogXdw+N/2JGPtiG8kl2dHa1ABeG0ioBuIMUWvAe7i
bZcAYSxEycL2LtQvHf4FxkIyqGsmjegTaMiJaC+OzibvqQJj0G8jzQdio23K+sKa
oQIDAQAB
-----END PUBLIC KEY-----";

    public static async Task<bool> ValidateLicenseAsync(string licFilePath, string currentHwid)
    {
        if (!File.Exists(licFilePath))
            throw new FileNotFoundException("Chưa cài đặt file bản quyền AV_License.avlic!");

        // 1. Đọc và giải mã cấu trúc JSON
        string jsonText = File.ReadAllText(licFilePath);
        using var doc = JsonDocument.Parse(jsonText);
        
        string payloadString = doc.RootElement.GetProperty("payload").GetRawText();
        string signatureBase64 = doc.RootElement.GetProperty("signature").GetString();

        // 2. Xác thực chữ ký số RSA-2048 SHA-256 (Offline)
        using (var rsa = RSA.Create())
        {
            rsa.ImportFromPem(PUBLIC_KEY_PEM);
            byte[] dataBytes = Encoding.UTF8.GetBytes(payloadString);
            byte[] sigBytes = Convert.FromBase64String(signatureBase64);

            bool isSignatureValid = rsa.VerifyData(
                dataBytes,
                sigBytes,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1
            );

            if (!isSignatureValid)
                throw new CryptographicException("Chữ ký số bản quyền không hợp lệ! File có dấu hiệu bị can thiệp.");
        }

        // 3. Kiểm tra tính hợp lệ của Payload
        var payload = doc.RootElement.GetProperty("payload");
        string licHwid = payload.GetProperty("hwid").GetString();
        if (!string.Equals(licHwid, currentHwid, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException($"Giấy phép cấp cho máy khác ({licHwid}), không khớp với máy này ({currentHwid})!");

        bool isPermanent = payload.GetProperty("is_permanent").GetBoolean();
        if (!isPermanent)
        {
            DateTime expiryDate = DateTime.Parse(payload.GetProperty("expiry_date").GetString());
            if (DateTime.Today > expiryDate)
                throw new TimeoutException($"Giấy phép đã hết hạn sử dụng vào ngày {expiryDate:dd/MM/yyyy}!");
        }

        // 4. Kiểm tra danh sách đen trực tuyến từ GitHub (Asynchronous non-blocking)
        string checkUrl = "https://raw.githubusercontent.com/tienhoaa/AV_Releases/main/revoked_hwid.json";
        try
        {
            using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(3) })
            {
                string rawJson = await client.GetStringAsync(checkUrl + "?t=" + DateTime.UtcNow.Ticks);
                using var blacklistDoc = JsonDocument.Parse(rawJson);
                var revokedDict = blacklistDoc.RootElement.GetProperty("revoked_hwids");

                if (revokedDict.TryGetProperty(currentHwid, out var revokedInfo))
                {
                    string reason = revokedInfo.GetProperty("reason").GetString();
                    throw new UnauthorizedAccessException($"Máy tính đã bị thu hồi bản quyền trên máy chủ! Lý do: {reason}");
                }
            }
        }
        catch (HttpRequestException)
        {
            // Mất mạng Internet: Vẫn cho phép kỹ sư sử dụng bình thường (Offline mode)
        }

        return true;
    }
}
```
