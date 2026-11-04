import pefile

pe = pefile.PE(r"C:\AVRoad_2026\acad.exe")
sec = pe.sections[0] # .text
data = sec.get_data()
vaddr = sec.VirtualAddress
image_base = pe.OPTIONAL_HEADER.ImageBase
target_va = 0x4048b0

callers = []
for i in range(len(data) - 4):
    if data[i] == 0xe8:
        rel = int.from_bytes(data[i+1:i+5], byteorder='little', signed=True)
        call_va = image_base + vaddr + i
        next_va = call_va + 5
        if next_va + rel == target_va:
            callers.append(hex(call_va))

print(f"Total callers of wcscpy: {len(callers)}")
for c in callers:
    print(c)
