# -*- coding: utf-8 -*-
"""
Build script for AVRoad 2026 (TDT 9.1) - Lite Edition.
Packages only AVRoad / TDT 9.1 runtime files into payload_lite.zip (excluding all
stock AutoCAD 2007 binaries, Help, Drv, Express, UserDataCache, MSI, .mnr, and junk),
and compiles Setup_AVRoad_Lite.exe with embedded payload.zip resource.
"""

import hashlib
import os
import shutil
import subprocess
import sys
import zipfile

ROOT = r"C:\TDT_RESEARCH"
APP_SRC = r"C:\AVRoad_2026"
PAYLOAD_RC2 = os.path.join(ROOT, "payload_rc2.zip")
PAYLOAD_LITE = os.path.join(ROOT, "payload_lite.zip")
SETUP_OUT = os.path.join(ROOT, "Setup_AVRoad_Lite.exe")
DESKTOP_OUT = os.path.join(os.path.expanduser("~"), "Desktop", "Setup_AVRoad_Lite.exe")
CSC4 = r"C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"

# 17 core TDT ARX files declared in Acad.rx
ARX_17 = [
    "TDTObjectsUI17.arx",
    "FileManager17.arx",
    "AlignmentUI17.arx",
    "RoadMarkingUI.arx",
    "NhapSoLieuTuyen17.arx",
    "TDTHorizontalProfileDesign.arx",
    "TDTInputData.arx",
    "BtBD17.arx",
    "ProjectDH17.arx",
    "TaoDiaVat17.arx",
    "ProjectSohoa17.arx",
    "Utils17.arx",
    "SNVN17.arx",
    "ThietKeKenh17.arx",
    "DrawSymbols.arx",
    "DCCT.arx",
    "RoadSignsUI.arx",
]

# Additional TDT 9.1 native/runtime files required alongside Acad.rx
TDT_CORE_FILES = [
    "Acad.rx",
    "TDTObjects17.dbx",
    "AlignmentDb17.dbx",
    "TDTObjectsWrapper.arx",
    "GeologicLayerManager.arx",
    "GeologyLayerManager.arx",
    "libxl.dll",
    "UsbCrypto.dll",
    "RM.dll",
    "DCCT.dll",
    "RM.ini",
    "MaKSDCCT.dat",
    "dnmc.dcct",
    "DauCo.dwg",
    "KSVN.Palette.xml",
    "KSVN.PaletteSet.xml",
    "tdt.bmp",
    "acad.lin",
    "TDT.shx",
    "giaothong1.ttf",
    "giaothong2.ttf",
    "banner_144.bmp",
    "banner_146.bmp",
    "vcredist_x86.exe",
]

TDT_DIRS = ["Data", "CTTK", "Lib", "Blocks", "DCCT"]

CUI_MENUS_7 = [
    "DCCT.cui",
    "KSVN.cui",
    "KVN.cui",
    "RM.cui",
    "SANNEN.cui",
    "TDT3D.cui",
    "VNROAD.cui",
]

AVROAD_APP_FILES = [
    "AVRoad.exe",
    "AVRoad.Deployment.dll",
    "AVUpdater.exe",
    "AVUpdater.v2.exe",
    "AVLicense.exe",
    "GetKey.exe",
    "AVSetting.exe",
    "Uninstall.exe",
    "version.json",
    "AVRoad.ini",
    "AVRoad.ico",
    "AVRoad_Profile.arg",
    "Profile.aws",
    "FixedProfile.aws",
    "acad.exe.config",
    "startup_bg.png",
    "startup_bg_clean.png",
    "av_bim_logo.png",
    "av_bim_logo_transparent.png",
    "av_mark.png",
    "AVRoad_Icon_256.png",
    "AVRoad_Logo.png",
]

JUNK_EXTS = {
    ".bak", ".mnr", ".log", ".scr", ".tmp", ".err", ".dmp",
    ".cs", ".py", ".ps1", ".old", ".msi", ".zip", ".dwl", ".dwl2",
}

FORBIDDEN_TOP_DIRS = {
    "help", "drv", "express", "userdatacache", "template",
    "en-us", "microsoft.vc80.mfc", "microsoft.vc80.crt",
    "webdepot", "upi", "webservices", "cer", ".avroad-rollback",
    "diagnostics", "config", "data links", "plotters", "plot styles",
}

FORBIDDEN_STOCK_FILES = {
    "acad.exe", "acdb17.dll", "acge17.dll", "acgex17.dll", "acui17.dll",
    "adui17.dll", "ac1st17.dll", "axdb.dll", "heidi9.dll", "vl.arx",
    "tdt9.1.exe", "tdtsolution9.1cad2007.msi", "acad.bak.cui",
}


