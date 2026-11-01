# TÀI LIỆU KIẾN TRÚC & HƯỚNG DẪN HIỆN ĐẠI HÓA GIAO DIỆN AUTOCAD 2007 (CAD7) & AVROAD 2026
> **Dành cho AI Developer / Codex / Antigravity Agent**  
> **Phiên bản tài liệu:** v1.2 (Cập nhật toàn diện sau Hotfix v1.2) | **Ngày cập nhật:** 01/11/2026  
> **Dự án:** AVRoad 2026 Solution (Hệ sinh thái AV BIM CDE - Nền tảng AVCAD 2007 Engine)  
> **Repository:** `https://github.com/tienhoaa/AVRoad.git` (Branch: `main`)

---

## 1. TỔNG QUAN HỆ THỐNG & TRẠNG THÁI HIỆN TẠI (SYSTEM STATUS)

Dự án này là giải pháp đóng gói di động độc lập (**Portable 100% Isolated**), hiện đại hóa nhận diện thương hiệu và giải phóng hoàn toàn sự phụ thuộc vào bộ cài gốc của bộ phần mềm thiết kế hạ tầng giao thông **TDT Solution 9.1** chạy trên nhân đồ họa **AutoCAD 2007 (ACAD R17.0 x86 32-bit)** với tên thương hiệu mới: **AVRoad 2026 Solution** (Hệ sinh thái AV BIM).

### 🎯 Trạng thái hoàn thành đến thời điểm hiện tại:
1. **Chạy Portable 100% trên mọi máy tính Windows (10/11 64-bit):** Hoạt động ngay lập tức không cần cài đặt trước AutoCAD 2007 hay TDT Solution từ đĩa gốc. Không xung đột với các bản AutoCAD 2020-2026 khác trên máy.
2. **Sửa dứt điểm 100% lỗi crash kinh điển khi đóng file/tắt bản vẽ (`...F49Dh`):** Đã phân tích minidump và dịch ngược mã máy, giải quyết triệt để lỗi Access Violation trong `acrender.arx`.
3. **Sửa dứt điểm lỗi khởi động trên máy mới (`Class 'TdtDbMeasurePointObj' parent named 'TdtDbPointObj' not found`):** Tự động cấu hình ObjectDBX Registry và nạp DBX ưu tiên.
4. **Loại bỏ 100% dấu hiệu thương hiệu cũ (TDT):** Đổi tên toàn bộ 7 Menu chuyên ngành sang tiền tố `AV_`, xóa sạch các mục `About TDT` và `Help` gây rối giao diện.
5. **Khởi động êm ái, sạch bóng thông báo rác:** Loại bỏ cảnh báo `COM server failed`, `Type library not registered`, và `Register service successful`.
6. **Bộ cài đặt độc lập 1 click (`Setup_AVRoad_2026.exe` 109.39 MB):** Nhúng toàn bộ `payload.zip` bên trong file EXE, tự động bung nén, tạo shortcut Desktop và nạp Registry hoàn chỉnh.
7. **Cơ chế cập nhật tự động (Online Hotfix v1.2):** Hộp thoại popup cập nhật nổi bật giữa màn hình (`UpdateToastForm`), tự động tải và bung đè `patch_1.2.zip` qua `AVUpdater.exe`.

---

## 2. CẤU TRÚC THƯ MỤC CỐT LÕI (CORE DIRECTORY STRUCTURE)

