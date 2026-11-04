import subprocess
import time
import os
import glob

# Create AutoCAD test script
scr_content = """(setvar "LOGFILEMODE" 1)
(princ "\\n>>> STARTING VERIFICATION SCRIPT <<<\\n")
(vl-load-com)
(princ "\\n>>> VL-LOAD-COM SUCCESS <<<\\n")
(setq acadApp (vlax-get-acad-object))
(princ (strcat "\\n>>> ACAD APP: " (vl-prin1-to-string acadApp) " <<<\\n"))
(setq doc (vla-get-ActiveDocument acadApp))
(princ (strcat "\\n>>> ACTIVE DOC: " (vl-prin1-to-string doc) " <<<\\n"))
(setq ms (vla-get-ModelSpace doc))
(setq pt1 (vlax-3d-point '(0.0 0.0 0.0)))
(setq pt2 (vlax-3d-point '(100.0 100.0 0.0)))
(setq myline (vla-AddLine ms pt1 pt2))
(princ (strcat "\\n>>> CREATED LINE: " (vl-prin1-to-string myline) " <<<\\n"))
(princ (strcat "\\n>>> LINE LAYER: " (vla-get-Layer myline) " <<<\\n"))
(princ (strcat "\\n>>> LINE COLOR: " (vl-prin1-to-string (vla-get-Color myline)) " <<<\\n"))
(princ (strcat "\\n>>> LINE LENGTH: " (vl-prin1-to-string (vla-get-Length myline)) " <<<\\n"))
(sssetfirst nil (ssget "L"))
_properties
(princ "\\n>>> PROPERTIES COMMAND EXECUTED <<<\\n")
_quit
_y
"""

scr_path = r"c:\TDT_RESEARCH\test_com_prop.scr"
with open(scr_path, "w", encoding="ascii") as f:
    f.write(scr_content)

print(f"Created script at {scr_path}")

# Run AutoCAD
cmd = [r"C:\AVRoad_2026\acad.exe", "/p", "AVRoad", "/b", scr_path]
proc = subprocess.Popen(cmd)
print(f"Started AutoCAD PID={proc.pid}, waiting for it to finish...")

# Wait up to 15 seconds
for i in range(15):
    time.sleep(1)
    if proc.poll() is not None:
        print(f"AutoCAD exited cleanly with code {proc.returncode}")
        break
else:
    print("AutoCAD still running, terminating...")
    proc.terminate()
    time.sleep(2)
    if proc.poll() is None:
        proc.kill()

# Check log file
log_dir = os.path.expandvars(r"%LOCALAPPDATA%\Autodesk\AutoCAD 2007\R17.0\enu")
logs = glob.glob(os.path.join(log_dir, "*.log"))
if logs:
    latest_log = max(logs, key=os.path.getmtime)
    print(f"\n--- LATEST LOG: {latest_log} ---")
    with open(latest_log, "r", encoding="utf-8", errors="ignore") as f:
        print(f.read()[-3000:])
else:
    print("No log file found.")
