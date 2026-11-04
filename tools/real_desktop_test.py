import subprocess
import time
import ctypes
from ctypes import wintypes
from PIL import Image

user32 = ctypes.windll.user32
gdi32 = ctypes.windll.gdi32

# Start AutoCAD
proc = subprocess.Popen([r"C:\AVRoad_2026\acad.exe", "/p", "AVRoad"])
print("Launched acad PID:", proc.pid)

# Wait 10 seconds for AutoCAD to be completely loaded
time.sleep(10)

hwnd_acad = None
def cb(hwnd, extra):
    global hwnd_acad
    length = user32.GetWindowTextLengthW(hwnd)
    if length > 0:
        buf = ctypes.create_unicode_buffer(length + 1)
        user32.GetWindowTextW(hwnd, buf, length + 1)
        if "AutoCAD 2007" in buf.value or "AVRoad" in buf.value:
            hwnd_acad = hwnd
            return False
    return True

WNDENUMPROC = ctypes.WINFUNCTYPE(ctypes.c_bool, wintypes.HWND, ctypes.c_void_p)
user32.EnumWindows(WNDENUMPROC(cb), 0)

if not hwnd_acad:
    print("Could not find AutoCAD window")
    proc.terminate()
    exit(1)

print("Found AutoCAD HWND:", hwnd_acad)
user32.SetForegroundWindow(hwnd_acad)
time.sleep(1)

# Function to post key presses or string to AutoCAD
# We can use SendMessage or PostMessage WM_CHAR
WM_CHAR = 0x0102
WM_KEYDOWN = 0x0100
WM_KEYUP = 0x0101
VK_RETURN = 0x0D

def send_cmd(cmd_str):
    for ch in cmd_str:
        user32.PostMessageW(hwnd_acad, WM_CHAR, ord(ch), 0)
        time.sleep(0.02)
    user32.PostMessageW(hwnd_acad, WM_KEYDOWN, VK_RETURN, 0)
    user32.PostMessageW(hwnd_acad, WM_KEYUP, VK_RETURN, 0)
    time.sleep(0.5)

# Send commands:
# 1. Draw a line
send_cmd("line 100,100 500,500  ")
time.sleep(1)
# 2. Select the line
send_cmd('(sssetfirst nil (ssget "L"))')
time.sleep(1)
# 3. Open properties
send_cmd("_properties")
time.sleep(3)

# Capture desktop
hwnd_dt = user32.GetDesktopWindow()
rect = wintypes.RECT()
user32.GetWindowRect(hwnd_dt, ctypes.byref(rect))
w, h = rect.right - rect.left, rect.bottom - rect.top

hwndDC = user32.GetWindowDC(hwnd_dt)
mfcDC = gdi32.CreateCompatibleDC(hwndDC)
saveBitMap = gdi32.CreateCompatibleBitmap(hwndDC, w, h)
gdi32.SelectObject(mfcDC, saveBitMap)

# BitBlt desktop
SRCCOPY = 0x00CC0020
gdi32.BitBlt(mfcDC, 0, 0, w, h, hwndDC, 0, 0, SRCCOPY)

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
im.save(r'C:\Users\PC\.gemini\antigravity\brain\4bae2500-d3e0-440e-a0f3-6b918cb91ce7\real_desktop_test.png')
print("Captured real desktop successfully!")

proc.terminate()
time.sleep(1)
if proc.poll() is None:
    proc.kill()
