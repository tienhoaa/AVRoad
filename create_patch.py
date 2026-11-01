"""
AVRoad - Patch Creator Tool (Công cụ đóng gói Bản Vá Hotfix cho GitHub)
Quy tắc đặt phiên bản:
- Bắt đầu: v1.0
- Cập nhật nhẹ / sửa lỗi / icon / lsp: 1.1, 1.2, 1.3...
- Cập nhật lớn / thêm menu / tính năng lớn: 2.0, 3.0...
"""

import os
import sys
import json
import zipfile
import datetime

def suggest_next_version(ver_str):
    try:
        parts = ver_str.split('.')
        major = int(parts[0])
        minor = int(parts[1]) if len(parts) > 1 else 0
        return f"{major}.{minor + 1}"
    except:
        return "1.1"

def main():
    print("=" * 65)
    print("      AVRoad - CONG CU DONG GOI BAN VA HOTFIX GITHUB       ")
    print("=" * 65)

    base_dir = os.path.dirname(os.path.abspath(__file__))
    portable_dir = os.path.join(base_dir, "AVROAD_PORTABLE")

    if not os.path.exists(portable_dir):
        print(f"Lỗi: Không tìm thấy thư mục {portable_dir}")
        return

    # Đọc version hiện tại từ version.json
    ver_path = os.path.join(portable_dir, "version.json")
    current_ver = "1.0"
    if os.path.exists(ver_path):
        try:
            with open(ver_path, "r", encoding="utf-8") as f:
                data = json.load(f)
                current_ver = data.get("version", current_ver)
        except:
            pass

    suggested_ver = suggest_next_version(current_ver)

    print(f"\nPhiên bản hiện tại: v{current_ver}")
    print(f"Gợi ý: Cập nhật nhẹ -> v{suggested_ver} | Cập nhật lớn/thêm menu -> v2.0, v3.0...")
    new_ver = input(f"Nhập số phiên bản mới cần phát hành [Mặc định: {suggested_ver}]: ").strip()
    if not new_ver:
        new_ver = suggested_ver

    notes = input("Nhập nội dung thay đổi (Changelog): ").strip()
    if not notes:
        notes = f"Bản cập nhật v{new_ver}: Tối ưu giao diện và cải tiến tính năng."

    github_repo = input("Nhập GitHub Repo [mặc định: tienhoaa/AVRoad]: ").strip()
    if not github_repo:
        github_repo = "tienhoaa/AVRoad"

    patch_filename = f"patch_{new_ver}.zip"
    patch_path = os.path.join(base_dir, patch_filename)

    # Danh sách các tệp tin cập nhật trong bản vá
    candidate_relpaths = [
        "AVRoad.exe",
        "AVSetting.exe",
        "AVUpdater.exe",
        "AVRoad.ini",
        "version.json",
        "CHAY_NGAY.bat",
        "Support/acad.cui",
        "Support/acad.mnl",
        "Support/acad2007.lsp",
        "Support/acad2007doc.lsp",
    ]

    icons_dir = os.path.join(portable_dir, "Support", "icons")
    if os.path.exists(icons_dir):
        for f in os.listdir(icons_dir):
            candidate_relpaths.append(os.path.join("Support", "icons", f))

    # Đóng gói zip
    print(f"\nĐang nén các tệp bản vá vào {patch_filename}...")
    with zipfile.ZipFile(patch_path, "w", compression=zipfile.ZIP_DEFLATED) as zf:
        packed_count = 0
        for rel in candidate_relpaths:
            full = os.path.join(portable_dir, rel)
            if os.path.exists(full):
                zf.write(full, rel)
                packed_count += 1
                print(f"  + {rel}")

    patch_size_kb = os.path.getsize(patch_path) / 1024
    print(f"\n=> Đã đóng gói thành công {packed_count} tệp! Dung lượng bản vá: {patch_size_kb:.1f} KB")

    # Cập nhật version.json
    download_url = f"https://github.com/{github_repo}/releases/download/v{new_ver}/{patch_filename}"
    new_info = {
        "version": new_ver,
        "release_date": datetime.date.today().isoformat(),
        "patch_name": f"AVRoad v{new_ver} Hotfix",
        "patch_download_url": download_url,
        "google_drive_installer": "https://drive.google.com/drive/folders/YOUR_FOLDER_ID",
        "google_drive_portable": "https://drive.google.com/drive/folders/YOUR_FOLDER_ID",
        "changelog": notes
    }

    with open(ver_path, "w", encoding="utf-8") as f:
        json.dump(new_info, f, ensure_ascii=False, indent=2)

    repo_ver_path = os.path.join(base_dir, "version.json")
    with open(repo_ver_path, "w", encoding="utf-8") as f:
        json.dump(new_info, f, ensure_ascii=False, indent=2)

    print("\n" + "=" * 65)
    print(f"HOÀN TẤT ĐÓNG GÓI BẢN VÁ v{new_ver}! HƯỚNG DẪN ĐẨY LÊN GITHUB:")
    print("=" * 65)
    print(f"1. Commit và push file 'version.json' lên nhánh main của GitHub:")
    print(f"     git add version.json")
    print(f"     git commit -m \"Phat hanh ban va v{new_ver}\"")
    print(f"     git push origin main")
    print(f"2. Vào GitHub -> Releases -> 'Draft a new release':")
    print(f"     - Tag: v{new_ver}")
    print(f"     - Release title: AVRoad Hotfix v{new_ver}")
    print(f"     - Đính kèm file: {patch_filename} ({patch_size_kb:.1f} KB)")
    print(f"3. Khi người dùng mở AVRoad trên máy tính:")
    print(f"     - Góc màn hình sẽ tự động hiện Popup: 'AVRoad - Bản vá mới v{new_ver}'")
    print(f"     - Người dùng bấm 'CẬP NHẬT NGAY', phần mềm sẽ tự tải và vá xong trong vài giây!")
    print("=" * 65)

if __name__ == "__main__":
    main()
