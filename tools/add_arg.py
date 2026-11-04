with open("AVRoad_Launcher.cs", "r", encoding="utf-8") as f:
    code = f.read()
target = 'if (a.Equals("/about", StringComparison.OrdinalIgnoreCase))'
replacement = '''if (a.Equals("/prepare-only", StringComparison.OrdinalIgnoreCase))
                {
                    PrepareProfile(appDir, argProfile, fontPath, plotStylePath, plotterPath);
                    RegisterAutoCADApplications(appDir);
                    return;
                }
                if (a.Equals("/about", StringComparison.OrdinalIgnoreCase))'''
code = code.replace(target, replacement)
with open("AVRoad_Launcher.cs", "w", encoding="utf-8") as f:
    f.write(code)
print("Added /prepare-only arg!")
