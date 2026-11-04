with open(r"D:\cracks.rar-20240317T150045Z-001\cracks\autocad2007\Bin\acadFeui\acad.msi", "rb") as f:
    data = f.read()

start = 1555000
end = 1558000
chunk = data[start:end]

import re
# Look for CLSID or ProgID patterns
strings = re.findall(b'[\x20-\x7e]{4,}', chunk)
for s in strings:
    text = s.decode('ascii', errors='ignore')
    if any(k in text for k in ['CLSID', 'AutoCAD.Application', 'LocalServer32', 'TypeLib', 'ProgID']):
        print(text)
