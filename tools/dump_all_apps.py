import ctypes
from ctypes import wintypes

msi = ctypes.windll.msi

hDb = wintypes.HANDLE()
path = r"D:\cracks.rar-20240317T150045Z-001\cracks\autocad2007\Bin\acadFeui\acad.msi"
msi.MsiOpenDatabaseW(path, 0, ctypes.byref(hDb))

hView = wintypes.HANDLE()
sql = "SELECT `Root`, `Key`, `Name`, `Value` FROM `Registry`"
msi.MsiDatabaseOpenViewW(hDb, sql, ctypes.byref(hView))
msi.MsiViewExecute(hView, None)

hRec = wintypes.HANDLE()
buf = ctypes.create_unicode_buffer(2048)

apps = {}
while msi.MsiViewFetch(hView, ctypes.byref(hRec)) == 0:
    def get_str(col):
        sz = wintypes.DWORD(2048)
        msi.MsiRecordGetStringW(hRec, col, buf, ctypes.byref(sz))
        return buf.value

    root = get_str(1)
    key = get_str(2)
    name = get_str(3)
    val = get_str(4)
    msi.MsiCloseHandle(hRec)

    if "\\Applications" in key:
        idx = key.find("\\Applications") + len("\\Applications")
        sub = key[idx:].strip("\\")
        parts = sub.split("\\")
        app = parts[0]
        if not app:
            continue
        sub_key = "\\".join(parts[1:])
        if app not in apps:
            apps[app] = {}
        entry_key = f"{sub_key}\\{name}" if sub_key else name
        apps[app][entry_key] = val

msi.MsiCloseHandle(hView)
msi.MsiCloseHandle(hDb)

print(f"Total Applications: {len(apps)}")
for app, items in sorted(apps.items()):
    loader = items.get("LOADER", "")
    ctrls = items.get("LOADCTRLS", "")
    desc = items.get("DESCRIPTION", "")
    cmds = [k for k in items if k.startswith("Commands\\")]
    print(f"{app:25} LOADER={loader:40} LOADCTRLS={ctrls:5} CMDS={len(cmds)}")