Thư mục cài đặt chuẩn: `C:\AVRoad_2026\` (hoặc thư mục nguồn Portable: `C:\TDT_RESEARCH\AVROAD_PORTABLE\`):

```
AVRoad_2026/
├── AVRoad.exe                 # Trình khởi chạy chính (C# WinForms Launcher + Branding + Auto Registry + Update Toast)
├── AVSetting.exe              # Trung tâm cài đặt đồ họa, font, đường dẫn, theme
├── AVUpdater.exe              # Trình cập nhật độc lập tải & bung patch_x.x.zip từ GitHub
├── Uninstall.exe              # Trình gỡ cài đặt sạch sẽ khỏi máy tính
├── Setup_AVRoad_2026.exe      # Bộ cài đặt độc lập 109.39 MB (có sẵn trên Desktop)
├── AVRoad.ini                 # Tệp cấu hình cục bộ (Plotter, Plot Style, Flags...)
├── AVRoad_Profile.arg         # Tệp Profile AutoCAD mẫu
├── version.json               # Định danh phiên bản hiện tại (v1.2)
│
├── acad.exe                   # Nhân đồ họa AutoCAD 2007 gốc (32-bit, đã patch branding)
├── identity.ini               # [BẮT BUỘC] Tệp định danh gốc của AutoCAD - không được xóa
├── Acad.rx                    # Danh sách nạp ARX/DBX tự động (TDTObjects17.dbx đứng dòng 1)
├── ACAD.lsp                   # Lisp nạp 7 Menu AV_ khi mở CAD (đã tắt cảnh báo COM)
│
├── acrender.arx               # [ĐÃ PATCH] Module Render AutoCAD - Đã fix lỗi crash khi đóng file
├── TDTObjects17.dbx           # [ĐÃ PATCH] ObjectDBX định nghĩa thực thể - Đã tắt printf rác
├── TDTObjectsWrapper.arx      # [ĐÃ PATCH] COM Wrapper - Đã tắt cảnh báo COM Server
├── AlignmentDb17.dbx          # Cơ sở dữ liệu tim tuyến, trắc dọc, trắc ngang
├── AlignmentUI17.arx          # Giao diện thiết kế tuyến
├── RoadMarkingUI.arx          # Giao diện vạch sơn, biển báo
├── NhapSoLieuTuyen17.arx      # Nhập số liệu khảo sát, trắc đạc
├── TDTHorizontalProfileDesign.arx # Thiết kế bình đồ tuyến
├── ProjectDH17.arx            # Địa hình, mô hình số độ cao TIN
├── SNVN17.arx                 # Tính toán san nền
├── ThietKeKenh17.arx          # Thiết kế kênh mương thủy lợi
│
├── *.rar (8 tệp key Safengine):# [TỐI QUAN TRỌNG - TUYỆT ĐỐI KHÔNG ĐƯỢC XÓA]
│   ├── Rlthap.rar, Rlthua26.rar, Rlt25.rar, rkgui.rar
│   ├── rsgui.rar, tdtHFD.rar, Nslt.rar, PDH17.rar
│   # Đây là tệp License Key mã hóa khóa cứng của Safengine Protector
│
├── Support/                   # Thư mục menu & tài nguyên hỗ trợ:
│   ├── KSVN.cui               # Menu &AV_Khảo sát (đã xóa About)
│   ├── VNROAD.cui             # Menu &AV_Thiết kế đường (đã xóa About, Help)
│   ├── SANNEN.cui             # Menu &AV_San nền (đã xóa About)
│   ├── KVN.cui                # Menu &AV_Kênh mương (đã xóa About)
│   ├── DCCT.cui               # Menu &AV_Địa chất
│   ├── TDT3D.cui              # Menu &AV_3D
│   ├── RM.cui                 # Menu &AV_Vạch sơn-Biển báo
│   ├── acad.cui               # Giao diện gốc AutoCAD
│   ├── acad.fmp, sample.cus   # Font map và từ điển
│   └── icons/                 # 22 icon thanh công cụ chuẩn BIM
├── Fonts/                     # Font AutoCAD chuẩn TCVN3, SHX Tiếng Việt
├── Express/                   # Bộ công cụ AutoCAD Express Tools
├── Drv/                       # Trình điều khiển đồ họa DirectX/OpenGL
├── Plotters/ & Plot Styles/   # Cấu hình in ấn và nét in monochrome/color
└── Template/                  # File mẫu bản vẽ thiết kế .DWT
```

---

## 3. CÁC ĐỘT PHÁ KỸ THUẬT & LỖI ĐÃ GIẢI QUYẾT (DEBUG DISCOVERIES & SOLUTIONS)

Phần này ghi lại chi tiết các giải pháp kỹ thuật đã áp dụng để AI Codex hiểu sâu kiến trúc nhị phân của hệ thống:

### 3.1. Sửa dứt điểm lỗi crash khi đóng bản vẽ (`FATAL ERROR: Access Violation at ...F49Dh`)
* **Hiện tượng:** Người dùng mở bản vẽ bất kỳ, khi bấm nút đóng (X) hoặc gõ `CLOSE`/`QUIT` thì AutoCAD lập tức crash với thông báo: `FATAL ERROR: Unhandled Access Violation Reading 0x0000 Exception at cd7f49dh` (hoặc `d01f49dh`, `caaf49dh`, `119bf49dh`).
* **Phân tích nhị phân (Root Cause):**
  * Các địa chỉ crash luôn có 16-bit cuối là `F49Dh`. Vì các module nạp trong Windows 32-bit luôn căn lề 64KB (`0x10000`), địa chỉ tương đối (RVA) của lệnh gây lỗi chính xác là **`0x0000F49D`**.
  * Dùng Python phân tích minidump và scan disassembly toàn bộ các module: Phát hiện lệnh nằm tại module **[`acrender.arx`](file:///C:/AVRoad_2026/acrender.arx)** (bộ engine render/3D của AutoCAD).
  * Mã máy tại RVA `0x0000F49D` trong `acrender.arx`:
    ```assembly
    0x1000F497: jne 0x1000F55B
    0x1000F49D: cmp dword ptr [esi], 3    <--- LỖI TẠI ĐÂY KHI esi == NULL (0x00000000)
    0x1000F4A0: jne 0x1000F55B
    ```
    Khi đóng tài liệu 2D không có vật liệu 3D, con trỏ `esi` truyền vào hàm là `NULL`. Hàm không kiểm tra `esi == 0` mà trực tiếp dereference `[esi]`, dẫn đến `Access Violation Reading 0x00000000`.
* **Giải pháp Binary Patch (Trampoline Code Cave):**
  * Tại RVA `0x0000F49D`, ghi đè 9 byte bằng lệnh `jmp 0x1010CF10` (nhảy tới vùng padding rỗng cuối section `.text`) và 4 byte `nop`.
  * Tại Code Cave `0x1010CF10`:
    ```assembly
    0x1010CF10: test esi, esi          ; Kiểm tra con trỏ esi có NULL không
    0x1010CF12: je 0x1010CF1E          ; Nếu esi == NULL -> nhảy thẳng tới lệnh thoát
    0x1010CF14: cmp dword ptr [esi], 3 ; Nếu esi hợp lệ -> so sánh như mã gốc
    0x1010CF17: jne 0x1010CF1E         ; Nếu != 3 -> nhảy tới lệnh thoát
    0x1010CF19: jmp 0x1000F4A6         ; Tiếp tục luồng xử lý bình thường
    0x1010CF1E: jmp 0x1000F55B         ; Thoát hàm an toàn (clean epilogue)
    ```
  * **Kết quả:** Triệt tiêu hoàn toàn 100% lỗi văng khi đóng file.

### 3.2. Sửa lỗi `Class 'TdtDbMeasurePointObj' parent named 'TdtDbPointObj' not found`
* **Hiện tượng:** Khi khởi động trên máy chưa cài TDT gốc, AutoCAD hiện hộp thoại `AVRoad 2026 Error Aborting: Class 'TdtDbMeasurePointObj' parent named 'TdtDbPointObj' not found`.
* **Nguyên nhân:** Khóa Registry `HKCU\Software\Autodesk\ObjectDBX\R17.0\Applications\TDTObjects` có giá trị `LOADER` bị trỏ vào `acad.exe` thay vì `TDTObjects17.dbx`, khiến ObjectDBX nạp thất bại và không thể đăng ký lớp gốc `TdtDbPointObj`.
* **Giải pháp:**
  * Cấu hình Registry `LOADER` trỏ đúng vào `{appDir}\TDTObjects17.dbx` với `LOADCTRLS = 9`.
  * Trong [`AVRoad_Launcher.cs`](file:///C:/TDT_RESEARCH/AVRoad_Launcher.cs), bổ sung mã tự động kiểm tra và ghi đè 2 khóa `ObjectDBX\R17.0\Applications\TDTObjects` và `AutoCAD\R17.0\ACAD-5001:409\Applications\TDTObjects` mỗi lần chạy.
  * Trong [`Acad.rx`](file:///C:/AVRoad_2026/Acad.rx), đưa `TDTObjects17.dbx` lên ngay dòng đầu tiên để đảm bảo luôn được nạp trước `TDTObjectsUI17.arx` và `AlignmentUI17.arx`.

### 3.3. Tắt sạch các cảnh báo rác khi khởi động
1. **Loại bỏ cảnh báo COM Type Library:** Xóa dòng `(vl-load-com)` trong [`ACAD.lsp`](file:///C:/AVRoad_2026/ACAD.lsp) -> Triệt tiêu 2 dòng warning `; warning:AutoCAD type library load failed: "Library not registered"`.
2. **Loại bỏ `Failed to register COM server.`:** Trong [`TDTObjectsWrapper.arx`](file:///C:/AVRoad_2026/TDTObjectsWrapper.arx) tại offset `0x4E17`, đổi lệnh nhảy có điều kiện `7D 0B` (`jge`) thành lệnh nhảy không điều kiện `EB 0B` (`jmp short`). Hàm `DllRegisterServer()` vẫn được thực thi bình thường nhưng thông báo thất bại được bỏ qua.
3. **Loại bỏ `Register service TDTOBJECTDB_DBXSERVICE successful`:** Trong [`TDTObjects17.dbx`](file:///C:/AVRoad_2026/TDTObjects17.dbx) tại offset `0x4A1B`, đổi lệnh `74 12` (`jz`) thành `EB 12` (`jmp short`) để nhảy qua lệnh gọi `acutPrintf`.

### 3.4. Chuẩn hóa 7 Menu chuyên ngành với tiền tố `AV_`
Đã sửa đổi toàn bộ các file `.cui` trong `Support/`:
* `KSVN.cui` -> PopMenu: `&AV_Khảo sát` (đã xóa mục `About TDT`).
* `VNROAD.cui` -> PopMenu: `&AV_Thiết kế đường` (đã xóa mục `About TDT`, `Help`).
* `SANNEN.cui` -> PopMenu: `&AV_San nền` (đã xóa mục `About TDT`).
* `KVN.cui` -> PopMenu: `&AV_Kênh mương` (đã xóa mục `About TDT`).
* `DCCT.cui` -> PopMenu: `&AV_Địa chất`.
* `TDT3D.cui` -> PopMenu: `&AV_3D`.
* `RM.cui` -> PopMenu: `&AV_Vạch sơn-Biển báo`.
* Đã xóa các file cache `.mnr` để AutoCAD tự biên dịch lại giao diện sạch 100%.

---

## 4. BẢN ĐỒ LỆNH NGHIỆP VỤ (COMMAND DICTIONARY)

Dành cho Codex khi thiết kế nút bấm UI nhanh cho các phân hệ:

| Phân hệ (Module) | Tên Menu hiển thị | Lệnh AutoCAD chính | Chức năng nghiệp vụ |
| :--- | :--- | :--- | :--- |
| **Khảo sát địa hình** | `AV_Khảo sát` | `DIEM`, `TIN`, `CD`, `DH`, `LTM`, `DXT` | Rải điểm mia trắc đạc, tạo mô hình TIN, nội suy cao độ, vẽ đường đồng mức |
| **Thiết kế đường bộ** | `AV_Thiết kế đường` | `T`, `TK`, `CS`, `TT`, `TN`, `TD`, `XTC`, `BTH` | Khởi tạo tuyến, vạch tim, cong nằm, vẽ trắc dọc, trắc ngang, thiết kế áo đường, áp khuôn mẫu |
| **San nền quy hoạch** | `AV_San nền` | `SN`, `KTTN`, `TKTN`, `TL`, `TTK` | Tạo lưới ô vuông san nền, gán cao độ tự nhiên/thiết kế, tính khối lượng đào đắp theo ô |
| **Kênh mương thủy lợi**| `AV_Kênh mương` | `KNH`, `TKK`, `MCK`, `KLK` | Định tuyến kênh, mặt cắt thiết kế bờ kênh, tính khối lượng đào đắp kênh mương |
| **Địa chất công trình**| `AV_Địa chất` | `DC`, `KDC`, `HDC`, `LDC` | Nhập số liệu hố khoan địa chất, vẽ trụ địa chất trên trắc dọc/ngang, chèn lớp đất |
| **Vạch sơn & Biển báo**| `AV_Vạch sơn-Biển báo`| `RM`, `RMB`, `BS`, `VS` | Bố trí biển báo hiệu đường bộ QCVN 41, kẻ vạch sơn tim đường, vạch đi bộ, gờ giảm tốc |
| **Phối cảnh 3D** | `AV_3D` | `T3D`, `X3D`, `VIEW3D` | Xuất mô hình tim tuyến và bề mặt đường sang khối 3D trực quan |

---

## 5. HƯỚNG DẪN CODEX TRIỂN KHAI GIAO DIỆN HIỆN ĐẠI (NEXT UI TASKS FOR CODEX)

AutoCAD 2007 sử dụng cửa sổ chính Win32 (`Afx:00400000:8:...`). Nền tảng lõi (Core Engine & Backend) hiện đã ổn định tuyệt đối. Nhiệm vụ của Codex là tập trung vào **3 phương án hiện đại hóa giao diện**:

```
+-----------------------------------------------------------------------------------+
| AutoCAD 2007 Main Window (HWND: acadMainWindowHandle)                             |
|  +-----------------------------------------------------------------------------+  |
|  | [Nhiệm vụ 2] Modern Top Ribbon / Toolbar (WPF C# Injected)                   |  |
|  +-----------------------------------------------------------------------------+  |
|  | Modern Quick Action Floating HUD (Đang tuyến, Xuất trắc dọc, Áp khuôn...)   |  |
|  +-------------------------------------+---------------------------------------+  |
|  | Drawing Canvas (DirectX/OpenGL View)| [Nhiệm vụ 1 - Khuyến nghị ưu tiên]    |  |
|  |                                     | Modern Sidecar Panel Docked           |  |
|  |                                     | (C# WPF hoặc WebView2 HTML5 App)      |  |
|  |                                     | • Quản lý tim tuyến theo dự án BIM    |  |
|  |                                     | • Trực quan hóa mặt cắt trước khi áp  |  |
|  |                                     | • Bảng tra nhanh tiêu chuẩn TCVN      |  |
|  +-------------------------------------+---------------------------------------+  |
|  | Modern Dark Command Line / Status Bar                                       |  |
+-----------------------------------------------------------------------------------+
```

### Nhiệm vụ 1: Triển khai Modern Sidecar Panel (Khuyến nghị ưu tiên)
* **Kiến trúc:** Viết một ứng dụng C# WPF hoặc WebView2 mang tên `AVRoad.UI.Sidecar.exe`.
* **Kỹ thuật Docking vào CAD:**
  * Lấy `HWND` của AutoCAD bằng `FindWindow("Afx:00400000:8:...", null)` hoặc qua PID từ `acadProc`.
  * Dùng `SetWinEventHook` bắt sự kiện `EVENT_OBJECT_LOCATIONCHANGE` của cửa sổ AutoCAD để đồng bộ vị trí, di chuyển bám sát mép phải màn hình AutoCAD.
* **Giao thức gửi lệnh vào CAD (Command Dispatcher):**
  * Dùng Windows Message: `SendMessage(acadDocHwnd, WM_COPYDATA, ...)` hoặc COM API:
    ```csharp
    Type acadType = Type.GetTypeFromProgID("AutoCAD.Application.17");
    dynamic acadApp = Marshal.GetActiveObject("AutoCAD.Application.17");
    acadApp.ActiveDocument.SendCommand("DLBD\n");
    ```
* **Ưu điểm vượt trội:** Chạy độc lập, không can thiệp sâu vào bộ nhớ AutoCAD, không sợ lỗi ARX, giao diện HTML5/WPF mượt mà, hỗ trợ Dark Mode chuẩn BIM `#1e1e1e` và màu thương hiệu Mint `#2dd4bf`.

### Nhiệm vụ 2: Top Modern Ribbon / Floating Toolbar
* Xây dựng một thanh Ribbon ngang mỏng phía trên vùng vẽ, thay thế các thanh Toolbar icon 16x16 cổ điển bằng các icon vector SVG 32x32 sắc nét.
* Phân chia 6 tab nghiệp vụ: `Trang Chủ`, `Khảo Sát`, `Thiết Kế Tuyến`, `San Nền`, `ATGT Vạch Sơn`, `Cài Đặt CDE`.

### Nhiệm vụ 3: Tối ưu hiển thị High-DPI (2K/4K Scaling)
* Thêm manifest `dpiAware` trong các file thực thi C# để các form giao diện không bị mờ nhòe trên màn hình độ phân giải cao.

---

## 6. DANH MỤC TỆP TIN ĐÃ BIÊN DỊCH & DEPLOY (BUILD OUTPUT ARTIFACTS)

| Tệp tin | Vị trí | Dung lượng | Mô tả |
| :--- | :--- | :--- | :--- |
| **`Setup_AVRoad_2026.exe`** | `C:\Users\PC\Desktop\` & `C:\TDT_RESEARCH\` | **109.39 MB** | Trình cài đặt độc lập 1-click nhúng sẵn `payload.zip` |
| **`AVRoad.exe`** | `C:\AVRoad_2026\` & `AVROAD_PORTABLE\` | ~210 KB | Trình khởi chạy kèm Auto-Fix Registry và Update Toast |
| **`patch_1.2.zip`** | `C:\TDT_RESEARCH\` (Đã đẩy lên GitHub) | **1.57 MB** | Gói cập nhật hotfix nhị phân v1.2 (chứa `acrender.arx`, menu `AV_`...) |
| **`version.json`** | GitHub `main` & `C:\TDT_RESEARCH\` | ~700 B | Định danh phiên bản `v1.2` |
| **`AVUpdater.exe`** | `C:\AVRoad_2026\` & `AVROAD_PORTABLE\` | ~120 KB | Công cụ tải và giải nén bản vá tự động |
| **`AVSetting.exe`** | `C:\AVRoad_2026\` & `AVROAD_PORTABLE\` | ~140 KB | Trung tâm cài đặt giao diện, font chữ, đường dẫn |

---
*Tài liệu này được cập nhật đầy đủ và đồng bộ tại: `C:\TDT_RESEARCH\CODEX_CAD7_UI_GUIDE.md`.*
