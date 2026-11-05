using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Xml;
using Microsoft.Win32;

namespace AVRoadDeployment
{
    public static class RegistryScope
    {
        // RC1 deliberately does not re-run legacy HKLM/ACL/type-library repairs.
        // Clean-machine bootstrap beyond these three values is not yet approved/tested.
        public static bool IsLimited { get { return true; } }
    }
    // Only the three native factories verified against the matching CAD7 snapshot.
    public static class Cad7ComRegistration
    {
        public static readonly string[] Ids = {
            "{B53CEF4B-1A13-49DE-BBC5-A7100FB2F38C}",
            "{F78DCF7C-043D-45FC-9D21-676FC307BA3F}",
            "{4BD03680-3C0F-4501-AFF7-3D008586917F}" };

        public static string Apply(string appDir, string backupFile)
        {
            string server = Path.GetFullPath(Path.Combine(appDir, "axdb.dll"));
            if (!File.Exists(server)) throw new FileNotFoundException("Missing native ObjectDBX server", server);
            if (String.IsNullOrEmpty(backupFile)) backupFile = Path.Combine(appDir, ".avroad-rollback", "com-v2.xml");
            using (RegistryKey root = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32))
            {
                XmlDocument doc = new XmlDocument();
                XmlElement list = doc.CreateElement("ComBackup"); doc.AppendChild(list);
                foreach (string id in Ids)
                {
                    string parent = @"Software\Classes\CLSID\" + id;
                    string path = parent + @"\InprocServer32";
                    using (RegistryKey p = root.OpenSubKey(parent, false))
                    using (RegistryKey k = root.OpenSubKey(path, false))
                    {
                        XmlElement row = doc.CreateElement("Value");
                        row.SetAttribute("path", path); row.SetAttribute("parent", parent);
                        row.SetAttribute("parentExisted", (p != null).ToString());
                        row.SetAttribute("keyExisted", (k != null).ToString());
                        bool has = k != null && Array.IndexOf(k.GetValueNames(), "") >= 0;
                        row.SetAttribute("hasValue", has.ToString()); row.SetAttribute("target", server);
                        if (has)
                        {
                            RegistryValueKind kind = k.GetValueKind("");
                            if (kind != RegistryValueKind.String && kind != RegistryValueKind.ExpandString)
                                throw new InvalidDataException("Unexpected COM registry value kind: " + path);
                            row.SetAttribute("kind", kind.ToString());
                            row.InnerText = (string)k.GetValue("", "", RegistryValueOptions.DoNotExpandEnvironmentNames);
                        }
                        list.AppendChild(row);
                    }
                }
                // Preserve the first rollback point across idempotent launches.
                if (!File.Exists(backupFile))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(backupFile)));
                    doc.Save(backupFile);
                }
                try
                {
                    foreach (XmlElement row in list.ChildNodes)
                        using (RegistryKey k = root.CreateSubKey(row.GetAttribute("path")))
                        {
                            k.SetValue("", server, RegistryValueKind.String);
                            if (!String.Equals((string)k.GetValue(""), server, StringComparison.OrdinalIgnoreCase))
                                throw new IOException("COM readback failed");
                        }
                }
                catch
                {
                    // Roll back this call's state, rather than an older launch's state.
                    RestoreDocument(doc, root); throw;
                }
            }
            return backupFile;
        }

        public static void Restore(string backupFile)
        {
            XmlDocument doc = new XmlDocument(); doc.Load(backupFile);
            using (RegistryKey root = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32))
                RestoreDocument(doc, root);
        }

        private static void RestoreDocument(XmlDocument doc, RegistryKey root)
        {
            if (doc.DocumentElement == null || doc.DocumentElement.Name != "ComBackup" || doc.DocumentElement.ChildNodes.Count != 3)
                throw new InvalidDataException("Invalid COM rollback file");
            // Validate the complete backup before writing anything.
            foreach (XmlElement row in doc.DocumentElement.ChildNodes)
            {
                bool allowed = false;
                foreach (string id in Ids)
                    if (row.GetAttribute("path") == @"Software\Classes\CLSID\" + id + @"\InprocServer32" &&
                        row.GetAttribute("parent") == @"Software\Classes\CLSID\" + id) allowed = true;
                if (!allowed) throw new InvalidDataException("Rollback outside the approved COM scope");
                using (RegistryKey k = root.OpenSubKey(row.GetAttribute("path"), false))
                {
                    string value = k == null ? null : k.GetValue("", null, RegistryValueOptions.DoNotExpandEnvironmentNames) as string;
                    if (value != null && value != row.GetAttribute("target") && value != row.InnerText)
                        throw new IOException("COM changed after deployment; manual review required");
                }
            }
            foreach (XmlElement row in doc.DocumentElement.ChildNodes)
            {
                using (RegistryKey k = root.OpenSubKey(row.GetAttribute("path"), true))
                    if (k != null)
                    {
                        if (Boolean.Parse(row.GetAttribute("hasValue")))
                            k.SetValue("", row.InnerText, (RegistryValueKind)Enum.Parse(typeof(RegistryValueKind), row.GetAttribute("kind")));
                        else k.DeleteValue("", false);
                    }
                RemoveCreatedEmpty(root, row.GetAttribute("path"), row.GetAttribute("keyExisted"));
                RemoveCreatedEmpty(root, row.GetAttribute("parent"), row.GetAttribute("parentExisted"));
            }
        }
        private static void RemoveCreatedEmpty(RegistryKey root, string path, string existed)
        {
            if (Boolean.Parse(existed)) return;
            bool empty;
            using (RegistryKey k = root.OpenSubKey(path, false)) empty = k != null && k.ValueCount == 0 && k.SubKeyCount == 0;
            if (empty) root.DeleteSubKey(path, false);
        }
    }

    // Initializes only the AVRoad per-user profile exported by this deployment.
    // It deliberately does not create Autodesk product keys or register TypeLibs.
    public static class Cad7ProfileBootstrap
    {
        private const string ProfilePath = @"Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409\Profiles\AVRoad";
        private const string ProfileRegPath = @"HKEY_CURRENT_USER\Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409\Profiles\AVRoad";

        public static void ValidateExport(string profileFile)
        {
            if (!File.Exists(profileFile)) throw new FileNotFoundException("Missing AVRoad profile export", profileFile);
            string allowed = "[" + ProfileRegPath + "]";
            bool hasGeneral = false;
            foreach (string raw in File.ReadAllLines(profileFile))
            {
                string line = raw.Trim();
                if (line.StartsWith("[-", StringComparison.Ordinal))
                    throw new InvalidDataException("AVRoad profile export contains registry deletions");
                if (!line.StartsWith("[", StringComparison.Ordinal)) continue;
                if (!line.EndsWith("]", StringComparison.Ordinal) ||
                    !(String.Equals(line, allowed, StringComparison.OrdinalIgnoreCase) ||
                      line.StartsWith(allowed.Substring(0, allowed.Length - 1) + "\\", StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidDataException("AVRoad profile export escapes the approved HKCU Profiles\\AVRoad key");
                if (line.StartsWith(allowed.Substring(0, allowed.Length - 1) + "\\General]", StringComparison.OrdinalIgnoreCase)) hasGeneral = true;
            }
            if (!hasGeneral) throw new InvalidDataException("AVRoad profile export has no General profile section");
        }

        public static void EnsureProductBaseline(string appDir, XmlDocument state)
        {
            // 1. ProgramData folders
            try
            {
                string pdataEnu = @"C:\ProgramData\Autodesk\AutoCAD 2007\R17.0\enu";
                string pdataAdlm = @"C:\ProgramData\Autodesk\AutoCAD 2007\R17.0\Adlm";
                if (!Directory.Exists(pdataEnu)) Directory.CreateDirectory(pdataEnu);
                if (!Directory.Exists(pdataAdlm)) Directory.CreateDirectory(pdataAdlm);
            }
            catch { }

            // 2. HKCU 32-bit product identity
            try
            {
                using (RegistryKey root = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32))
                {
                    using (RegistryKey kAcad = root.CreateSubKey(@"Software\Autodesk\AutoCAD"))
                    {
                        if (kAcad != null) kAcad.SetValue("CurVer", "R17.0", RegistryValueKind.String);
                    }
                    using (RegistryKey kR17 = root.CreateSubKey(@"Software\Autodesk\AutoCAD\R17.0"))
                    {
                        if (kR17 != null) kR17.SetValue("CurVer", "ACAD-5001:409", RegistryValueKind.String);
                    }
                    using (RegistryKey mainKey = root.CreateSubKey(@"Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409"))
                    {
                        if (mainKey != null)
                        {
                            mainKey.SetValue("Roaming", 1, RegistryValueKind.DWord);
                            mainKey.SetValue("ExtensionRootFolder", @"C:\ProgramData\Autodesk\AutoCAD 2007\R17.0\enu\", RegistryValueKind.String);
                            string appDataEnu = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Autodesk\AutoCAD 2007\R17.0\enu") + @"\";
                            string localDataEnu = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Autodesk\AutoCAD 2007\R17.0\enu") + @"\";
                            mainKey.SetValue("RoamableRootFolder", appDataEnu, RegistryValueKind.String);
                            mainKey.SetValue("LocalRootFolder", localDataEnu, RegistryValueKind.String);
                            mainKey.SetValue("Release", "17.0.54.110", RegistryValueKind.String);
                            mainKey.SetValue("Language", "English", RegistryValueKind.String);
                            mainKey.SetValue("ProductCode", "{5783F2D7-5001-0409-0002-0060B0CE6BBA}", RegistryValueKind.String);
                            mainKey.SetValue("AcadLocation", appDir, RegistryValueKind.String);
                            mainKey.SetValue("Location", appDir, RegistryValueKind.String);
                            mainKey.SetValue("ProductId", "5001", RegistryValueKind.String);
                            mainKey.SetValue("LangAbbrev", "enu", RegistryValueKind.String);
                            mainKey.SetValue("LocaleId", 0x409, RegistryValueKind.DWord);
                            mainKey.SetValue("ProductName", "AutoCAD 2007", RegistryValueKind.String);
                            mainKey.SetValue("StandaloneNetworkType", 1, RegistryValueKind.DWord);
                            mainKey.SetValue("NetSupport", "TCP", RegistryValueKind.String);
                            mainKey.SetValue("AdLMInfoPath", @"C:\ProgramData\Autodesk\AutoCAD 2007\R17.0\Adlm\", RegistryValueKind.String);
                            mainKey.SetValue("SerialNumber", "000-00000000", RegistryValueKind.String);
                            mainKey.SetValue("DefaultHelpFileName", "acad170", RegistryValueKind.String);
                        }
                    }
                    using (RegistryKey profKey = root.CreateSubKey(@"Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409\Profiles"))
                    {
                        if (profKey != null && profKey.GetValue("") == null)
                        {
                            profKey.SetValue("", "AVRoad", RegistryValueKind.String);
                        }
                    }
                }
            }
            catch { }

            // 3. HKLM 32-bit product baseline (scoped, only if elevated, no GrantUsersFullControl)
            try
            {
                using (RegistryKey rootLM = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
                {
                    using (RegistryKey existingLM = rootLM.OpenSubKey(@"Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409", false))
                    {
                        if (existingLM == null && state != null)
                        {
                            state.DocumentElement.SetAttribute("hklmCreated", "true");
                        }
                    }
                    using (RegistryKey kAcadLM = rootLM.CreateSubKey(@"Software\Autodesk\AutoCAD"))
                    {
                        if (kAcadLM != null) kAcadLM.SetValue("CurVer", "R17.0", RegistryValueKind.String);
                    }
                    using (RegistryKey kR17LM = rootLM.CreateSubKey(@"Software\Autodesk\AutoCAD\R17.0"))
                    {
                        if (kR17LM != null) kR17LM.SetValue("CurVer", "ACAD-5001:409", RegistryValueKind.String);
                    }
                    using (RegistryKey mainKeyLM = rootLM.CreateSubKey(@"Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409"))
                    {
                        if (mainKeyLM != null)
                        {
                            mainKeyLM.SetValue("Roaming", 1, RegistryValueKind.DWord);
                            mainKeyLM.SetValue("ExtensionRootFolder", @"C:\ProgramData\Autodesk\AutoCAD 2007\R17.0\enu\", RegistryValueKind.String);
                            string appDataEnu = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Autodesk\AutoCAD 2007\R17.0\enu") + @"\";
                            string localDataEnu = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Autodesk\AutoCAD 2007\R17.0\enu") + @"\";
                            mainKeyLM.SetValue("RoamableRootFolder", appDataEnu, RegistryValueKind.String);
                            mainKeyLM.SetValue("LocalRootFolder", localDataEnu, RegistryValueKind.String);
                            mainKeyLM.SetValue("Release", "17.0.54.110", RegistryValueKind.String);
                            mainKeyLM.SetValue("Language", "English", RegistryValueKind.String);
                            mainKeyLM.SetValue("ProductCode", "{5783F2D7-5001-0409-0002-0060B0CE6BBA}", RegistryValueKind.String);
                            mainKeyLM.SetValue("AcadLocation", appDir, RegistryValueKind.String);
                            mainKeyLM.SetValue("Location", appDir, RegistryValueKind.String);
                            mainKeyLM.SetValue("ProductId", "5001", RegistryValueKind.String);
                            mainKeyLM.SetValue("LangAbbrev", "enu", RegistryValueKind.String);
                            mainKeyLM.SetValue("LocaleId", 0x409, RegistryValueKind.DWord);
                            mainKeyLM.SetValue("ProductName", "AutoCAD 2007", RegistryValueKind.String);
                            mainKeyLM.SetValue("StandaloneNetworkType", 1, RegistryValueKind.DWord);
                            mainKeyLM.SetValue("NetSupport", "TCP", RegistryValueKind.String);
                            mainKeyLM.SetValue("AdLMInfoPath", @"C:\ProgramData\Autodesk\AutoCAD 2007\R17.0\Adlm\", RegistryValueKind.String);
                            if (mainKeyLM.GetValue("SerialNumber") == null)
                                mainKeyLM.SetValue("SerialNumber", "000-00000000", RegistryValueKind.String);
                            mainKeyLM.SetValue("DefaultHelpFileName", "acad170", RegistryValueKind.String);
                        }
                    }
                }
            }
            catch
            {
                try
                {
                    using (RegistryKey rootLM = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
                    using (RegistryKey checkLM = rootLM.OpenSubKey(@"Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409", false))
                    {
                        if (checkLM == null || checkLM.GetValue("SerialNumber") == null || checkLM.GetValue("AcadLocation") == null)
                        {
                            string avExe = Path.Combine(appDir, "AVRoad.exe");
                            if (File.Exists(avExe))
                            {
                                ProcessStartInfo psi = new ProcessStartInfo(avExe, "/prepare-only");
                                psi.Verb = "runas";
                                psi.UseShellExecute = true;
                                using (Process p = Process.Start(psi))
                                {
                                    if (p != null) p.WaitForExit(15000);
                                }
                            }
                        }
                    }
                }
                catch { }
            }

            // 4. AWS profile files in AppData
            try
            {
                string appDataProfiles = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Autodesk\AutoCAD 2007\R17.0\enu\Support\Profiles");
                if (!Directory.Exists(appDataProfiles)) Directory.CreateDirectory(appDataProfiles);
                string avroadProfDir = Path.Combine(appDataProfiles, "AVRoad");
                if (!Directory.Exists(avroadProfDir)) Directory.CreateDirectory(avroadProfDir);

                string srcAws = Path.Combine(appDir, "Profile.aws");
                if (!File.Exists(srcAws)) srcAws = Path.Combine(appDir, @"UserDataCache\Support\Profiles\AVRoad\Profile.aws");

                if (File.Exists(srcAws))
                {
                    string targetFixed = Path.Combine(appDataProfiles, "FixedProfile.aws");
                    if (!File.Exists(targetFixed)) File.Copy(srcAws, targetFixed, true);
                    string targetAvroad = Path.Combine(avroadProfDir, "Profile.aws");
                    if (!File.Exists(targetAvroad)) File.Copy(srcAws, targetAvroad, true);
                }
            }
            catch { }
        }

        public static void Ensure(string appDir, string profileFile)
        {
            appDir = Path.GetFullPath(appDir);
            ValidateExport(profileFile);
            string rollbackDir = Path.Combine(appDir, ".avroad-rollback", "profile-bootstrap");
            Directory.CreateDirectory(rollbackDir);
            string statePath = Path.Combine(rollbackDir, "state.xml");
            string beforePath = Path.Combine(rollbackDir, "profile-before.reg");

            using (RegistryKey root = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32))
            {
                bool stateExists = File.Exists(statePath);
                XmlDocument state = stateExists ? LoadState(statePath) : NewState();

                EnsureProductBaseline(appDir, state);
                bool profileExisted;
                if (stateExists) profileExisted = Boolean.Parse(state.DocumentElement.GetAttribute("existed"));
                else { using (RegistryKey existing = root.OpenSubKey(ProfilePath, false)) profileExisted = existing != null; }
                if (!stateExists)
                {
                    if (profileExisted) RunReg("export " + Quote(ProfileRegPath) + " " + Quote(beforePath) + " /y");
                    state.DocumentElement.SetAttribute("existed", profileExisted.ToString());
                    state.DocumentElement.SetAttribute("before", profileExisted ? Path.GetFileName(beforePath) : "");
                    state.Save(statePath);
                }

                using (RegistryKey general = root.OpenSubKey(ProfilePath + @"\General", false))
                    if (general == null) RunReg("import " + Quote(profileFile));

                using (RegistryKey general = root.CreateSubKey(ProfilePath + @"\General"))
                {
                    if (general == null) throw new IOException("Could not initialize HKCU AVRoad profile");
                    ApplyProfilePaths(general, appDir);
                    general.SetValue("DWGCheck", 0, RegistryValueKind.DWord);
                    general.SetValue("Pickfirst", 1, RegistryValueKind.DWord);
                    general.SetValue("Pickstyle", 1, RegistryValueKind.DWord);
                    general.SetValue("DwgAlwaysAssociate", 0, RegistryValueKind.DWord);
                }

                string current = Path.Combine(rollbackDir, "profile-current.reg");
                RunReg("export " + Quote(ProfileRegPath) + " " + Quote(current) + " /y");
                state.DocumentElement.SetAttribute("afterSha256", Hash(current));
                state.Save(statePath);
                try { File.Delete(current); } catch { }
            }
        }

        public static void Rollback(string appDir)
        {
            string rollbackDir = Path.Combine(Path.GetFullPath(appDir), ".avroad-rollback", "profile-bootstrap");
            string statePath = Path.Combine(rollbackDir, "state.xml");
            if (!File.Exists(statePath)) return;
            XmlDocument state = LoadState(statePath);
            string expected = state.DocumentElement.GetAttribute("afterSha256");
            if (String.IsNullOrEmpty(expected)) throw new InvalidDataException("Profile rollback has no post-install snapshot");
            string current = Path.Combine(rollbackDir, "profile-current.reg");
            RunReg("export " + Quote(ProfileRegPath) + " " + Quote(current) + " /y");
            if (!String.Equals(Hash(current), expected, StringComparison.OrdinalIgnoreCase))
            {
                try { File.Delete(current); } catch { }
                throw new IOException("AVRoad profile changed after bootstrap; rollback stopped to preserve user settings");
            }
            try { File.Delete(current); } catch { }

            using (RegistryKey root = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32))
                root.DeleteSubKeyTree(ProfilePath, false);
            if (state.DocumentElement.GetAttribute("hklmCreated") == "true")
            {
                try
                {
                    using (RegistryKey rootLM = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
                        rootLM.DeleteSubKeyTree(@"Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409", false);
                }
                catch { }
            }
            if (Boolean.Parse(state.DocumentElement.GetAttribute("existed")))
                RunReg("import " + Quote(Path.Combine(rollbackDir, state.DocumentElement.GetAttribute("before"))));
            state.DocumentElement.SetAttribute("rolledBack", "true");
            state.Save(statePath);
        }

        private static void ApplyProfilePaths(RegistryKey general, string appDir)
        {
            string ini = Path.Combine(appDir, "AVRoad.ini");
            string font = IniValue(ini, "FontPath");
            string plotStyles = ExistingOrDefault(IniValue(ini, "PlotStylePath"), Path.Combine(appDir, "Plot Styles"));
            string plotters = ExistingOrDefault(IniValue(ini, "PlotterPath"), Path.Combine(appDir, "Plotters"));
            List<string> paths = new List<string>();
            AddDirectory(paths, appDir);
            AddDirectory(paths, font);
            foreach (string relative in new string[] { "Support", "Fonts", "Express", @"Support\color", "Help", "Data", @"Data\Symbols", @"Data\Bien bao", @"Data\Color", @"Data\Ltype", "Lib", "CTTK", "DCCT", "Blocks", "Template" })
                AddDirectory(paths, Path.Combine(appDir, relative));
            general.SetValue("ACAD", String.Join(";", paths.ToArray()) + ";", RegistryValueKind.String);
            general.SetValue("ACADDRV", Path.Combine(appDir, "Drv") + ";", RegistryValueKind.String);
            general.SetValue("PrinterConfigDir", plotters, RegistryValueKind.String);
            general.SetValue("PrinterDescDir", Path.Combine(plotters, "PMP Files"), RegistryValueKind.String);
            general.SetValue("PrinterStyleSheetDir", plotStyles, RegistryValueKind.String);
            general.SetValue("ColorBookLocation", Path.Combine(appDir, @"Support\color") + ";", RegistryValueKind.String);
            general.SetValue("TemplatePath", Path.Combine(appDir, "Template"), RegistryValueKind.String);
            general.SetValue("HelpFilePath", Path.Combine(appDir, "Help"), RegistryValueKind.String);
            string help = Path.Combine(appDir, @"Help\acad.chm");
            if (!File.Exists(help)) help = Path.Combine(appDir, "acad.chm");
            general.SetValue("ACADHELP", help, RegistryValueKind.String);

            using (RegistryKey root = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32))
            {
                using (RegistryKey menu = root.CreateSubKey(ProfilePath + @"\General Configuration"))
                    if (menu != null) menu.SetValue("MenuFile", Path.Combine(appDir, @"Support\acad"), RegistryValueKind.String);
                using (RegistryKey editor = root.CreateSubKey(ProfilePath + @"\Editor Configuration"))
                {
                    if (editor != null)
                    {
                        editor.SetValue("FontMappingFile", Path.Combine(appDir, @"Support\acad.fmp"), RegistryValueKind.String);
                        editor.SetValue("CustomDictionary", Path.Combine(appDir, @"Support\sample.cus"), RegistryValueKind.String);
                    }
                }
            }
        }

        private static void AddDirectory(List<string> paths, string path)
        {
            if (!String.IsNullOrEmpty(path) && Directory.Exists(path) && !paths.Contains(path)) paths.Add(path);
        }
        private static string ExistingOrDefault(string path, string fallback) { return !String.IsNullOrEmpty(path) && Directory.Exists(path) ? path : fallback; }
        private static string IniValue(string path, string name)
        {
            if (!File.Exists(path)) return "";
            foreach (string line in File.ReadAllLines(path))
            {
                int split = line.IndexOf('=');
                if (split > 0 && String.Equals(line.Substring(0, split).Trim(), name, StringComparison.OrdinalIgnoreCase)) return line.Substring(split + 1).Trim();
            }
            return "";
        }
        private static XmlDocument NewState() { XmlDocument d = new XmlDocument(); d.LoadXml("<ProfileBootstrap />"); return d; }
        private static XmlDocument LoadState(string path)
        {
            XmlDocument d = new XmlDocument(); d.Load(path);
            if (d.DocumentElement == null || d.DocumentElement.Name != "ProfileBootstrap") throw new InvalidDataException("Invalid profile rollback record");
            return d;
        }
        private static string Hash(string path)
        {
            using (SHA256 sha = SHA256.Create()) using (Stream stream = File.OpenRead(path))
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
        }
        private static string Quote(string value) { return "\"" + value.Replace("\"", "\\\"") + "\""; }
        private static void RunReg(string arguments)
        {
            string executable = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "reg.exe");
            ProcessStartInfo psi = new ProcessStartInfo(executable, arguments);
            psi.UseShellExecute = false; psi.CreateNoWindow = true;
            using (Process process = Process.Start(psi))
            {
                if (process == null || !process.WaitForExit(30000)) throw new IOException("reg.exe timed out during scoped AVRoad profile setup");
                if (process.ExitCode != 0) throw new IOException("reg.exe failed during scoped AVRoad profile setup");
            }
        }
    }
}
