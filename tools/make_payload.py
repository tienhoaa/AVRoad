import os, zipfile

app_dir = r'C:\AVRoad_2026'
out_zip = r'C:\TDT_RESEARCH\payload.zip'

if os.path.exists(out_zip):
    try: os.remove(out_zip)
    except: pass

with zipfile.ZipFile(out_zip, 'w', compression=zipfile.ZIP_DEFLATED, compresslevel=6) as z:
    for root, dirs, files in os.walk(app_dir):
        for f in files:
            if f.endswith(('.bak', '.tmp', '.log', '.scr')):
                continue
            full_path = os.path.join(root, f)
            rel_path = os.path.relpath(full_path, app_dir)
            z.write(full_path, arcname=rel_path)

sz = os.path.getsize(out_zip) / (1024 * 1024)
print(f'Created {out_zip} size: {sz:.2f} MB')
