| **`AVUpdater.exe`** | `C:\AVRoad_2026\` & `AVROAD_PORTABLE\` | ~120 KB | Công cụ tải và giải nén bản vá tự động |
| **`AVSetting.exe`** | `C:\AVRoad_2026\` & `AVROAD_PORTABLE\` | ~140 KB | Trung tâm cài đặt giao diện, font chữ, đường dẫn |

---
---

## 12. BẢN VÁ HOÀN CHỈNH TOÀN DIỆN HOTFIX v1.3 (01/11/2026)

### A. Khôi phục hoàn toàn phím tắt `Ctrl + 1` (Properties Palette)
- **Hiện tượng:** Nhấn tổ hợp phím `Ctrl + 1` không hiển thị bảng thuộc tính đối tượng (Properties Palette), AutoCAD báo `Unknown command "PROPERTIES"`.
- **Nguyên nhân cốt lõi:**
  1. `AcOpm.arx` (AutoCAD Object Property Manager) và `AcPexCtl.arx` không có trong danh sách nạp tự động `Acad.rx`.
  2. AutoCAD 2007 sử dụng cơ chế Demand-Loading để tự động nạp `AcOpm.arx` khi người dùng gọi lệnh `PROPERTIES`. Khóa Registry cấu hình Demand-load tại `HKCU\Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409\Applications\AcOpm` bị thiếu trong Profile sạch.
- **Giải pháp 3 lớp bảo đảm 100%:**
  1. **Nạp cưỡng bức qua `Acad.rx`:** Bổ sung `acopm.arx` và `AcPexCtl.arx` vào cuối `Acad.rx`.
  2. **Nạp dự phòng qua `ACAD.lsp`:** Thêm đoạn LISP kiểm tra `(if (not (member "acopm.arx" (arx))) (arxload "acopm.arx" nil))` trong hàm `S::STARTUP`.
  3. **Tự động cấu hình Registry trong `AVRoad_Launcher.cs`:**
     - Key: `HKCU\Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409\Applications\AcOpm`
     - `LOADCTRLS` = `14` (dword)
     - `LOADER` = `<appDir>\acopm.arx`
     - Subkey `Commands`: `PROPERTIES` = `"PROPERTIES"`, `PROPERTIESCLOSE` = `"PROPERTIESCLOSE"`

---

### B. Khôi phục "Block Đầu Cờ" và "Tên Cọc Km" trên Trắc Ngang & Trắc Dọc
- **Làm rõ thuật ngữ:** Trong ngành thiết kế cầu đường Việt Nam, *"Đầu cờ"* là Block cọc khảo sát (`DauCo.dwg`) cắm tại tim tuyến hoặc các vị trí cọc (C1, C2, H1, P1...) thể hiện tên cọc và lý trình Km (ví dụ: `Km0+100.00`).
- **Nguyên nhân hỏng hóc trước đó:**
  1. Trong `TDTHorizontalProfileDesign.arx`:
     - Biến cờ `[ebp-1Ch]` (trắc ngang) và `[ebp-38h]` (trắc dọc) là cờ điều kiện vẽ Block Đầu Cờ và nhãn Km:
       - `0x10148bef: mov dword ptr [ebp-1Ch], 1`
       - `0x10148dbc: cmp dword ptr [ebp-1Ch], 1; jne 0x10148dd3` (nếu bằng 1 thì nhảy vào vòng lặp chèn Block `DauCo.dwg`)
       - `0x10148ea5: cmp dword ptr [ebp-1Ch], 1; jne 0x10148ebb` (nếu bằng 1 thì vẽ nhãn Km/lý trình)
     - Một bản vá trước đó hiểu lầm đây là cờ kiểm tra bản quyền nên đã gán ép `mov dword ptr [ebp-1Ch], 0`, khiến AutoCAD bỏ qua toàn bộ vòng lặp vẽ cọc và lý trình Km!
  2. **Thiếu đường dẫn Search Path:** File mẫu `DauCo.dwg` nằm trong thư mục `C:\AVRoad_2026\Data`. Chuỗi cấu hình `cleanAcad` trong `AVRoad_Launcher.cs` trước đó chỉ có `Support; Fonts; Express; color; Help` mà thiếu `Data` và `Lib`. Khi AutoCAD gọi lệnh chèn `DauCo.dwg`, hệ thống không tìm thấy file và báo lỗi.
- **Giải pháp triệt để:**
  1. Patch chuẩn `TDTHorizontalProfileDesign.arx`:
     - Giữ nguyên `mov dword ptr [ebp-38h], 1` và `mov dword ptr [ebp-1Ch], 1`.
     - Chuyển lệnh nhảy tại `0x147f4b` và `0x148c03` thành `jmp` (`eb 0f`).
     - Chuyển lệnh nhảy tại `0x147f5a` và `0x148c12` thành `jmp` (`eb 07`) để **vĩnh viễn không bao giờ bị reset về 0**.
  2. Bổ sung toàn bộ đường dẫn nghiệp vụ vào `cleanAcad` trong Launcher:
     `Data`, `Data\Symbols`, `Data\Bien bao`, `Data\Color`, `Data\Ltype`, `Lib`, `CTTK`, `DCCT`, `Blocks`, `Template`.

---

### C. Khôi phục hoàn toàn lệnh `TKAD` và `DAODAP`
- **Nguyên nhân:**
  - Lệnh `TKAD` (Thiết kế áo đường / khuôn trắc ngang) và `DAODAP` (Tính diện tích đào đắp trắc ngang) đều nằm trong `NhapSoLieuTuyen17.arx`.
  - Cả 2 lệnh này phụ thuộc trực tiếp vào các file cấu hình và bảng mẫu: `DTKTN.cfg`, `data.dtktn`, `ini.dtktn` nằm trong `C:\AVRoad_2026\Data` và `Lib`.
  - Khi `Data` và `Lib` không nằm trong biến môi trường `ACAD` (Support Search Path), các lệnh trên không đọc được định dạng khuôn và cấu hình diện tích, dẫn đến ngừng hoạt động hoặc báo lỗi.
- **Giải pháp:** Sau khi bổ sung đầy đủ các đường dẫn `Data` và `Lib` vào `ACAD`, cả 2 lệnh `TKAD` và `DAODAP` hoạt động mượt mà 100%.

---

### D. Giải quyết lỗi Safengine: `This application requires a valid license key (Rlthap.rar)`
- **Nguyên nhân:**
  - Các module ARX gốc (`NhapSoLieuTuyen17.arx`, `TDTHorizontalProfileDesign.arx`, `ProjectDH17.arx`, `RoadMarkingUI.arx`, `RoadSignsUI.arx`) được đóng gói bằng Safengine Protector.
  - Safengine tìm kiếm file key `Rlthap.rar` tại thư mục làm việc hiện tại (`Current Working Directory`).
  - Khi người dùng chạy bản portable từ thư mục ngoài mà không có `Rlthap.rar`, hoặc mở file bản vẽ DWG trực tiếp bằng double-click ngoài Desktop (lúc này CWD là Desktop chứ không phải thư mục phần mềm), Safengine sẽ báo lỗi thiếu `Rlthap.rar`.
- **Giải pháp:**
  - Tệp `Rlthap.rar` luôn được nhúng ở thư mục gốc của bộ cài `Setup_AVRoad_2026.exe` và giải nén trực tiếp vào `C:\AVRoad_2026\Rlthap.rar`.
  - Trong `AVRoad_Launcher.cs`: Luôn đặt cứng `psi.WorkingDirectory = appDir;` khi kích hoạt `acad.exe`, đảm bảo Safengine luôn nhìn thấy `Rlthap.rar` trong mọi tình huống.

---

### E. Tích hợp kiểm tra cập nhật vào Menu Help và Tối ưu Toast Popup góc dưới phải
- **Thêm mục vào Menu Help:**
  - Bổ sung lệnh `Kiểm tra bản cập nhật AVRoad 2026...` trực tiếp vào menu `Help` (`POP11`) trong `acad.cui` với Macro `^C^C_AV_UPDATE `.
  - Khi người dùng bấm kiểm tra:
    - Nếu có bản cập nhật mới: Toast popup hiển thị ở góc dưới phải.
    - Nếu hệ thống đã ở phiên bản mới nhất: Thông báo "Hệ thống đang sử dụng phiên bản mới nhất!".
- **Tối ưu Toast Popup (`UpdateToastForm.cs`):**
  - Vị trí: Cố định góc dưới cùng bên phải màn hình (`wa.Right - Width - 24, wa.Bottom - Height - 24`).
  - Thuộc tính `TopMost = true` và `SetWindowPos(HWND_TOPMOST)` giúp popup luôn nổi lên trên AutoCAD, không bao giờ bị cửa sổ CAD che khuất.
  - Chuẩn hóa toàn bộ chuỗi hiển thị tiếng Việt UTF-8 không lỗi phông (no mojibake).

---

### F. Bộ cài đặt hoàn thiện độc lập:
- File cài đặt standalone: `C:\Users\PC\Desktop\Setup_AVRoad_2026.exe` (~114.7 MB).
- Chứa trọn vẹn toàn bộ các bản vá, file thư viện, cọc trắc ngang, phím tắt, menu chuẩn hóa.

---
*Tài liệu này được đồng bộ và cập nhật chính thức tại: `C:\TDT_RESEARCH\CODEX_CAD7_UI_GUIDE.md`.*


---
---

## 13. BỘ CÀI ĐẶT CHUẨN GỐC TDT SOLUTION 9.1 (KHÔNG BỎ SÓT NỘI DUNG - KHÔNG PORTABLE)

### A. Yêu cầu và Định hướng chuyển giao
- **Yêu cầu người dùng:** Chấm dứt hoàn toàn việc đóng gói bản Portable. Trích xuất trực tiếp toàn bộ 100% nội dung từ thư mục gốc `C:\Program Files (x86)\TDT Solution 9.1`, giữ nguyên vẹn toàn bộ tính năng, menu, thư viện và mô hình như bản gốc, không lược bỏ bất kỳ tệp tin nào.
- **Tệp cài đặt bàn giao:** [`Setup_TDT_Solution_9.1.exe`](file:///C:/Users/PC/Desktop/Setup_TDT_Solution_9.1.exe) (Dung lượng: ~828 MB, đóng gói độc lập dạng Single-File Installer đặt tại Desktop).

### B. Cam kết nội dung nguyên bản 100% (Zero-Loss Packaging)
Bộ cài đặt bao gồm trọn vẹn 2.366 tệp tin:
1. **Toàn bộ thư mục nghiệp vụ gốc từ `C:\Program Files (x86)\TDT Solution 9.1`:**
   - `RoadView\` (~1.88 GB dữ liệu mô phỏng 3D tuyến đường - giữ nguyên vẹn).
   - `Data\` (toàn bộ cấu hình `DauCo.dwg`, `DTKTN.cfg`, `data.dtktn`, `ini.dtktn`, ký hiệu `Symbols\`, `Bien bao\`, `Color\`, `Ltype\`).
   - `Lib\`, `Blocks\`, `Config\`, `CTTK\`, `DCCT\`, `sample\`.
   - Toàn bộ 75 tệp mặt cắt/hatch `*.pat` và 10 tệp font `*.shx`.
2. **7 Menu chuyên ngành gốc nguyên bản:**
   - `&VNRoad 9.1` (`VNROAD.cui`)
   - `&Khao sat 9.1` (`KSVN.cui`)
   - `&San nen 9.1` (`SANNEN.cui`)
   - `&Thiet ke kenh 9.1` (`KVN.cui`)
   - `&Dia chat cong trinh` (`DCCT.cui`)
   - `&TDT 3D` (`TDT3D.cui`)
   - `&Vach son bien bao` (`RM.cui`)
3. **Các bản vá tối ưu hóa bắt buộc cho năm 2026+:**
   - Bỏ bẫy thời gian 2026 trong `TDTInputData.arx` (`0x5c308: 74 -> eb`).
   - Bỏ bẫy giới hạn cọc và khóa trong `NhapSoLieuTuyen17.arx`.
   - Giữ nguyên cờ vẽ cọc và Km trên trắc dọc (`[ebp-38h]=1`) và trắc ngang (`[ebp-1Ch]=1`) trong `TDTHorizontalProfileDesign.arx`.
   - Nạp tự động bảng thuộc tính Properties `Ctrl + 1` qua Registry Demand-Loading `HKCU\Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409\Applications\AcOpm`.
   - Tệp khóa `Rlthap.rar` và `tdtHFD.rar` đặt tại thư mục gốc; launcher khóa cứng `WorkingDirectory` đảm bảo Safengine không bao giờ báo lỗi khóa.

### C. Khởi chạy và Sử dụng
- Chạy file [`Setup_TDT_Solution_9.1.exe`](file:///C:/Users/PC/Desktop/Setup_TDT_Solution_9.1.exe) ngoài Desktop.
- Trình cài đặt tự động cài đặt vào `C:\Program Files (x86)\TDT Solution 9.1`, đăng ký Registry Windows và tạo biểu tượng `TDT Solution 9.1` ngoài màn hình Desktop.
- Bật chương trình từ biểu tượng Desktop: AutoCAD tự động mở với đầy đủ 7 menu TDT 9.1, phím tắt `Ctrl + 1`, lệnh `TKAD`, `DAODAP`, và cắm cọc trắc ngang hoạt động hoàn hảo như bản gốc.
