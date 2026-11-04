import winreg
import os

app_dir = r"C:\AVRoad_2026"
acad_exe = os.path.join(app_dir, "acad.exe")

def set_val(root, subkey, name, val):
    with winreg.CreateKey(root, subkey) as k:
        winreg.SetValueEx(k, name, 0, winreg.REG_SZ, val)

classes = winreg.HKEY_CURRENT_USER
base = r"Software\Classes"

# 1. AutoCAD.Application.17
set_val(classes, rf"{base}\AutoCAD.Application.17", "", "AutoCAD Application 17")
set_val(classes, rf"{base}\AutoCAD.Application.17\CLSID", "", "{28B7AA99-C0F9-4C47-995E-8A8D729603A1}")

# 2. AutoCAD.Application
set_val(classes, rf"{base}\AutoCAD.Application", "", "AutoCAD Application")
set_val(classes, rf"{base}\AutoCAD.Application\CLSID", "", "{28B7AA99-C0F9-4C47-995E-8A8D729603A1}")
set_val(classes, rf"{base}\AutoCAD.Application\CurVer", "", "AutoCAD.Application.17")

# 3. CLSID AcadApplication
clsid_app = rf"{base}\CLSID\{{28B7AA99-C0F9-4C47-995E-8A8D729603A1}}"
set_val(classes, clsid_app, "", "AcadApplication")
set_val(classes, rf"{clsid_app}\ProgID", "", "AutoCAD.Application.17")
set_val(classes, rf"{clsid_app}\VersionIndependentProgID", "", "AutoCAD.Application")
set_val(classes, rf"{clsid_app}\Programmable", "", "")
set_val(classes, rf"{clsid_app}\TypeLib", "", "{4B0F75B4-A785-4523-8B35-C50ACDEE89FF}")
set_val(classes, rf"{clsid_app}\LocalServer32", "", f'"{acad_exe}" /Automation')

# 4. AutoCAD.Drawing.17
set_val(classes, rf"{base}\AutoCAD.Drawing.17", "", "AutoCAD Drawing 17")
set_val(classes, rf"{base}\AutoCAD.Drawing.17\CLSID", "", "{7AABBB95-79BE-4C0F-8024-EB6AF271231C}")

# 5. AutoCAD.Drawing
set_val(classes, rf"{base}\AutoCAD.Drawing", "", "AutoCAD Drawing")
set_val(classes, rf"{base}\AutoCAD.Drawing\CLSID", "", "{7AABBB95-79BE-4C0F-8024-EB6AF271231C}")
set_val(classes, rf"{base}\AutoCAD.Drawing\CurVer", "", "AutoCAD.Drawing.17")

# 6. CLSID AcadDocument
clsid_doc = rf"{base}\CLSID\{{7AABBB95-79BE-4C0F-8024-EB6AF271231C}}"
set_val(classes, clsid_doc, "", "AcadDocument")
set_val(classes, rf"{clsid_doc}\ProgID", "", "AutoCAD.Drawing.17")
set_val(classes, rf"{clsid_doc}\VersionIndependentProgID", "", "AutoCAD.Drawing")
set_val(classes, rf"{clsid_doc}\Programmable", "", "")
set_val(classes, rf"{clsid_doc}\TypeLib", "", "{4B0F75B4-A785-4523-8B35-C50ACDEE89FF}")
set_val(classes, rf"{clsid_doc}\LocalServer32", "", f'"{acad_exe}" /Automation')

# 7. Interfaces
# _DAcadApplicationEvents: {22C8E547-0D27-4793-BB9F-AC58D4F8E794}
if_app = rf"{base}\Interface\{{22C8E547-0D27-4793-BB9F-AC58D4F8E794}}"
set_val(classes, if_app, "", "_DAcadApplicationEvents")
set_val(classes, rf"{if_app}\TypeLib", "", "{4B0F75B4-A785-4523-8B35-C50ACDEE89FF}")

# _DAcadDocumentEvents: {C814C69D-A74B-494C-8EE6-23731B145CCB}
if_doc = rf"{base}\Interface\{{C814C69D-A74B-494C-8EE6-23731B145CCB}}"
set_val(classes, if_doc, "", "_DAcadDocumentEvents")
set_val(classes, rf"{if_doc}\TypeLib", "", "{4B0F75B4-A785-4523-8B35-C50ACDEE89FF}")

print("COM classes registered successfully in HKCU!")
