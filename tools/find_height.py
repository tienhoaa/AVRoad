with open(r'c:\TDT_RESEARCH\AVRoad_Launcher.cs', 'r', encoding='utf-8') as f:
    lines = f.readlines()
for i, l in enumerate(lines):
    if '500' in l:
        print(f'{i+1}: {l.strip()}')
        for j in range(max(0, i-4), min(len(lines), i+6)):
            print(f'  {j+1}: {lines[j].strip()}')
