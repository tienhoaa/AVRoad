# TÀI LIỆU KIẾN TRÚC & HƯỚNG DẪN HIỆN ĐẠI HÓA GIAO DIỆN AUTOCAD 2007 (CAD7) & AVROAD 2026
> **Dành cho AI Developer / Codex / Antigravity Agent**  
> **Phiên bản tài liệu:** v1.0 | **Ngày lập:** 01/11/2026  
> **Dự án:** AVRoad 2026 Solution (TDT Solution 9.1 + AVCAD 2007 Engine)

---

## 1. TỔNG QUAN HỆ THỐNG (SYSTEM OVERVIEW)

Dự án này là giải pháp đóng gói di động độc lập (Portable Isolated), nâng cấp nhận diện thương hiệu và hiện đại hóa hệ sinh thái phần mềm thiết kế hạ tầng giao thông **TDT Solution 9.1 (Update 2025/2026)** chạy trên nền nhân đồ họa **AutoCAD 2007 (ACAD R17.0 x86 32-bit)** với tên thương hiệu mới: **AVRoad 2026 Solution** (thuộc Hệ sinh thái AV BIM CDE).

### Mục tiêu kỹ thuật chính:
1. **Chạy Portable 100%:** Hoạt động trên mọi máy tính Windows (Windows 10, 11 64-bit) mà không yêu cầu cài đặt trước AutoCAD hay TDT Solution từ bộ đĩa gốc. Không làm xung đột với các bản AutoCAD khác (AutoCAD 2020-2026) đang cài trên máy.
2. **Làm lại & Hiện đại hóa Giao diện (CAD7 UI Modernization):** Thay thế giao diện cổ điển (MFC xám xịt từ năm 2006 của AutoCAD 2007) bằng ngôn ngữ thiết kế hiện đại (Modern Dark Theme, Fluent/BIM Design, Xanh ngọc Mint Neon `#2dd4bf` & Xanh lá Rừng `#07231a`, Ribbon/Floating HUD hoặc Dockable Palette).
3. **Cơ chế Bản vá Online (Online Hotfix Update):** Tự động phát hiện phiên bản mới từ GitHub Releases qua `version.json` và tải `patch_x.x.zip` giải nén tự động qua `AVUpdater.exe`.

---

## 2. KIẾN TRÚC THƯ MỤC & CÁC TỆP TIN CỐT LÕI (CORE FILE STRUCTURE)

