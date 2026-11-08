# -*- coding: utf-8 -*-
import os, subprocess, zipfile, hashlib, json, shutil

root = r"C:\TDT_RESEARCH"
csc4 = r"C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"
desk_test = r"C:\Users\PC\Desktop\AVRoad_RC2_Test"
desk_root = r"C:\Users\PC\Desktop"
ui21 = os.path.join(root, "UI21")
tools = os.path.join(root, "Support", "Tools")
drv_xml = r"C:\AVRoad_2026\drv\AdskHwCertificationDatabase.xml"

print("====================================================================")
print("  BUILDING & PACKAGING AVROAD 2026 / TDT 9.1 (LICENSE ENFORCEMENT)")
print("====================================================================")

# 1. Update version metadata
with open(os.path.join(root, "version.json"), "r", encoding="utf-8") as f:
    version_meta = json.load(f)

# 2. Build patch_2.1_rc2.zip
print("\n[1/5] Building patch_2.1_rc2.zip (4 core files strictly validated by PatchTransaction)...")
temp_ver_path = os.path.join(root, "version_inner.json")
with open(temp_ver_path, "w", encoding="utf-8") as f:
    json.dump(version_meta, f, ensure_ascii=False, indent=2)

patch_zip_path = os.path.join(root, "patch_2.1_rc2.zip")
if os.path.exists(patch_zip_path):
    os.remove(patch_zip_path)

with zipfile.ZipFile(patch_zip_path, "w", zipfile.ZIP_DEFLATED) as zf:
    zf.write(os.path.join(root, "AVRoad.exe"), "AVRoad.exe")
    zf.write(os.path.join(root, "AVRoad.Deployment.dll"), "AVRoad.Deployment.dll")
    zf.write(os.path.join(root, "AVUpdater.v2.exe"), "AVUpdater.v2.exe")
    zf.write(temp_ver_path, "version.json")

os.remove(temp_ver_path)

with open(patch_zip_path, "rb") as f:
    patch_sha256 = hashlib.sha256(f.read()).hexdigest().lower()

version_meta["patch_sha256"] = patch_sha256
print(f"  patch_2.1_rc2.zip size: {os.path.getsize(patch_zip_path):,} bytes, sha256: {patch_sha256}")

for vp in [os.path.join(root, "version.json"), os.path.join(root, "version-2.0-rc.json"), os.path.join(r"C:\AVRoad_2026", "version.json")]:
    with open(vp, "w", encoding="utf-8") as f:
        json.dump(version_meta, f, ensure_ascii=False, indent=2)

if os.path.exists(desk_test):
    shutil.copy2(patch_zip_path, os.path.join(desk_test, "patch_2.1_rc2.zip"))
    shutil.copy2(os.path.join(root, "version.json"), os.path.join(desk_test, "version.json"))

# 3. Update payload_rc2.zip
print("\n[2/5] Updating payload_rc2.zip with latest binaries, UI21, Security and Tools...")
payload_zip = os.path.join(root, "payload_rc2.zip")
temp_payload = os.path.join(root, "payload_temp.zip")

replace_files = {
    "AVRoad.exe": os.path.join(root, "AVRoad.exe"),
    "AVRoad.Deployment.dll": os.path.join(root, "AVRoad.Deployment.dll"),
    "AVUpdater.v2.exe": os.path.join(root, "AVUpdater.v2.exe"),
    "AVUpdater.exe": os.path.join(root, "AVUpdater.exe"),
    "Uninstall.exe": os.path.join(root, "Uninstall.exe"),
    "GetKey.exe": os.path.join(root, "GetKey.exe"),
    "AVLicense.exe": os.path.join(root, "AVLicense.exe"),
    "version.json": os.path.join(root, "version.json"),
    "Profile.aws": os.path.join(root, "Profile.aws"),
    "UI21/AVRoad.SinglePanel21.dll": os.path.join(ui21, "AVRoad.SinglePanel21.dll"),
    "UI21/commands.xml": os.path.join(ui21, "commands.xml"),
    "Support/acad2007doc.lsp": os.path.join(root, "Support", "acad2007doc.lsp"),
    "Support/Tools/MPL_inModelLayout.vlx": os.path.join(tools, "MPL_inModelLayout.vlx"),
    "Support/Tools/convertfont.vlx": os.path.join(tools, "convertfont.vlx"),
    "drv/AdskHwCertificationDatabase.xml": drv_xml
}

