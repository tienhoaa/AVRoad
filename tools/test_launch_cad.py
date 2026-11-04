import subprocess
import time
import os
import sys

# 1. Launch AutoCAD
cmd = [r"C:\AVRoad_2026\acad.exe", "/p", "AVRoad", "/b", r"c:\TDT_RESEARCH\test_properties.scr"]
proc = subprocess.Popen(cmd)
print(f"Launched acad.exe PID={proc.pid}")

time.sleep(8)

# 2. Take screenshot of screen
try:
    from PIL import ImageGrab
    im = ImageGrab.grab()
    screenshot_path = r"C:\Users\PC\.gemini\antigravity\brain\4bae2500-d3e0-440e-a0f3-6b918cb91ce7\cad_screenshot.png"
    im.save(screenshot_path)
    print(f"Saved screenshot to {screenshot_path}")
except Exception as e:
    print(f"Screenshot error: {e}")

# 3. Kill acad
proc.terminate()
time.sleep(2)
if proc.poll() is None:
    proc.kill()
print("Terminated acad.exe")
