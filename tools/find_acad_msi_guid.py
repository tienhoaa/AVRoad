with open(r"D:\cracks.rar-20240317T150045Z-001\cracks\autocad2007\Bin\acadFeui\acad.msi", "rb") as f:
    data = f.read()

pos = 0
while True:
    idx = data.find(b"28B7AA99", pos)
    if idx == -1: break
    print(f"\n--- MATCH AT {idx} ---")
    chunk = data[max(0, idx - 200):idx + 300]
    # replace non-printable chars
    clean = "".join(chr(b) if 32 <= b <= 126 else f"[{b:02x}]" for b in chunk)
    print(clean)
    pos = idx + 1