replaced = set()
with zipfile.ZipFile(payload_zip, "r") as zin, zipfile.ZipFile(temp_payload, "w", zipfile.ZIP_DEFLATED) as zout:
    for item in zin.infolist():
        norm_name = item.filename.replace("\\", "/")
        matching_key = None
        for k in replace_files:
            if k.lower() == norm_name.lower():
                matching_key = k
                break
        if matching_key:
            zout.write(replace_files[matching_key], item.filename)
            replaced.add(matching_key)
        else:
            zout.writestr(item, zin.read(item.filename))
    for k, src_path in replace_files.items():
        if k not in replaced and os.path.exists(src_path):
            zout.write(src_path, k)

shutil.move(temp_payload, payload_zip)
print(f"  payload_rc2.zip updated successfully: {os.path.getsize(payload_zip):,} bytes")

# 4. Compile Setup_AVRoad_2.2.2.exe
print("\n[3/5] Compiling Setup_AVRoad_2.2.2.exe installer...")
setup_desk_target = os.path.join(desk_root, "Setup_AVRoad_2.2.2.exe")
setup_desk_test = os.path.join(desk_test, "Setup_AVRoad_2.2.2.exe") if os.path.exists(desk_test) else None
setup_root = os.path.join(root, "Setup_AVRoad_2026.exe")

manifest_arg = f"/win32manifest:{os.path.join(root, 'setup.manifest')}" if os.path.exists(os.path.join(root, 'setup.manifest')) else ""

cmd_setup = [
    csc4, "/nologo", "/target:winexe", "/platform:x86",
    f"/out:{setup_desk_target}",
    f"/win32icon:{os.path.join(root, 'AVRoad.ico')}",
    "/r:System.dll", "/r:System.Core.dll", "/r:System.Xml.dll",
    "/r:System.Drawing.dll", "/r:System.Windows.Forms.dll",
    "/r:System.IO.Compression.dll", "/r:System.IO.Compression.FileSystem.dll",
    f"/resource:{payload_zip},payload.zip",
    os.path.join(root, "Setup_AVRoad_2026.cs"),
    os.path.join(root, "CADRegistryConfig.cs"),
    os.path.join(root, "Cad7ComRegistration.cs"),
    os.path.join(root, "PatchTransaction.cs")
]
if manifest_arg:
    cmd_setup.insert(4, manifest_arg)

subprocess.check_call(cmd_setup)
print(f"  Setup_AVRoad_2.2.2.exe compiled on Desktop: {os.path.getsize(setup_desk_target):,} bytes")

if setup_desk_test:
    shutil.copy2(setup_desk_target, setup_desk_test)
    shutil.copy2(setup_desk_target, os.path.join(desk_test, "Setup_AVRoad_2.2.1.exe"))

shutil.copy2(setup_desk_target, setup_root)

# 5. Generate SHA256SUMS.txt
print("\n[4/5] Generating SHA256SUMS.txt...")
sums_lines = []
targets = [
    ("Setup_AVRoad_2.2.2.exe", setup_desk_target),
    ("patch_2.1_rc2.zip", patch_zip_path),
    ("Fix_CleanPC_CAD7.bat", os.path.join(root, "Fix_CleanPC_CAD7.bat")),
    ("version.json", os.path.join(root, "version.json"))
]

for name, path in targets:
    if os.path.exists(path):
        with open(path, "rb") as f:
            h = hashlib.sha256(f.read()).hexdigest().upper()
        sums_lines.append(f"{h}  {name}")

sums_content = "\n".join(sums_lines) + "\n"
with open(os.path.join(root, "SHA256SUMS.txt"), "w", encoding="utf-8") as f:
    f.write(sums_content)

if os.path.exists(desk_test):
    with open(os.path.join(desk_test, "SHA256SUMS.txt"), "w", encoding="utf-8") as f:
        f.write(sums_content)

print(sums_content)

print("\n====================================================================")
print("  AVROAD 2.2.2 WITH STRICT LICENSE ENFORCEMENT PACKAGED SUCCESSFULLY!")
print("====================================================================")
