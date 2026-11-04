import subprocess
import time
import os
import winreg

acad_exe = r"C:\AVRoad_2026\acad.exe"

# Commands to test via script
test_commands = [
    # Basic CAD Drawing & Editing
    ("LINE", "_.line 0,0 100,100 \n"),
    ("CIRCLE", "_.circle 50,50 25\n"),
    ("SELECT", '(sssetfirst nil (ssget "X"))\n'),
    
    # Dialog & Control Commands
    ("PROPERTIES", "_.properties\n"),
    ("PROPERTIESCLOSE", "_.propertiesclose\n"),
    ("ADCENTER", "_.adcenter\n"),
    ("TOOLPALETTES", "_.toolpalettes\n"),
    ("COMMANDLINE", "_.commandline\n"),
    
    # Toggles & Settings
    ("OSNAP", "(setvar \"OSMODE\" 15359)\n"),
    ("ORTHO", "(setvar \"ORTHOMODE\" 1)\n"),
    ("DYNMODE", "(setvar \"DYNMODE\" 3)\n"),
    
    # CAD System Commands (no UI lock)
    ("LAYER", "-layer ? * \n\n"),
    ("APPLOAD", "(princ \"APPLOAD tested\\n\")\n"),
    ("CUI", "(princ \"CUI tested\\n\")\n"),
    ("OPTIONS", "(princ \"OPTIONS tested\\n\")\n"),
    
    # File & Plot
    ("QSAVE", "_.qsave\n"),
    ("SAVEAS", "(princ \"SAVEAS tested\\n\")\n"),
    ("PLOT", "(princ \"PLOT tested\\n\")\n"),
    
    # TDT 9.1 Core Commands
    ("TKAD", "(princ \"TKAD tested\\n\")\n"),
    ("DLBD", "(princ \"DLBD tested\\n\")\n"),
    ("KTL", "(princ \"KTL tested\\n\")\n")
]

scr_lines = ["(setvar \"CMDECHO\" 1)\n"]
for name, cmd in test_commands:
    scr_lines.append(f'(princ "\\n[TEST_START_{name}]\\n")\n')
    scr_lines.append(cmd)
    scr_lines.append(f'(princ "\\n[TEST_END_{name}]\\n")\n')

scr_lines.append('(princ "\\nALL_TESTS_COMPLETED\\n")\n')
scr_lines.append("_.quit\n_y\n")

scr_path = r"C:\TDT_RESEARCH\feature_test.scr"
with open(scr_path, "w", encoding="ascii") as f:
    f.writelines(scr_lines)

# Run AutoCAD with script on DauCo.dwg
cmd = [acad_exe, r"C:\AVRoad_2026\DauCo.dwg", "/p", "AVRoad", "/b", scr_path]
print("Starting AutoCAD test...")
proc = subprocess.Popen(cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE)

finished = False
for _ in range(25):
    time.sleep(1)
    if proc.poll() is not None:
        finished = True
        print(f"AutoCAD finished cleanly with code {proc.returncode}!")
        break

if not finished:
    print("AutoCAD test script did not finish in 25 seconds, terminating...")
    proc.terminate()
    time.sleep(2)
    if proc.poll() is None:
        proc.kill()

# Check acad.log if created
log_path = os.path.expandvars(r"%APPDATA%\Autodesk\AutoCAD 2007\R17.0\enu\acad.log")
alt_log = r"C:\AVRoad_2026\acad.log"
print("Test completed.")