class SourceResolver:
    """Resolves runtime files from C:\\TDT_RESEARCH -> C:\\AVRoad_2026 -> payload_rc2.zip."""

    def __init__(self):
        self.rc2_zip = None
        self.rc2_index = {}
        if os.path.exists(PAYLOAD_RC2):
            self.rc2_zip = zipfile.ZipFile(PAYLOAD_RC2, "r")
            for info in self.rc2_zip.infolist():
                if not info.is_dir():
                    norm = info.filename.replace("\\", "/").lstrip("/")
                    self.rc2_index[norm.lower()] = info

    def close(self):
        if self.rc2_zip is not None:
            self.rc2_zip.close()
            self.rc2_zip = None

    def read_bytes(self, rel_path, prefer_root=False):
        norm = rel_path.replace("\\", "/").lstrip("/")
        candidates = []
        if prefer_root:
            candidates.append(os.path.join(ROOT, norm.replace("/", os.sep)))
            candidates.append(os.path.join(APP_SRC, norm.replace("/", os.sep)))
        else:
            candidates.append(os.path.join(APP_SRC, norm.replace("/", os.sep)))
            candidates.append(os.path.join(ROOT, norm.replace("/", os.sep)))

        for fp in candidates:
            if os.path.isfile(fp):
                with open(fp, "rb") as f:
                    return f.read(), fp

        info = self.rc2_index.get(norm.lower())
        if info is not None and self.rc2_zip is not None:
            return self.rc2_zip.read(info.filename), f"{PAYLOAD_RC2}!{info.filename}"

        return None, None


def ensure_avsetting_compiled():
    src = os.path.join(ROOT, "AVSetting.cs")
    exe = os.path.join(ROOT, "AVSetting.exe")
    if os.path.exists(src) and (
        not os.path.exists(exe) or os.path.getmtime(src) > os.path.getmtime(exe)
    ):
        print("[0/3] Compiling latest AVSetting.exe from AVSetting.cs...")
        cmd = [
            CSC4,
            "/nologo",
            "/target:winexe",
            "/platform:x86",
            f"/win32icon:{os.path.join(ROOT, 'AVRoad.ico')}",
            "/r:System.dll",
            "/r:System.Drawing.dll",
            "/r:System.Windows.Forms.dll",
            f"/out:{exe}",
            src,
        ]
        subprocess.check_call(cmd)
        print(f"  AVSetting.exe built: {os.path.getsize(exe):,} bytes")


def is_junk_name(filename):
    low = filename.lower()
    if low in FORBIDDEN_STOCK_FILES or low.endswith(".bak.cui"):
        return True
    ext = os.path.splitext(low)[1]
    return ext in JUNK_EXTS


