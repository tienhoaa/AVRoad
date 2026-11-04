import subprocess
import time
import os
import ctypes
from ctypes import wintypes
from PIL import Image

user32 = ctypes.windll.user32
gdi32 = ctypes.windll.gdi32

class BITMAPINFOHEADER(ctypes.Structure):
    _fields_ = [
        ('biSize', wintypes.DWORD),
        ('biWidth', wintypes.LONG),
        ('biHeight', wintypes.LONG),
        ('biPlanes', wintypes.WORD),
        ('biBitCount', wintypes.WORD),
        ('biCompression', wintypes.DWORD),
        ('biSizeImage', wintypes.DWORD),
        ('biXPelsPerMeter', wintypes.LONG),
        ('biYPelsPerMeter', wintypes.LONG),
        ('biClrUsed', wintypes.DWORD),
        ('biClrImportant', wintypes.DWORD)
    ]

class BITMAPINFO(ctypes.Structure):
    _fields_ = [('bmiHeader', BITMAPINFOHEADER), ('bmiColors', wintypes.DWORD * 3)]

def capture_window(hwnd, save_path):
    rect = wintypes.RECT()
    user32.GetWindowRect(hwnd, ctypes.byref(rect))
    w = rect.right - rect.left
    h = rect.bottom - rect.top
    if w <= 0 or h <= 0:
        return False
    
    hwndDC = user32.GetWindowDC(hwnd)
    mfcDC = gdi32.CreateCompatibleDC(hwndDC)
    saveBitMap = gdi32.CreateCompatibleBitmap(hwndDC, w, h)
    gdi32.SelectObject(mfcDC, saveBitMap)
    
    res = user32.PrintWindow(hwnd, mfcDC, 2)
    if not res:
        res = user32.PrintWindow(hwnd, mfcDC, 0)
    
    bmi = BITMAPINFO()
    bmi.bmiHeader.biSize = ctypes.sizeof(BITMAPINFOHEADER)
    bmi.bmiHeader.biWidth = w
    bmi.bmiHeader.biHeight = -h
    bmi.bmiHeader.biPlanes = 1
    bmi.bmiHeader.biBitCount = 32
    bmi.bmiHeader.biCompression = 0

    buf = ctypes.create_string_buffer(w * h * 4)
    gdi32.GetDIBits(mfcDC, saveBitMap, 0, h, buf, ctypes.byref(bmi), 0)

    gdi32.DeleteObject(saveBitMap)
    gdi32.DeleteDC(mfcDC)
    user32.ReleaseDC(hwnd, hwndDC)

    im = Image.frombuffer('RGBA', (w, h), buf, 'raw', 'BGRA', 0, 1)
    im.save(save_path)
    return True

# 1. Create SCR script that draws a circle, selects it, and opens properties
scr_content = """(setvar "CMDECHO" 1)
_circle 100,100 50
(sssetfirst nil (ssget "L"))
_properties
"""

scr_path = r"c:\TDT_RESEARCH\test_circle_prop.scr"
with open(scr_path, "w", encoding="ascii") as f:
    f.write(scr_content)

# 2. Launch AutoCAD
cmd = [r"C:\AVRoad_2026\acad.exe", "/p", "AVRoad", "/b", scr_path]
proc = subprocess.Popen(cmd)
print(f"Launched acad.exe PID={proc.pid}")

# 3. Wait for AutoCAD window
hwnd_acad = None
for _ in range(20):
    time.sleep(1)
    # Find window by process ID
    def enum_windows_cb(hwnd, extra):
        pid = wintypes.DWORD()
        user32.GetWindowThreadProcessId(hwnd, ctypes.byref(pid))
        if pid.value == proc.pid:
            length = user32.GetWindowTextLengthW(hwnd)
            buf = ctypes.create_unicode_buffer(length + 1)
            user32.GetWindowTextW(hwnd, buf, length + 1)
            title = buf.value
            if "AutoCAD" in title or "AVRoad" in title:
                extra.append((hwnd, title))
        return True

    windows = []
    WNDENUMPROC = ctypes.WINFUNCTYPE(ctypes.c_bool, wintypes.HWND, ctypes.c_void_p)
    # Use standard ctypes enum
    enum_cb = ctypes.WINFUNCTYPE(wintypes.BOOL, wintypes.HWND, ctypes.POINTER(ctypes.c_void_p))(lambda h, p: enum_windows_cb(h, windows))
    user32.EnumWindows(enum_cb, 0)

    if windows:
        hwnd_acad = windows[0][0]
        print(f"Found AutoCAD window: {windows[0][1]} HWND={hwnd_acad}")
        break

if not hwnd_acad:
    print("Could not find AutoCAD window by title, using GetForegroundWindow or searching all windows")
    hwnd_acad = user32.GetForegroundWindow()

# Wait 5 seconds for script execution and properties palette rendering
time.sleep(6)

# 4. Capture screenshot
out_path = r"C:\Users\PC\.gemini\antigravity\brain\4bae2500-d3e0-440e-a0f3-6b918cb91ce7\cad_verified_properties.png"
if capture_window(hwnd_acad, out_path):
    print(f"Captured screenshot to: {out_path}")
else:
    print("Capture window failed")

# 5. Clean up
proc.terminate()
time.sleep(2)
if proc.poll() is None:
    proc.kill()
print("Test completed and AutoCAD closed.")
