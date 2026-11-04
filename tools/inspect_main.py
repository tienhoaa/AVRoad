with open('AVRoad_Launcher.cs', 'r', encoding='utf-8') as f:
    lines = f.readlines()

main_idx = -1
for i, l in enumerate(lines):
    if 'static void Main(string[] args)' in l:
        main_idx = i
        break

print('Main is at line', main_idx + 1)
for j in range(main_idx, main_idx + 35):
    print(f'{j+1}: {lines[j]}', end='')
