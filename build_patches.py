import os, zipfile

app_dir = r'C:\AVRoad_2026'
research_dir = r'c:\TDT_RESEARCH'

files_map = {
    'AVRoad.exe': os.path.join(research_dir, 'AVRoad.exe'),
    'acad.exe': os.path.join(research_dir, 'acad_clean.exe'),
    'FixedProfile.aws': os.path.join(research_dir, 'FixedProfile.aws'),
    'version.json': os.path.join(research_dir, 'version.json'),
    'startup_bg_clean.png': os.path.join(app_dir, 'startup_bg_clean.png'),
    'startup_bg.png': os.path.join(app_dir, 'startup_bg.png'),
    'AVRoad_Profile.arg': os.path.join(app_dir, 'AVRoad_Profile.arg'),
    'acad.rx': os.path.join(app_dir, 'acad.rx'),
    'adui17res.dll': os.path.join(app_dir, 'adui17res.dll'),
    'Autodesk.AutoCAD.Interop.dll': os.path.join(app_dir, 'Autodesk.AutoCAD.Interop.dll'),
    'Autodesk.AutoCAD.Interop.Common.dll': os.path.join(app_dir, 'Autodesk.AutoCAD.Interop.Common.dll'),
    'AcLayer.resources.dll': os.path.join(app_dir, 'AcLayer.resources.dll'),
    'AcLayerTools.resources.dll': os.path.join(app_dir, 'AcLayerTools.resources.dll'),
    'AcCustomize.resources.dll': os.path.join(app_dir, 'AcCustomize.resources.dll'),
    'AcCalcUi.resources.dll': os.path.join(app_dir, 'AcCalcUi.resources.dll'),
    'gdiplus.dll': os.path.join(app_dir, 'gdiplus.dll'),
    'KSVN.cui': os.path.join(app_dir, 'KSVN.cui'),
    'VNROAD.cui': os.path.join(app_dir, 'VNROAD.cui'),
    'SANNEN.cui': os.path.join(app_dir, 'SANNEN.cui'),
    'KVN.cui': os.path.join(app_dir, 'KVN.cui'),
    'DCCT.cui': os.path.join(app_dir, 'DCCT.cui'),
    'TDT3D.cui': os.path.join(app_dir, 'TDT3D.cui'),
    'RM.cui': os.path.join(app_dir, 'RM.cui'),
    'acopm.arx': os.path.join(app_dir, 'acopm.arx'),
    'Acpi.arx': os.path.join(app_dir, 'Acpi.arx'),
    'AcOpmExt.arx': os.path.join(app_dir, 'AcOpmExt.arx'),
    'oleaprot.arx': os.path.join(app_dir, 'oleaprot.arx'),
    'AcDmPropertyEditors.arx': os.path.join(app_dir, 'AcDmPropertyEditors.arx'),
    'AcDblClkEdit.arx': os.path.join(app_dir, 'AcDblClkEdit.arx'),
    'AcDblClkEditPE.arx': os.path.join(app_dir, 'AcDblClkEditPE.arx'),
    'AcObjClassImp.arx': os.path.join(app_dir, 'AcObjClassImp.arx'),
    'AcFdUi.arx': os.path.join(app_dir, 'AcFdUi.arx'),
    'acrender.arx': os.path.join(app_dir, 'acrender.arx'),
    'acetutil.arx': os.path.join(app_dir, 'acetutil.arx'),
    'AcLayer.dll': os.path.join(app_dir, 'AcLayer.dll'),
    'AcLayerTools.dll': os.path.join(app_dir, 'AcLayerTools.dll'),
    'TDTObjectsWrapper.arx': os.path.join(app_dir, 'TDTObjectsWrapper.arx'),
    'TDTObjects17.dbx': os.path.join(app_dir, 'TDTObjects17.dbx'),
    'AlignmentDb17.dbx': os.path.join(app_dir, 'AlignmentDb17.dbx'),
    'AVSetting.exe': os.path.join(app_dir, 'AVSetting.exe'),
    'AVUpdater.exe': os.path.join(app_dir, 'AVUpdater.exe')
}

target_zips = [
    os.path.join(research_dir, 'patch_1.9.zip'),
    os.path.join(research_dir, 'patch_1.8.zip'),
    os.path.join(research_dir, 'patch_1.7.zip'),
    os.path.join(research_dir, 'patch_1.6.zip'),
    os.path.join(research_dir, 'patch_1.5.zip'),
    os.path.join(research_dir, 'patch_1.4.zip'),
    os.path.join(research_dir, 'patch_1.3.zip'),
    os.path.join(research_dir, 'patch_1.2.zip')
]

for target_zip in target_zips:
    with zipfile.ZipFile(target_zip, 'w', compression=zipfile.ZIP_DEFLATED) as z:
        for arcname, src in files_map.items():
            if os.path.exists(src):
                z.write(src, arcname=arcname)
            else:
                print(f'WARNING: {src} not found!')
    print(f'Created {os.path.basename(target_zip)} ({os.path.getsize(target_zip):,} bytes)')

print('All patch packages created successfully!')
