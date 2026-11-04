import subprocess
import time
import ctypes
from ctypes import wintypes
from PIL import Image

user32 = ctypes.windll.user32
gdi32 = ctypes.windll.gdi32

scr_path = r'c:\TDT_RESEARCH\test_open_prop.scr'
with open(scr_path, 'w', encoding='ascii') as f:
    f.write('_.zoom _e\n')
    f.write('(sssetfirst nil (ssget "X"))\n')
    f.write('_.properties\n')

proc = subprocess.Popen([r'C:\AVRoad_2026\acad.exe', r'C:\AVRoad_2026\DauCo.dwg', '/p', 'AVRoad', '/b', scr_path])
print("AutoCAD started PID:", proc.pid)
time.sleep(14)

hwnd_target = None
def cb(hwnd, extra):
    global hwnd_target
    length = user32.GetWindowTextLengthW(hwnd)
    if length > 0:
        buf = ctypes.create_unicode_buffer(length + 1)
        user32.GetWindowTextW(hwnd, buf, length + 1)
        if 'AutoCAD 2007' in buf.value or 'AVRoad' in buf.value or 'DauCo.dwg' in buf.value:
            hwnd_target = hwnd
            return False
    return True

WNDENUMPROC = ctypes.WINFUNCTYPE(ctypes.c_bool, wintypes.HWND, ctypes.c_void_p)
user32.EnumWindows(WNDENUMPROC(cb), 0)

if hwnd_target:
    print('Found window:', hwnd_target)
    rect = wintypes.RECT()
    user32.GetWindowRect(hwnd_target, ctypes.byref(rect))
    w, h = rect.right - rect.left, rect.bottom - rect.top
    hwndDC = user32.GetWindowDC(hwnd_target)
    mfcDC = gdi32.CreateCompatibleDC(hwndDC)
    saveBitMap = gdi32.CreateCompatibleBitmap(hwndDC, w, h)
    gdi32.SelectObject(mfcDC, saveBitMap)
    user32.PrintWindow(hwnd_target, mfcDC, 0)
    
    class BITMAPINFOHEADER(ctypes.Structure):
        _fields_ = [
            ('biSize', wintypes.DWORD), ('biWidth', wintypes.LONG), ('biHeight', wintypes.LONG),
            ('biPlanes', wintypes.WORD), ('biBitCount', wintypes.WORD), ('biCompression', wintypes.DWORD),
            ('biSizeImage', wintypes.DWORD), ('biXPelsPerMeter', wintypes.LONG), ('biYPelsPerMeter', wintypes.LONG),
            ('biClrUsed', wintypes.DWORD), ('biClrImportant', wintypes.DWORD)
        ]
    class BITMAPINFO(ctypes.Structure):
        _fields_ = [('bmiHeader', BITMAPINFOHEADER), ('bmiColors', wintypes.DWORD * 3)]
        
    bmi = BITMAPINFO()
    bmi.bmiHeader.biSize = ctypes.sizeof(BITMAPINFOHEADER)
    bmi.bmiHeader.biWidth = w
    bmi.bmiHeader.biHeight = -h
    bmi.bmiHeader.biPlanes = 1
    bmi.bmiHeader.biBitCount = 32
    buf = ctypes.create_string_buffer(w * h * 4)
    gdi32.GetDIBits(mfcDC, saveBitMap, 0, h, buf, ctypes.byref(bmi), 0)
    im = Image.frombuffer('RGBA', (w, h), buf, 'raw', 'BGRA', 0, 1)
    im.resize((1288, 704)).save(r'C:\Users\PC\.gemini\antigravity\brain\4bae2500-d3e0-440e-a0f3-6b918cb91ce7\cad_prop_live2.png')
    print('Captured live screenshot successfully!')
else:
    print('Window not found')

proc.terminate()
time.sleep(2)
if proc.poll() is None:
    proc.kill()
