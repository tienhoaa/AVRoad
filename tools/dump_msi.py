import ctypes
from ctypes import wintypes

msi = ctypes.windll.msi

hDb = wintypes.HANDLE()
path = r"D:\cracks.rar-20240317T150045Z-001\cracks\autocad2007\Bin\acadFeui\acad.msi"
res = msi.MsiOpenDatabaseW(path, 0, ctypes.byref(hDb))
if res != 0:
    print("Failed to open MSI:", res)
    exit(1)

hView = wintypes.HANDLE()
sql = "SELECT `Root`, `Key`, `Name`, `Value` FROM `Registry`"
res = msi.MsiDatabaseOpenViewW(hDb, sql, ctypes.byref(hView))
if res != 0:
    print("Failed to open view:", res)
    exit(1)

msi.MsiViewExecute(hView, None)

hRec = wintypes.HANDLE()
buf = ctypes.create_unicode_buffer(1024)
buf_size = wintypes.DWORD(1024)

found = []
while msi.MsiViewFetch(hView, ctypes.byref(hRec)) == 0:
    def get_str(col):
        sz = wintypes.DWORD(1024)
        msi.MsiRecordGetStringW(hRec, col, buf, ctypes.byref(sz))
        return buf.value

    root = get_str(1)
    key = get_str(2)
    name = get_str(3)
    val = get_str(4)
    msi.MsiCloseHandle(hRec)

    if "Applications" in key or "acopm" in val.lower() or "properties" in val.lower():
        found.append((root, key, name, val))

msi.MsiCloseHandle(hView)
msi.MsiCloseHandle(hDb)

print(f"Total matching registry entries: {len(found)}")
for r in found:
    print(r)
