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
    'acopm.arx',
    'Acpi.arx',
    'AcOpmExt.arx',
    'oleaprot.arx',
    'AcDmPropertyEditors.arx',
    'AcDblClkEdit.arx',
    'AcDblClkEditPE.arx',
    'AcObjClassImp.arx',
    'AcFdUi.arx',
    'acrender.arx',
    'acetutil.arx',
    'AcLayer.dll',
    'AcLayerTools.dll',
    'TDTObjectsWrapper.arx',
    'TDTObjects17.dbx',
    'AlignmentDb17.dbx',
    'AVSetting.exe',
    'AVUpdater.exe'
]

app_dir = r'C:\AVRoad_2026'

target_zips = [
    r'C:\TDT_RESEARCH\patch_1.8.zip',
    r'C:\TDT_RESEARCH\patch_1.7.zip',
    r'C:\TDT_RESEARCH\patch_1.6.zip',
    r'C:\TDT_RESEARCH\patch_1.5.zip',
    r'C:\TDT_RESEARCH\patch_1.4.zip',
    r'C:\TDT_RESEARCH\patch_1.3.zip',
    r'C:\TDT_RESEARCH\patch_1.2.zip'
]

for target_zip in target_zips:
    with zipfile.ZipFile(target_zip, 'w', compression=zipfile.ZIP_DEFLATED) as z:
        for f in files_to_pack:
            src = os.path.join(app_dir, f)
            if os.path.exists(src):
                z.write(src, arcname=f)
            else:
                print(f'WARNING: {f} not found!')
    print(f'Created {os.path.basename(target_zip)} ({os.path.getsize(target_zip):,} bytes)')

print('All patch packages created successfully!')
