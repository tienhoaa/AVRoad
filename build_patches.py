import os, zipfile

files_to_pack = [
    'AVRoad.exe',
    'startup_bg_clean.png',
    'startup_bg.png',
    'ACAD.lsp',
    'AVRoad_Profile.arg',
    'acad.rx',
    'version.json',
    'KSVN.cui',
    'VNROAD.cui',
    'SANNEN.cui',
    'KVN.cui',
    'DCCT.cui',
    'TDT3D.cui',
    'RM.cui',
    'acrender.arx',
    'TDTObjectsWrapper.arx',
    'TDTObjects17.dbx',
    'AlignmentDb17.dbx',
    'AVSetting.exe',
    'AVUpdater.exe'
]

app_dir = r'C:\AVRoad_2026'

for target_zip in [r'C:\TDT_RESEARCH\patch_1.4.zip', r'C:\TDT_RESEARCH\patch_1.3.zip', r'C:\TDT_RESEARCH\patch_1.2.zip']:
    with zipfile.ZipFile(target_zip, 'w', compression=zipfile.ZIP_DEFLATED) as z:
        for f in files_to_pack:
            src = os.path.join(app_dir, f)
            if os.path.exists(src):
                z.write(src, arcname=f)
                print(f'Packed {f} into {os.path.basename(target_zip)}')
            else:
                print(f'WARNING: {f} not found!')

print('Patch packages created successfully!')
