import subprocess
import time
import os

# Create script to open properties and quit
scr_content = """(setvar "CMDECHO" 1)
_properties
(princ "\\nProperties opened\\n")
_quit
_y
"""

scr_path = r"c:\TDT_RESEARCH\test_open_prop.scr"
with open(scr_path, "w", encoding="ascii") as f:
    f.write(scr_content)

# Run AutoCAD
cmd = [r"C:\AVRoad_2026\acad.exe", "/p", "AVRoad", "/b", scr_path]
proc = subprocess.Popen(cmd)
print(f"Started AutoCAD PID={proc.pid}...")

for i in range(15):
    time.sleep(1)
    if proc.poll() is not None:
        print(f"AutoCAD finished with code {proc.returncode}")
        break
else:
    print("AutoCAD still running, terminating...")
    proc.terminate()
    time.sleep(2)
    if proc.poll() is None:
        proc.kill()

# Check Profile.aws
aws_path = os.path.expandvars(r"%APPDATA%\Autodesk\AutoCAD 2007\R17.0\enu\Support\Profiles\AVRoad\Profile.aws")
if os.path.exists(aws_path):
    print(f"\nProfile.aws size: {os.path.getsize(aws_path)}")
    with open(aws_path, "r", encoding="utf-8", errors="ignore") as f:
        text = f.read()
    import xml.etree.ElementTree as ET
    root = ET.fromstring(text)
    for elem in root.iter():
        if elem.attrib.get("Name") == "PROPERTIES":
            print("PROPERTIES node in Profile.aws:")
            print(ET.tostring(elem, encoding="utf-8").decode("utf-8"))
else:
    print("Profile.aws does not exist.")
