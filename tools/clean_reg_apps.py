import re

clean_body = '''        public static void RegisterAutoCADApplications(string appDir)
        {
            // Pure AutoCAD 2007: clean up any legacy custom application entries
            try
            {
                string[] keysToDelete = new string[] {
                    "AcLayer", "AcLayerTools", "AcadUnits", "AcadDim", "AcadDWGAids",
                    "AcadFind", "AcadPurge", "AcadMatch", "AcadTable", "AcadBlock",
                    "AcadArray", "AcadOPM", "AcOpm", "AcadAppload"
                };
                string[] baseKeys = new string[] {
                    @"Software\\Autodesk\\AutoCAD\\R17.0\\ACAD-5001:409\\Applications",
                    @"Software\\WOW6432Node\\Autodesk\\AutoCAD\\R17.0\\ACAD-5001:409\\Applications"
                };
                foreach (string bk in baseKeys)
                {
                    try
                    {
                        RegistryKey rootKey = bk.Contains("WOW6432Node") ? Registry.LocalMachine : Registry.CurrentUser;
                        using (RegistryKey appRoot = rootKey.OpenSubKey(bk, true))
                        {
                            if (appRoot == null) continue;
                            foreach (string k in keysToDelete)
                            {
                                try { appRoot.DeleteSubKeyTree(k, false); } catch { }
                            }
                        }
                    }
                    catch { }
                }

                // Pre-register TDTObjectsWrapper COM server to avoid "Failed to register COM server"
                try
                {
                    string wrapperPath = Path.Combine(appDir, "TDTObjectsWrapper.arx");
                    if (File.Exists(wrapperPath))
                    {
                        ProcessStartInfo psiReg = new ProcessStartInfo("regsvr32.exe", "/s \\"" + wrapperPath + "\\"")
                        {
                            CreateNoWindow = true,
                            UseShellExecute = false
                        };
                        using (Process pReg = Process.Start(psiReg))
                        {
                            pReg.WaitForExit(3000);
                        }
                    }
                }
                catch { }

                // Clean old cached MNR files so new AV_ menus recompile cleanly
                try
                {
                    string appDataSupport = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Autodesk\\AutoCAD 2007\\R17.0\\enu\\Support");
                    foreach (string folder in new string[] { appDir, appDataSupport })
                    {
                        if (Directory.Exists(folder))
                        {
                            foreach (string mnr in Directory.GetFiles(folder, "*.mnr"))
                            {
                                try { File.Delete(mnr); } catch { }
                            }
                        }
                    }
                }
                catch { }
            }
            catch { }
        }
'''

for file_path in [r'c:\TDT_RESEARCH\AVRoad_Launcher.cs', r'c:\TDT_RESEARCH\Setup_AVRoad_2026.cs']:
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()

    # Find RegisterAutoCADApplications definition
    pattern = r'(public|private)\s+static\s+void\s+RegisterAutoCADApplications\(string appDir\)[\s\S]*?\n        \}\r?\n'
    m = re.search(pattern, content)
    if m:
        content = content[:m.start()] + clean_body.replace('\n', '\r\n') + content[m.end():]
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(content)
        print(f'Updated {file_path}')
    else:
        print(f'Pattern not found in {file_path}')
