with open('AVRoad_Launcher.cs', 'r', encoding='utf-8') as f:
    code = f.read()

# Replace prepare-only to declare vars first
old_block = '''            string appDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\\\');
            string iniPath = Path.Combine(appDir, \"AVRoad.ini\");

            foreach (string a in args)
            {
                if (a.Equals(\"/prepare-only\", StringComparison.OrdinalIgnoreCase))
                {
                    PrepareProfile(appDir, argProfile, fontPath, plotStylePath, plotterPath);
                    RegisterAutoCADApplications(appDir);
                    return;
                }'''

new_block = '''            string appDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\\\');
            string iniPath = Path.Combine(appDir, \"AVRoad.ini\");
            string fontPath = \"\";
            string plotStylePath = Path.Combine(appDir, \"Plot Styles\");
            string plotterPath = Path.Combine(appDir, \"Plotters\");
            string argProfile = Path.Combine(appDir, \"AVRoad_Profile.arg\");

            foreach (string a in args)
            {
                if (a.Equals(\"/prepare-only\", StringComparison.OrdinalIgnoreCase))
                {
                    PrepareProfile(appDir, argProfile, fontPath, plotStylePath, plotterPath);
                    RegisterAutoCADApplications(appDir);
                    return;
                }'''

code = code.replace(old_block, new_block)

# Remove the duplicated declarations later in Main
dup_vars = '''            string fontPath = \"\";
            string plotStylePath = Path.Combine(appDir, \"Plot Styles\");
            string plotterPath = Path.Combine(appDir, \"Plotters\");
            string argProfile = Path.Combine(appDir, \"AVRoad_Profile.arg\");'''
# We only want to remove the second occurrence
parts = code.split(dup_vars)
if len(parts) == 3:
    code = parts[0] + dup_vars + parts[1] + parts[2]

with open('AVRoad_Launcher.cs', 'w', encoding='utf-8') as f:
    f.write(code)
print('Moved var declarations!')
