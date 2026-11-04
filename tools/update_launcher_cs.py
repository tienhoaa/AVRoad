import re

# Read AVRoad_Launcher.cs
with open(r'C:\TDT_RESEARCH\AVRoad_Launcher.cs', 'r', encoding='utf-8') as f:
    content = f.read()

replacement = """        [DllImport("oleaut32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int LoadTypeLib(string szFile, out IntPtr pptlib);

        [DllImport("oleaut32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int RegisterTypeLibForUser(IntPtr ptlib, string szFullPath, string szHelpDir);

        public static void RegisterTypeLibraries(string appDir)
        {
            string[] tlbs = new string[]
            {
                "acax17enu.tlb", "axdb17enu.tlb", "AcAuthEntities17enu.tlb",
                "AcMPolygon17enu.tlb", "AcObjClassImp17.tlb", "AcSmComponents17.tlb",
                "AdComFolderWatch17.tlb", "vl16.tlb", "ACcolor.ocx",
                "AcadLinetype.ocx", "HatchPattern.ocx"
            };
            foreach (string file in tlbs)
            {
                try
                {
                    string fullPath = Path.Combine(appDir, file);
                    if (File.Exists(fullPath))
                    {
                        IntPtr pTlib;
                        int hr = LoadTypeLib(fullPath, out pTlib);
                        if (hr == 0 && pTlib != IntPtr.Zero)
                        {
                            RegisterTypeLibForUser(pTlib, fullPath, appDir);
                            Marshal.Release(pTlib);
                        }
                    }
                }
                catch { }
            }
        }

        public static void RegisterAutoCADCOMClasses(string appDir)
        {
            try
            {
                string acadExe = Path.Combine(appDir, "acad.exe");
                using (RegistryKey classes = Registry.CurrentUser.OpenSubKey(@"Software\Classes", true))
                {
                    if (classes == null) return;

                    // 1. AutoCAD.Application.17
                    using (RegistryKey app17 = classes.CreateSubKey("AutoCAD.Application.17"))
                    {
                        if (app17 != null)
                        {
                            app17.SetValue("", "AutoCAD Application 17");
                            using (RegistryKey c = app17.CreateSubKey("CLSID")) { if (c != null) c.SetValue("", "{28B7AA99-C0F9-4C47-995E-8A8D729603A1}"); }
                        }
                    }

                    // 2. AutoCAD.Application
                    using (RegistryKey app = classes.CreateSubKey("AutoCAD.Application"))
                    {
                        if (app != null)
                        {
                            app.SetValue("", "AutoCAD Application");
                            using (RegistryKey c = app.CreateSubKey("CLSID")) { if (c != null) c.SetValue("", "{28B7AA99-C0F9-4C47-995E-8A8D729603A1}"); }
                            using (RegistryKey cv = app.CreateSubKey("CurVer")) { if (cv != null) cv.SetValue("", "AutoCAD.Application.17"); }
                        }
                    }

                    // 3. CLSID AcadApplication
                    string clsidAppPath = @"CLSID\{28B7AA99-C0F9-4C47-995E-8A8D729603A1}";
                    using (RegistryKey clsidApp = classes.CreateSubKey(clsidAppPath))
                    {
                        if (clsidApp != null)
                        {
                            clsidApp.SetValue("", "AcadApplication");
                            using (RegistryKey p = clsidApp.CreateSubKey("ProgID")) { if (p != null) p.SetValue("", "AutoCAD.Application.17"); }
                            using (RegistryKey p = clsidApp.CreateSubKey("VersionIndependentProgID")) { if (p != null) p.SetValue("", "AutoCAD.Application"); }
                            using (RegistryKey p = clsidApp.CreateSubKey("Programmable")) { if (p != null) p.SetValue("", ""); }
                            using (RegistryKey p = clsidApp.CreateSubKey("TypeLib")) { if (p != null) p.SetValue("", "{851A4561-F4EC-4631-9B0C-E7DC407512C9}"); }
                            using (RegistryKey p = clsidApp.CreateSubKey("LocalServer32")) { if (p != null) p.SetValue("", "\\\"" + acadExe + "\\\" /Automation"); }
                        }
                    }

                    // 4. AutoCAD.Drawing.17
                    using (RegistryKey dwg17 = classes.CreateSubKey("AutoCAD.Drawing.17"))
                    {
                        if (dwg17 != null)
                        {
                            dwg17.SetValue("", "AutoCAD Drawing 17");
                            using (RegistryKey c = dwg17.CreateSubKey("CLSID")) { if (c != null) c.SetValue("", "{7AABBB95-79BE-4C0F-8024-EB6AF271231C}"); }
                        }
                    }

                    // 5. AutoCAD.Drawing
                    using (RegistryKey dwg = classes.CreateSubKey("AutoCAD.Drawing"))
                    {
                        if (dwg != null)
                        {
                            dwg.SetValue("", "AutoCAD Drawing");
                            using (RegistryKey c = dwg.CreateSubKey("CLSID")) { if (c != null) c.SetValue("", "{7AABBB95-79BE-4C0F-8024-EB6AF271231C}"); }
                            using (RegistryKey cv = dwg.CreateSubKey("CurVer")) { if (cv != null) cv.SetValue("", "AutoCAD.Drawing.17"); }
                        }
                    }

                    // 6. CLSID AcadDocument
                    string clsidDocPath = @"CLSID\{7AABBB95-79BE-4C0F-8024-EB6AF271231C}";
                    using (RegistryKey clsidDoc = classes.CreateSubKey(clsidDocPath))
                    {
                        if (clsidDoc != null)
                        {
                            clsidDoc.SetValue("", "AcadDocument");
                            using (RegistryKey p = clsidDoc.CreateSubKey("ProgID")) { if (p != null) p.SetValue("", "AutoCAD.Drawing.17"); }
                            using (RegistryKey p = clsidDoc.CreateSubKey("VersionIndependentProgID")) { if (p != null) p.SetValue("", "AutoCAD.Drawing"); }
                            using (RegistryKey p = clsidDoc.CreateSubKey("Programmable")) { if (p != null) p.SetValue("", ""); }
                            using (RegistryKey p = clsidDoc.CreateSubKey("TypeLib")) { if (p != null) p.SetValue("", "{851A4561-F4EC-4631-9B0C-E7DC407512C9}"); }
                            using (RegistryKey p = clsidDoc.CreateSubKey("LocalServer32")) { if (p != null) p.SetValue("", "\\\"" + acadExe + "\\\" /Automation"); }
                        }
                    }

                    // 7. Event Interfaces
                    using (RegistryKey ifApp = classes.CreateSubKey(@"Interface\{22C8E547-0D27-4793-BB9F-AC58D4F8E794}"))
                    {
                        if (ifApp != null)
                        {
                            ifApp.SetValue("", "_DAcadApplicationEvents");
                            using (RegistryKey t = ifApp.CreateSubKey("TypeLib")) { if (t != null) t.SetValue("", "{851A4561-F4EC-4631-9B0C-E7DC407512C9}"); }
                        }
                    }
                    using (RegistryKey ifDoc = classes.CreateSubKey(@"Interface\{C814C69D-A74B-494C-8EE6-23731B145CCB}"))
                    {
                        if (ifDoc != null)
                        {
                            ifDoc.SetValue("", "_DAcadDocumentEvents");
                            using (RegistryKey t = ifDoc.CreateSubKey("TypeLib")) { if (t != null) t.SetValue("", "{851A4561-F4EC-4631-9B0C-E7DC407512C9}"); }
                        }
                    }
                }
            }
            catch { }
        }

        public static void RegisterAutoCADApplications(string appDir)
        {
            try
            {
                // Register demand load applications under HKCU
                using (RegistryKey appRoot = Registry.CurrentUser.CreateSubKey(@"Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409\Applications"))
                {
                    if (appRoot != null)
                    {
                        // 1. AcOpm (Properties Palette)
                        using (RegistryKey acOpm = appRoot.CreateSubKey("AcOpm"))
                        {
                            if (acOpm != null)
                            {
                                acOpm.SetValue("LOADER", Path.Combine(appDir, "acopm.arx"), RegistryValueKind.String);
                                acOpm.SetValue("LOADCTRLS", 14, RegistryValueKind.DWord);
                                acOpm.SetValue("DESCRIPTION", "AutoCAD Object Property Manager", RegistryValueKind.String);
                                using (RegistryKey cmdKey = acOpm.CreateSubKey("Commands"))
                                {
                                    if (cmdKey != null)
                                    {
                                        cmdKey.SetValue("PROPERTIES", "PROPERTIES", RegistryValueKind.String);
                                        cmdKey.SetValue("PROPERTIESCLOSE", "PROPERTIESCLOSE", RegistryValueKind.String);
                                    }
                                }
                            }
                        }

                        // 2. AcLayer (Layer Dialog)
                        using (RegistryKey acLayer = appRoot.CreateSubKey("AcLayer"))
                        {
                            if (acLayer != null)
                            {
                                acLayer.SetValue("LOADER", Path.Combine(appDir, "aclayer.dll"), RegistryValueKind.String);
                                acLayer.SetValue("LOADCTRLS", 14, RegistryValueKind.DWord);
                                acLayer.SetValue("DESCRIPTION", "Layer Manager", RegistryValueKind.String);
                                using (RegistryKey cmdKey = acLayer.CreateSubKey("Commands"))
                                {
                                    if (cmdKey != null)
                                    {
                                        cmdKey.SetValue("LAYER", "LAYER", RegistryValueKind.String);
                                    }
                                }
                            }
                        }

                        // 3. AcadAppload (Appload Dialog)
                        using (RegistryKey appload = appRoot.CreateSubKey("AcadAppload"))
                        {
                            if (appload != null)
                            {
                                appload.SetValue("LOADER", Path.Combine(appDir, "Appload.arx"), RegistryValueKind.String);
                                appload.SetValue("LOADCTRLS", 13, RegistryValueKind.DWord);
                                appload.SetValue("DESCRIPTION", "Application Loader", RegistryValueKind.String);
                                using (RegistryKey cmdKey = appload.CreateSubKey("Commands"))
                                {
                                    if (cmdKey != null)
                                    {
                                        cmdKey.SetValue("APPLOAD", "APPLOAD", RegistryValueKind.String);
                                    }
                                }
                            }
                        }

                        // 4. AcadArray
                        using (RegistryKey k = appRoot.CreateSubKey("AcadArray"))
                        {
                            if (k != null)
                            {
                                k.SetValue("LOADER", Path.Combine(appDir, "AcArray.arx"), RegistryValueKind.String);
                                k.SetValue("LOADCTRLS", 13, RegistryValueKind.DWord);
                                k.SetValue("DESCRIPTION", "Array Module", RegistryValueKind.String);
                                using (RegistryKey cmdKey = k.CreateSubKey("Commands"))
                                {
                                    if (cmdKey != null)
                                    {
                                        cmdKey.SetValue("ARRAY", "ARRAY", RegistryValueKind.String);
                                    }
                                }
                            }
                        }
                    }
                }

                // Register Type Libraries and COM classes
                RegisterTypeLibraries(appDir);
                RegisterAutoCADCOMClasses(appDir);

                // Pre-register TDTObjectsWrapper COM server to avoid "Failed to register COM server"
                try
                {
                    string wrapperPath = Path.Combine(appDir, "TDTObjectsWrapper.arx");
                    if (File.Exists(wrapperPath))
                    {
                        ProcessStartInfo psiReg = new ProcessStartInfo("regsvr32.exe", "/s \\\"" + wrapperPath + "\\\"")
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

        [DllImport("shell32.dll")]
        public static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

        public static void CleanPortableAssociations()
        {
            try
            {
                using (RegistryKey k = Registry.CurrentUser.OpenSubKey(@"Software\\Classes\\.dwg", true))
                {
                    if (k != null)
                    {
                        object val = k.GetValue("");
                        if (val != null && val.ToString().IndexOf("AutoCAD.Drawing.17", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            try { Registry.CurrentUser.DeleteSubKeyTree(@"Software\\Classes\\.dwg", false); } catch { }
                        }
                    }
                }
                try { SHChangeNotify(0x08000000, 0, IntPtr.Zero, IntPtr.Zero); } catch { }
            }
            catch { }
        }

        public static void FixPropertiesPalette(string appDir = null)
        {
            try
            {
                if (string.IsNullOrEmpty(appDir))
                {
                    appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                }
                string appDataProfiles = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Autodesk\\AutoCAD 2007\\R17.0\\enu\\Support\\Profiles");
                Directory.CreateDirectory(appDataProfiles);

                string srcAws = Path.Combine(appDir, "FixedProfile.aws");
                if (!File.Exists(srcAws))
                {
                    string fallback = @"C:\\AVRoad_2026\\FixedProfile.aws";
                    if (File.Exists(fallback)) srcAws = fallback;
                }

                string targetFixed = Path.Combine(appDataProfiles, "FixedProfile.aws");
                if (File.Exists(srcAws))
                {
                    try { File.Copy(srcAws, targetFixed, true); } catch { }
                }

                string[] targetProfiles = new string[] { "AVRoad", "<<Unnamed Profile>>", "Unnamed Profile" };
                foreach (string prof in targetProfiles)
                {
                    string pDir = Path.Combine(appDataProfiles, prof);
                    Directory.CreateDirectory(pDir);
                    string targetProfileAws = Path.Combine(pDir, "Profile.aws");
                    bool needsCopy = !File.Exists(targetProfileAws) || new FileInfo(targetProfileAws).Length < 500;
                    if (!needsCopy && File.Exists(targetProfileAws))
                    {
                        try
                        {
                            string txt = File.ReadAllText(targetProfileAws);
                            if (!txt.Contains("{6D32A2D9-832E-11D2-A83C-0060B0872C0B}") || txt.Contains("Orientation=\\\"59422\\\""))
                            {
                                needsCopy = true;
                            }
                        }
                        catch { }
                    }
                    if (needsCopy && File.Exists(srcAws))
                    {
                        try { File.Copy(srcAws, targetProfileAws, true); } catch { }
                    }
                }

                string[] awsFiles = Directory.GetFiles(appDataProfiles, "*.aws", SearchOption.AllDirectories);
                foreach (string aws in awsFiles)
                {
                    try
                    {
                        var doc = new System.Xml.XmlDocument();
                        doc.Load(aws);
                        bool modified = false;
                        var tools = doc.GetElementsByTagName("Tool");
                        foreach (System.Xml.XmlElement tool in tools)
                        {
                            string clsid = tool.GetAttribute("CLSID");
                            var pset = tool.SelectSingleNode("CAdUiPaletteSet") as System.Xml.XmlElement;
                            if ((pset != null && pset.GetAttribute("Name") == "PROPERTIES") || clsid.Equals("{6D32A2D9-832E-11D2-A83C-0060B0872C0B}", StringComparison.OrdinalIgnoreCase))
                            {
                                var dock = tool.SelectSingleNode("CAdUiDockControlBar") as System.Xml.XmlElement;
                                if (dock != null)
                                {
                                    dock.SetAttribute("Orientation", "-1");
                                    dock.SetAttribute("AllowDocking", "0");
                                    var finfo = dock.SelectSingleNode("FloatInfo") as System.Xml.XmlElement;
                                    if (finfo != null)
                                    {
                                        finfo.SetAttribute("Left", "60");
                                        finfo.SetAttribute("Top", "120");
                                        finfo.SetAttribute("Width", "300");
                                        finfo.SetAttribute("Height", "500");
                                    }
                                    var dinfo = dock.SelectSingleNode("DockInfo") as System.Xml.XmlElement;
                                    if (dinfo != null)
                                    {
                                        dinfo.SetAttribute("MRUDockID", "0");
                                        dinfo.SetAttribute("Left", "0");
                                        dinfo.SetAttribute("Top", "0");
                                        dinfo.SetAttribute("Width", "200");
                                        dinfo.SetAttribute("Height", "300");
                                    }
                                    modified = true;
                                }
                            }
                        }
                        if (modified) doc.Save(aws);
                    }
                    catch { }
                }
            }
            catch { }
        }
"""

# Regex find from "public static void RegisterAutoCADApplications" to end of FixPropertiesPalette method
pattern = re.compile(r'(\s*public static void RegisterAutoCADApplications\(string appDir\)[\s\S]*?public static void FixPropertiesPalette\(\)[\s\S]*?\n\s*\}\s*\n\s*\}\s*\n\s*\}\s*\n\s*\}\s*\n)', re.DOTALL)
match = pattern.search(content)
if not match:
    print("Could not match pattern!")
    exit(1)

# Keep the ending class/namespace braces:
new_code = replacement + "\n    }\n}\n}"
content_updated = content[:match.start()] + new_code

with open(r'C:\TDT_RESEARCH\AVRoad_Launcher.cs', 'w', encoding='utf-8') as f:
    f.write(content_updated)

print("Successfully updated AVRoad_Launcher.cs!")