Thư mục gốc của bộ phần mềm (ví dụ: `C:\AVRoad_2026\` hoặc `C:\TDT_RESEARCH\AVROAD_PORTABLE\`):

```
AVRoad_2026/
├── AVRoad.exe                 # Trình khởi chạy chính (C# WinForms Launcher + Branding + Update Hook)
├── AVSetting.exe              # Trung tâm cấu hình đồ họa, font, đường dẫn, theme
├── AVUpdater.exe              # Công cụ tải & giải nén bản vá đè (Hotfix) từ GitHub
├── Uninstall.exe              # Trình gỡ cài đặt sạch sẽ khỏi máy tính
├── AVRoad.ini                 # Tệp cấu hình cục bộ (Plotter, Plot Style, Flags...)
├── AVRoad_Profile.arg         # Tệp Profile mẫu của AutoCAD
├── version.json               # Tệp định danh phiên bản hiện tại (VD: {"version": "1.0", ...})
│
├── acad.exe                   # Nhân đồ họa AutoCAD 2007 chính hãng (32-bit)
├── identity.ini               # [BẮT BUỘC] Tệp cấu hình gốc AutoCAD - Thiếu file này AdMigrator sẽ báo lỗi
├── Acad.rx                    # Danh sách nạp tự động các module ObjectARX/DBX khi CAD khởi động
│
├── *.rar (8 tệp key Safengine):# [TỐI QUAN TRỌNG - KHÔNG ĐƯỢC XÓA]
│   ├── Rlthap.rar, Rlthua26.rar, Rlt25.rar, rkgui.rar
│   ├── rsgui.rar, tdtHFD.rar, Nslt.rar, PDH17.rar
│   # Đây KHÔNG PHẢI file nén rác! Đây là tệp Hardware License Key của Safengine Protector
│
├── *.arx, *.dbx:              # Các thư viện nghiệp vụ TDT Road:
│   ├── TDTObjects17.dbx       # ObjectDBX định nghĩa các thực thể đối tượng tuyến
│   ├── AlignmentDb17.dbx      # Cơ sở dữ liệu tim tuyến, trắc dọc, trắc ngang
│   ├── AlignmentUI17.arx      # Giao diện thiết kế tuyến (Cần TDTObjects nạp trước)
│   ├── RoadMarkingUI.arx      # Giao diện vạch sơn, biển báo
│   ├── NhapSoLieuTuyen17.arx   # Nhập số liệu khảo sát, trắc đạc
│   ├── TDTHorizontalProfileDesign.arx # Bình đồ tuyến
│   ├── ProjectDH17.arx        # Địa hình, mô hình số độ cao TIN
│   ├── SNVN17.arx             # Tính toán San nền
│   └── ThietKeKenh17.arx      # Thiết kế Kênh thủy lợi
│
├── Support/                   # Thư mục tài nguyên giao diện & Lisp:
│   ├── acad.cui, acad.mnr     # Giao diện gốc AutoCAD
│   ├── VNROAD.cui, TDT3D.cui  # Các menu nghiệp vụ TDT
│   ├── acad.fmp, sample.cus   # Font map và từ điển
│   └── icons/                 # 22 icon thanh công cụ mới chuẩn BIM đã được gắn vào CUI
├── Fonts/                     # Font AutoCAD chuẩn TCVN3, SHX Tiếng Việt
├── Express/                   # Bộ công cụ AutoCAD Express Tools
├── Drv/                       # Trình điều khiển đồ họa DirectX/OpenGL
├── Plotters/ & Plot Styles/   # Cấu hình in ấn và nét in monochrome/color
└── Template/                  # File mẫu bản vẽ thiết kế .DWT
```

---

## 3. CƠ CHẾ NẠP PROFILE VÀ REGISTRY (REGISTRY ARCHITECTURE)

AutoCAD 2007 quản lý toàn bộ đường dẫn và thanh menu thông qua Registry của User:
`HKEY_CURRENT_USER\Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409\Profiles\<Tên Profile>`

Khi `AVRoad.exe` khởi chạy, nó bắt buộc phải thiết lập Profile mang tên **`AVRoad`** với các quy tắc sống còn sau:

### 3.1. Các khóa Registry bắt buộc cấu hình động theo thư mục thực tế (`appDir`):
* `Profiles\AVRoad\General\ACAD`:
  `{appDir}\Support;{appDir}\Fonts;{appDir}\Express;{appDir}\Support\color;{appDir}\Help`  
  *(Cộng thêm đường dẫn Font ngoài nếu người dùng chọn trong AVSetting)*.
* `Profiles\AVRoad\General\PrinterConfigDir`: `{appDir}\Plotters`
* `Profiles\AVRoad\General\PrinterStyleSheetDir`: `{appDir}\Plot Styles`
* `Profiles\AVRoad\General\ColorBookLocation`: `{appDir}\Support\color`
* `Profiles\AVRoad\General\ACADDRV`: `{appDir}\Drv`
* `Profiles\AVRoad\General Configuration\MenuFile`: `{appDir}\Support\acad`
* `Profiles\AVRoad\Editor Configuration\FontMappingFile`: `{appDir}\Support\acad.fmp`

### 3.2. Thứ tự 19 Menu chuẩn trên thanh MenuBar (`Profiles\AVRoad\Menus`):
1. `ACAD pop1` (File)
2. `ACAD pop2` (Edit)
3. `ACAD pop3` (View)
4. `ACAD pop4` (Insert)
5. `ACAD pop5` (Format)
6. `ACAD pop6` (Tools)
7. `ACAD pop7` (Draw)
8. `ACAD pop8` (Dimension)
9. `ACAD pop9` (Modify)
10. `ACAD pop10` (Window)
11. `EXPRESS pop1` (Express Tools)
12. `KSVN POP1` (**AV_KS** - Khảo sát địa hình)
13. `VNROAD POP1` (**AV_Road** - Thiết kế đường)
14. `SANNEN POP1` (**AV_SanNen** - San lấp mặt bằng)
15. `KVN POP1` (**AV_Kenh** - Kênh mương thủy lợi)
16. `DCCT POP1` (**AV_DiaChat** - Địa chất công trình)
17. `TDT3D POP1` (**AV_3D** - Phối cảnh 3D tuyến)
18. `RM POP1` (**AV_VachSon** - Tổ chức giao thông, vạch sơn)
19. `ACAD pop11` (Help)

---

## 4. BẢN ĐỒ LỆNH NGHIỆP VỤ (COMMAND DICTIONARY)

Dưới đây là các lệnh trọng tâm của bộ công cụ để AI/Codex tạo nút bấm giao diện nhanh:

| Phân hệ (Module) | Lệnh tắt chính (AutoCAD Command) | Chức năng nghiệp vụ |
| :--- | :--- | :--- |
| **AV_KS (Khảo sát)** | `DIEM`, `TIN`, `CD`, `DH`, `LTM`, `DXT` | Rải điểm mia trắc đạc, tạo lưới tam giác TIN, mô hình số địa hình, đường đồng mức |
| **AV_Road (Thiết kế Tuyến)** | `T`, `TK`, `CS`, `TT`, `TN`, `TD`, `XTC`, `BTH` | Khởi tạo tuyến, vạch tim tuyến, cong nằm, trắc dọc tự nhiên, trắc ngang, thiết kế áo đường, áp khuôn |
| **AV_SanNen (San nền)** | `SN`, `KTTN`, `TKTN`, `TL`, `TTK` | Tạo lưới ô vuông san nền, gán cao độ tự nhiên/thiết kế, tính toán khối lượng đào đắp |
| **AV_Kenh (Kênh thủy lợi)** | `KNH`, `TKK`, `MCK`, `KLK` | Định tuyến kênh, mặt cắt thiết kế kênh mương, khối lượng đào đắp bờ kênh |
| **AV_DiaChat (Địa chất)** | `DC`, `KDC`, `HDC`, `LDC` | Nhập hố khoan địa chất, vẽ trụ hố khoan trên trắc dọc/ngang, chèn lớp địa chất |
| **AV_VachSon (Vạch sơn - Biển báo)** | `RM`, `RMB`, `BS`, `VS` | Bố trí biển báo giao thông QCVN 41, vạch sơn kẻ đường, vạch đi bộ, gờ giảm tốc |
| **AV_3D (Mô hình 3D)** | `T3D`, `X3D`, `VIEW3D` | Xuất bình đồ tuyến và hành lang đường sang khối 3D trực quan |

---

## 5. CÁC PHƯƠNG ÁN LÀM LẠI GIAO DIỆN (UI REDESIGN STRATEGIES FOR CAD 2007)

AutoCAD 2007 sử dụng thư viện **MFC 8.0 (Microsoft Foundation Classes)** kết hợp Win32 API cổ điển. Cửa sổ chính có window class `Afx:00400000:8:...`. Để "lột xác" giao diện cũ kỹ này thành giao diện phong cách 2026 hiện đại, có **3 phương án kiến trúc khả thi**:

```
+-----------------------------------------------------------------------------------+
| AutoCAD 2007 Main Window (HWND)                                                  |
|  +-----------------------------------------------------------------------------+  |
|  | [Phương án 1 & 3] Modern Top Ribbon / Toolbar (C# WPF / WinUI 3 Injected)    |  |
|  +-----------------------------------------------------------------------------+  |
|  | Modern Quick Action HUD (Đang tuyến, Xuất trắc dọc, Tính đào đắp...)        |  |
|  +-------------------------------------+---------------------------------------+  |
|  | Drawing Canvas (OpenGL/DirectX View)| [Phương án 2] Modern Sidecar Panel   |  |
|  |                                     | (Docked C# WPF / WebView2 HTML5 App)  |  |
|  |                                     | • Danh sách tim tuyến CDE             |  |
|  |                                     | • Quản lý hố khoan, mẫu mặt cắt       |  |
|  |                                     | • Bảng tra tiêu chuẩn đường TCVN      |  |
|  +-------------------------------------+---------------------------------------+  |
|  | Modern Dark Command Line / Status Bar                                       |  |
+-----------------------------------------------------------------------------------+
```

### PHƯƠNG ÁN 1: Modern Sidecar Panel (Khuyến nghị triển khai trước - Cực kỳ ổn định)
* **Nguyên lý:** Xây dựng một ứng dụng C# (.NET Framework 4.8 hoặc .NET 8) sử dụng **WPF** hoặc **WebView2 (HTML/CSS/Tailwind/Vue/React)**.
* **Cách thức gắn kết (Docking):**
  * Dùng Windows Hook (`SetWinEventHook` hoặc `WM_MOVE`, `WM_SIZE` của AutoCAD HWND) để bám chặt vào cạnh phải hoặc cạnh trái của cửa sổ AutoCAD.
  * Hoặc dùng `SetParent(sidecarHwnd, acadHwnd)` để biến panel thành một cửa sổ con thực thụ bên trong AutoCAD.
* **Tương tác với CAD:**
  * Gửi lệnh trực tiếp vào dòng lệnh AutoCAD thông qua Win32 `SendMessage(acadDocHwnd, WM_COPYDATA, ...)` hoặc AutoCAD COM API (`AutoCAD.Application` -> `ActiveDocument.SendCommand("... \n")`).
* **Ưu điểm:**
  * Hoàn toàn không can thiệp sâu vào bộ nhớ của `acad.exe`, **không lo crash ARX**, không bị Safengine chặn.
  * Giao diện đẹp đỉnh cao, hỗ trợ bảng tra TCVN, trực quan hóa mẫu trắc ngang trước khi bấm chèn vào CAD.

---

### PHƯƠNG ÁN 2: Modern Top Ribbon / Floating Modern Toolbar
* **Nguyên lý:** Ẩn các thanh Toolbar cũ kỹ của AutoCAD 2007 đi. Tạo một thanh Ribbon chuẩn BIM hiện đại (Dark Theme `#1e1e1e`, icon SVG/PNG 32x32 sắc nét với hiệu ứng hover bóng bẩy) nổi ngay phía trên vùng vẽ.
* **Tính năng:**
  * Phân chia rõ ràng các Tab:
    1. `Trang Chủ` (Mở file, Lưu, In ấn PDF nhanh, Đo khoảng cách).
    2. `Khảo Sát & Địa Hình` (Nhập số liệu, Vẽ tuyến khảo sát, Tạo mô hình TIN, Đường đồng mức).
    3. `Thiết Kế Tuyến` (Vạch tuyến, Đường cong, Trắc dọc, Áp khuôn trắc ngang).
    4. `Nút Giao & San Nền` (Vuốt nối nút giao, Lưới ô vuông san nền, Bảng khối lượng đào đắp).
    5. `Tổ Chức Giao Thông` (Biển báo chuẩn 41, Vạch sơn các loại, Vạch người đi bộ).
    6. `Cài Đặt & CDE` (Đổi màu nền Đen/Xám BIM, Đổi Font, Cập nhật bản vá GitHub).

---

### PHƯƠNG ÁN 3: Windows Dark Mode Theme Injection (DWM & Visual Styles)
* **Nguyên lý:** Can thiệp vào cửa sổ AutoCAD bằng Win32 API để biến toàn bộ thanh tiêu đề và thanh cuộn thành màu Dark:
  ```csharp
  [DllImport("dwmapi.dll")]
  private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

  // Bật Dark Mode cho Titlebar của AutoCAD trên Windows 10 (build 18985+) và Windows 11:
  const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
  int darkMode = 1;
  DwmSetWindowAttribute(acadMainWindowHandle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));
  ```
* Hook `SetWindowTheme(hwnd, "DarkMode_Explorer", null)` cho các TreeView/ListView trong các hộp thoại TDT.

---

## 6. CÁC LỖI KỸ THUẬT QUAN TRỌNG ĐÃ ĐƯỢC GIẢI MÃ (CRITICAL DEBUG KNOWLEDGE)

Bất kỳ AI hoặc lập trình viên nào làm việc trên repo này **PHẢI GHI NHỚ**:

1. **Lỗi `Access Violation C0000005 at DB353E3h` trong `AlignmentUI17.arx` & `SNVN17.arx`:**
   * **Nguyên nhân cốt lõi:** Profile `AVRoad` trong Registry lưu cứng đường dẫn cũ (`C:\TDT_RESEARCH\AVROAD_PORTABLE`), khiến AutoCAD khi chạy ở thư mục mới (`C:\AVRoad_2026`) nạp chéo module hoặc không tìm thấy các file hỗ trợ trong Support.
   * **Cách xử lý:** Trong `AVRoad_Launcher.cs`, hàm `PrepareProfile` phải **luôn ghi đè động toàn bộ các khóa Registry** (`ACAD`, `MenuFile`, `PrinterConfigDir`, `PlotStyles`, `TemplatePath`) về thư mục thực thi hiện tại `appDir`.
2. **Khóa bảo mật Safengine (`*.rar`):**
   * Các file `Rlthap.rar`, `Rlthua26.rar`, v.v... là tệp key mã hóa khóa cứng của Safengine cho các module ARX.
   * **Tuyệt đối không xóa, không đổi tên các tệp `.rar` này.** Nếu thiếu, AutoCAD sẽ báo lỗi `Requires valid license key (Rlthap.rar)`.
3. **Tệp `identity.ini`:**
   * Đây là tệp định danh của bộ cài AutoCAD 2007. Khi khởi chạy, `AdMigrator.exe` và `acad.exe` kiểm tra tệp này ở thư mục gốc. Luôn giữ tệp này trong gói phân phối.

---

## 7. KẾ HOẠCH BÀN GIAO CHO CODEX / DEVELOPER TIẾP THEO

Khi chuyển tiếp tài liệu này cho Codex, nhiệm vụ ưu tiên cần thực hiện theo thứ tự:
1. **Module 1 (Sidecar UI):** Viết project C# WPF `AVRoad.UI.Sidecar.csproj` làm thanh công cụ nổi/docking thông minh.
2. **Module 2 (Commands Dispatcher):** Sử dụng COM Interop hoặc P/Invoke để gửi lệnh mượt mà từ Sidecar UI vào CAD.
3. **Module 3 (High-DPI Scaling Fix):** Khắc phục tình trạng mờ icon trên màn hình 2K/4K bằng cách vẽ lại icon dạng vector hoặc PNG chuẩn độ phân giải cao.

---
*Tài liệu được sinh tự động và lưu trữ tại gốc repository: `C:\TDT_RESEARCH\CODEX_CAD7_UI_GUIDE.md`*