def build_payload_lite():
    print("\n[1/3] Building payload_lite.zip (AVRoad / TDT 9.1 Lite files only)...")
    resolver = SourceResolver()
    seen = set()
    stats = {}
    missing = []

    if os.path.exists(PAYLOAD_LITE):
        os.remove(PAYLOAD_LITE)

    def add_entry(zf, arc_path, data, category):
        norm = arc_path.replace("\\", "/").lstrip("/")
        low = norm.lower()
        top = low.split("/")[0] if "/" in low else ""
        if top in FORBIDDEN_TOP_DIRS:
            raise RuntimeError(f"Forbidden directory slipped into Lite payload: {norm}")
        if is_junk_name(os.path.basename(norm)):
            raise RuntimeError(f"Forbidden/junk file slipped into Lite payload: {norm}")
        if low in seen:
            return
        seen.add(low)
        zi = zipfile.ZipInfo(norm)
        zi.compress_type = zipfile.ZIP_DEFLATED
        zf.writestr(zi, data, compress_type=zipfile.ZIP_DEFLATED, compresslevel=9)
        info = zf.infolist()[-1]
        if category not in stats:
            stats[category] = [0, 0, 0]
        stats[category][0] += 1
        stats[category][1] += info.file_size
        stats[category][2] += info.compress_size

    try:
        with zipfile.ZipFile(PAYLOAD_LITE, "w", zipfile.ZIP_DEFLATED, compresslevel=9) as zf:
            # 1. 17 ARX files in Acad.rx
            for fn in ARX_17:
                data, _ = resolver.read_bytes(fn, prefer_root=False)
                if data is None:
                    missing.append(fn)
                else:
                    add_entry(zf, fn, data, "17_ARX")

            # 2. TDT Core DBX/DLL/Data/Fonts/vcredist
            for fn in TDT_CORE_FILES:
                prefer = fn in ("Acad.rx", "acad.exe.config")
                data, _ = resolver.read_bytes(fn, prefer_root=prefer)
                if data is None:
                    missing.append(fn)
                else:
                    add_entry(zf, fn, data, "TDT_Core")
                    if fn in ("TDT.shx", "giaothong1.ttf", "giaothong2.ttf"):
                        add_entry(zf, f"Fonts/{fn}", data, "Fonts_TDT")

            # 3. 8 *.rar files, custom TDT *.pat files, Vn*.shx fonts
            root_candidates = set()
            if os.path.isdir(APP_SRC):
                for fn in os.listdir(APP_SRC):
                    if os.path.isfile(os.path.join(APP_SRC, fn)):
                        root_candidates.add(fn)
            for k in resolver.rc2_index:
                if "/" not in k:
                    root_candidates.add(resolver.rc2_index[k].filename)

            for fn in sorted(root_candidates):
                low = fn.lower()
                if low.endswith(".rar"):
                    data, _ = resolver.read_bytes(fn, prefer_root=False)
                    if data is not None:
                        add_entry(zf, fn, data, "RAR_8")
                elif low.endswith(".pat"):
                    data, _ = resolver.read_bytes(fn, prefer_root=False)
                    if data is not None:
                        add_entry(zf, fn, data, "PAT_TDT")
                elif low.startswith("vn") and low.endswith(".shx"):
                    data, _ = resolver.read_bytes(fn, prefer_root=False)
                    if data is not None:
                        add_entry(zf, fn, data, "Fonts_TDT")
                        add_entry(zf, f"Fonts/{fn}", data, "Fonts_TDT")

            # 4. TDT Directories: Data/, CTTK/, Lib/, Blocks/, DCCT/
            for d in TDT_DIRS:
                dir_added = 0
                dp = os.path.join(APP_SRC, d)
                if os.path.isdir(dp):
                    for r, _, fs in os.walk(dp):
                        for fn in sorted(fs):
                            if is_junk_name(fn):
                                continue
                            fp = os.path.join(r, fn)
                            rel = os.path.relpath(fp, APP_SRC).replace("\\", "/")
                            with open(fp, "rb") as f:
                                add_entry(zf, rel, f.read(), f"Dir_{d}")
                            dir_added += 1
                else:
                    prefix = d.lower() + "/"
                    for k, info in sorted(resolver.rc2_index.items()):
                        if k.startswith(prefix) and not is_junk_name(os.path.basename(k)):
                            add_entry(zf, info.filename.replace("\\", "/"), resolver.rc2_zip.read(info.filename), f"Dir_{d}")
                            dir_added += 1
                if dir_added == 0:
                    missing.append(f"{d}/")

            # 5. 7 CUI menus + Support/acad.cui + Support/acad.mnl + acad2007doc.lsp + Tools/*.vlx + 22 av_*.bmp
            for cui in CUI_MENUS_7:
                data, _ = resolver.read_bytes(cui, prefer_root=True)
                if data is None:
                    missing.append(cui)
                else:
                    add_entry(zf, cui, data, "CUI_7")
                    add_entry(zf, f"Support/{cui.lower()}", data, "CUI_7")

            for sup_file, prefer in [
                ("Support/acad.cui", False),
                ("Support/acad.mnl", False),
                ("Support/acad2007doc.lsp", True),
            ]:
                data, _ = resolver.read_bytes(sup_file, prefer_root=prefer)
                if data is None:
                    missing.append(sup_file)
                else:
                    add_entry(zf, sup_file, data, "Support_AV")

            # Tools/*.vlx
            tools_dir = os.path.join(ROOT, "Support", "Tools")
            vlx_count = 0
            if os.path.isdir(tools_dir):
                for fn in sorted(os.listdir(tools_dir)):
                    if fn.lower().endswith(".vlx"):
                        with open(os.path.join(tools_dir, fn), "rb") as f:
                            add_entry(zf, f"Support/Tools/{fn}", f.read(), "Support_AV")
                        vlx_count += 1
            if vlx_count == 0:
                missing.append("Support/Tools/*.vlx")

            # 22 av_*.bmp icons
            sup_src = os.path.join(APP_SRC, "Support")
            bmp_count = 0
            if os.path.isdir(sup_src):
                for fn in sorted(os.listdir(sup_src)):
                    if fn.lower().startswith("av_") and fn.lower().endswith(".bmp"):
                        with open(os.path.join(sup_src, fn), "rb") as f:
                            bmp_bytes = f.read()
                        add_entry(zf, f"Support/{fn}", bmp_bytes, "Icons_22")
                        add_entry(zf, f"Support/icons/{fn}", bmp_bytes, "Icons_22")
                        bmp_count += 1
            else:
                for k, info in sorted(resolver.rc2_index.items()):
                    if k.startswith("support/av_") and k.endswith(".bmp"):
                        bmp_bytes = resolver.rc2_zip.read(info.filename)
                        fn = os.path.basename(k)
                        add_entry(zf, f"Support/{fn}", bmp_bytes, "Icons_22")
                        add_entry(zf, f"Support/icons/{fn}", bmp_bytes, "Icons_22")
                        bmp_count += 1
            if bmp_count < 22:
                missing.append(f"Support/av_*.bmp (only {bmp_count}/22)")

            # 6. AVRoad application files + UI21
            for fn in AVROAD_APP_FILES:
                data, _ = resolver.read_bytes(fn, prefer_root=True)
                if data is None:
                    missing.append(fn)
                else:
                    add_entry(zf, fn, data, "AVRoad_App")

            for ui_file in ["UI21/AVRoad.SinglePanel21.dll", "UI21/commands.xml"]:
                data, _ = resolver.read_bytes(ui_file, prefer_root=True)
                if data is None:
                    missing.append(ui_file)
                else:
                    add_entry(zf, ui_file, data, "UI21")

    finally:
        resolver.close()

    if missing:
        raise RuntimeError(f"Missing required files for Lite payload: {missing}")

    zip_bytes = os.path.getsize(PAYLOAD_LITE)
    tot_files = sum(v[0] for v in stats.values())
    tot_uncomp = sum(v[1] for v in stats.values())
    tot_comp = sum(v[2] for v in stats.values())
    print(f"  payload_lite.zip built: {tot_files} files | uncompressed={tot_uncomp/1024/1024:.2f} MB | zip={zip_bytes:,} bytes ({zip_bytes/1024/1024:.2f} MB)")
    for cat, (cnt, u, c) in sorted(stats.items(), key=lambda x: -x[1][2]):
        print(f"    - {cat:12s}: {cnt:4d} files | uncomp={u/1024/1024:6.2f} MB | comp={c/1024/1024:6.2f} MB")
    return zip_bytes


