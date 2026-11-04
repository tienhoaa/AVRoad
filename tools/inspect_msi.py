import win32com.client

installer = win32com.client.Dispatch('WindowsInstaller.Installer')
db = installer.OpenDatabase(r'D:\cracks.rar-20240317T150045Z-001\cracks\autocad2007\Bin\acadFeui\acad.msi', 0)

print("--- Querying TypeLib Table ---")
try:
    view = db.OpenView("SELECT `LibID`, `Language`, `Directory_`, `Description`, `HelpPath`, `Cost` FROM `TypeLib`")
    view.Execute()
    while True:
        rec = view.Fetch()
        if not rec: break
        print("TypeLib:", rec.StringData(1), rec.StringData(4))
except Exception as e:
    print("TypeLib error:", e)

print("\n--- Querying Class Table ---")
try:
    view = db.OpenView("SELECT `CLSID`, `Context`, `Component_`, `ProgId_Default`, `Description` FROM `Class`")
    view.Execute()
    count = 0
    while True:
        rec = view.Fetch()
        if not rec: break
        print("Class:", rec.StringData(1), rec.StringData(4), rec.StringData(5))
        count += 1
        if count > 30:
            print("... more classes")
            break
except Exception as e:
    print("Class error:", e)
