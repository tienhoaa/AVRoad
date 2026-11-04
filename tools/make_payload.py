import os, zipfile

app_dir = r'C:\AVRoad_2026'
out_zip = r'C:\TDT_RESEARCH\payload.zip'

if os.path.exists(out_zip):
    try: os.remove(out_zip)
    except: pass

exclude_dirs = {'backup_vc80', 'locked', '.git'}
exclude_files = {'acad_backup_branded.exe', 'avroad.old.exe'}

with zipfile.ZipFile(out_zip, 'w', compression=zipfile.ZIP_DEFLATED, compresslevel=6) as z:
    for root, dirs, files in os.walk(app_dir):
        # Prune excluded directories in-place
        dirs[:] = [d for d in dirs if d.lower() not in exclude_dirs]
        for f in files:
            fl = f.lower()
            if fl in exclude_files or fl.endswith(('.bak', '.tmp', '.log', '.scr', '.old.exe')):
                continue
            full_path = os.path.join(root, f)
            rel_path = os.path.relpath(full_path, app_dir)
            z.write(full_path, arcname=rel_path)

sz = os.path.getsize(out_zip) / (1024 * 1024)
print(f'Created {out_zip} size: {sz:.2f} MB')