def compile_setup_lite():
    print("\n[2/3] Compiling Setup_AVRoad_Lite.exe (.NET 4.0 x86, requireAdministrator)...")
    manifest_path = os.path.join(ROOT, "setup.manifest")
    cmd = [
        CSC4,
        "/nologo",
        "/target:winexe",
        "/platform:x86",
        f"/win32manifest:{manifest_path}",
        f"/out:{SETUP_OUT}",
        f"/win32icon:{os.path.join(ROOT, 'AVRoad.ico')}",
        "/r:System.dll",
        "/r:System.Core.dll",
        "/r:System.Xml.dll",
        "/r:System.Drawing.dll",
        "/r:System.Windows.Forms.dll",
        "/r:System.IO.Compression.dll",
        "/r:System.IO.Compression.FileSystem.dll",
        f"/resource:{PAYLOAD_LITE},payload.zip",
        os.path.join(ROOT, "Setup_AVRoad_Lite.cs"),
        os.path.join(ROOT, "CADRegistryConfig.cs"),
        os.path.join(ROOT, "Cad7ComRegistration.cs"),
        os.path.join(ROOT, "PatchTransaction.cs"),
    ]
    subprocess.check_call(cmd)
    shutil.copy2(SETUP_OUT, DESKTOP_OUT)
    print(f"  Compiled: {SETUP_OUT}")
    print(f"  Copied to Desktop: {DESKTOP_OUT}")


def report_hashes():
    print("\n[3/3] Release Artifacts Summary & SHA-256:")
    for path in [PAYLOAD_LITE, SETUP_OUT, DESKTOP_OUT]:
        sz = os.path.getsize(path)
        with open(path, "rb") as f:
            h = hashlib.sha256(f.read()).hexdigest().upper()
        print(f"  {os.path.basename(path):24s} | {sz:12,d} bytes ({sz/1024/1024:.2f} MB) | SHA256={h}")


def main():
    ensure_avsetting_compiled()
    build_payload_lite()
    if "--payload-only" in sys.argv:
        return
    compile_setup_lite()
    report_hashes()


if __name__ == "__main__":
    main()
