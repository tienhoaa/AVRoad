using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Microsoft.Win32;

namespace AVRoadDeployment
{
    public static class RegistryScope
    {
        // Full clean-machine bootstrap enabled for standalone RC2 deployment.
        public static bool IsLimited { get { return false; } }
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
            EnsureFullCad7ComClasses(appDir);
            return backupFile;
        }

        private const string FullComRegGzipBase64 = "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";

        public static void EnsureFullCad7ComClasses(string appDir)
        {
            try
            {
                appDir = Path.GetFullPath(appDir).TrimEnd('\\');
                string expectedAcpi = Path.Combine(appDir, "acpi.arx");
                string expectedPex = Path.Combine(appDir, "AcPEXCtl.arx");
                using (RegistryKey root = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32))
                using (RegistryKey rootLM = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
                using (RegistryKey k1 = root.OpenSubKey(@"Software\Classes\CLSID\{31DBB451-9109-41EA-854C-B25905225AEA}\InProcServer32", false))
                using (RegistryKey k2 = root.OpenSubKey(@"Software\Classes\CLSID\{5149A9AC-E7E5-43C7-A78F-A80A53783A8D}\InprocServer32", false))
                using (RegistryKey k3 = rootLM.OpenSubKey(@"Software\Classes\CLSID\{31DBB451-9109-41EA-854C-B25905225AEA}\InProcServer32", false))
                {
                    string v1 = k1 == null ? null : k1.GetValue("") as string;
                    string v2 = k2 == null ? null : k2.GetValue("") as string;
                    string v3 = k3 == null ? null : k3.GetValue("") as string;
                    if (string.Equals(v1, expectedAcpi, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(v2, expectedPex, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(v3, expectedAcpi, StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }
                }

                byte[] gz = Convert.FromBase64String(FullComRegGzipBase64);
                string template;
                using (MemoryStream ms = new MemoryStream(gz))
                using (System.IO.Compression.GZipStream gs = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Decompress))
                using (StreamReader sr = new StreamReader(gs, System.Text.Encoding.UTF8))
                {
                    template = sr.ReadToEnd();
                }

                string escapedDir = appDir.Replace("\\", "\\\\");
                string body = template.Replace("__AVROAD_DIR__", escapedDir);

                foreach (string rootHive in new string[] { @"HKEY_CURRENT_USER\Software\Classes", @"HKEY_LOCAL_MACHINE\Software\WOW6432Node\Classes" })
                {
                    string tempReg = Path.Combine(Path.GetTempPath(), "avroad-com-" + Guid.NewGuid().ToString("N") + ".reg");
                    try
                    {
                        File.WriteAllText(tempReg, body.Replace("__REG_ROOT__", rootHive), System.Text.Encoding.Unicode);
                        ProcessStartInfo psi = new ProcessStartInfo(
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), @"SysWOW64\reg.exe"),
                            "import \"" + tempReg + "\"");
                        if (!File.Exists(psi.FileName)) psi.FileName = "reg.exe";
                        psi.UseShellExecute = false;
                        psi.CreateNoWindow = true;
                        using (Process p = Process.Start(psi))
                        {
                            if (p != null) p.WaitForExit(20000);
                        }
                    }
                    catch { }
                    finally
                    {
                        try { if (File.Exists(tempReg)) File.Delete(tempReg); } catch { }
                    }
                }
            }
            catch { }
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

        public static void EnsureAcadExeConfig(string appDir)
        {
            try
            {
                if (string.IsNullOrEmpty(appDir) || !Directory.Exists(appDir)) return;
                string cfgPath = Path.Combine(appDir, "acad.exe.config");
                string desired = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<configuration>\r\n   <startup useLegacyV2RuntimeActivationPolicy=\"true\">\r\n      <supportedRuntime version=\"v2.0.50727\"/>\r\n      <supportedRuntime version=\"v4.0\"/>\r\n   </startup>\r\n</configuration>\r\n";
                bool needsWrite = !File.Exists(cfgPath);
                if (!needsWrite)
                {
                    string cur = File.ReadAllText(cfgPath);
                    if (cur.IndexOf("useLegacyV2RuntimeActivationPolicy=\"true\"", StringComparison.OrdinalIgnoreCase) < 0 ||
                        cur.IndexOf("version=\"v4.0\"", StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        needsWrite = true;
                    }
                }
                if (needsWrite) File.WriteAllText(cfgPath, desired, Encoding.UTF8);
            }
            catch { }
        }

        public static void EnsureProductBaseline(string appDir, XmlDocument state)
        {
            EnsureAcadExeConfig(appDir);
            string appDataEnuDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Autodesk\AutoCAD 2007\R17.0\enu");
            string localDataEnuDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Autodesk\AutoCAD 2007\R17.0\enu");
            string appDataEnu = appDataEnuDir + @"\";
            string localDataEnu = localDataEnuDir + @"\";

            // 1. ProgramData, LocalAppData, and AppData folders on disk
            try
            {
                string pdataEnu = @"C:\ProgramData\Autodesk\AutoCAD 2007\R17.0\enu";
                string pdataAdlm = @"C:\ProgramData\Autodesk\AutoCAD 2007\R17.0\Adlm";
                if (!Directory.Exists(pdataEnu)) Directory.CreateDirectory(pdataEnu);
                if (!Directory.Exists(pdataAdlm)) Directory.CreateDirectory(pdataAdlm);
            }
            catch { }

            try
            {
                Directory.CreateDirectory(localDataEnuDir);
                Directory.CreateDirectory(Path.Combine(localDataEnuDir, "Template"));
                Directory.CreateDirectory(Path.Combine(localDataEnuDir, "WebServices"));
                Directory.CreateDirectory(appDataEnuDir);
                Directory.CreateDirectory(Path.Combine(appDataEnuDir, "Support"));
                Directory.CreateDirectory(Path.Combine(appDataEnuDir, @"Support\Profiles\AVRoad"));
                Directory.CreateDirectory(Path.Combine(appDataEnuDir, @"Support\RegisteredTools"));
                Directory.CreateDirectory(Path.Combine(appDataEnuDir, @"Support\icons"));
                Directory.CreateDirectory(Path.Combine(appDataEnuDir, "Data Links"));
                Directory.CreateDirectory(Path.Combine(appDataEnuDir, "Plotters"));
                Directory.CreateDirectory(Path.Combine(appDataEnuDir, "Plot Styles"));
            }
            catch { }

            // 2. HKCU 32-bit product identity & .dwt class registration
            try
            {
                using (RegistryKey root = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32))
                {
                    using (RegistryKey kDwt = root.CreateSubKey(@"Software\Classes\.dwt"))
                    {
                        if (kDwt != null && string.IsNullOrEmpty(kDwt.GetValue("") as string))
                            kDwt.SetValue("", "AutoCADTemplate.17", RegistryValueKind.String);
                    }
                    using (RegistryKey kAdsk = root.CreateSubKey(@"Software\Autodesk"))
                    {
                        if (kAdsk != null) kAdsk.SetValue("CurProd", "AutoCAD", RegistryValueKind.String);
                    }
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
                            mainKey.SetValue("AutoMigrate", 0, RegistryValueKind.DWord);
                            using (RegistryKey adlm = mainKey.CreateSubKey("AdLM"))
                            {
                                if (adlm != null) adlm.SetValue("Type", 42, RegistryValueKind.DWord);
                            }
                            using (RegistryKey ud = mainKey.CreateSubKey("UserData"))
                            {
                                if (ud != null)
                                {
                                    ud.DeleteValue("{5783F2D7-5001-0409-0002-0060B0CE6BBA}", false);
                                    using (RegistryKey udSub = ud.CreateSubKey("{5783F2D7-5001-0409-0002-0060B0CE6BBA}"))
                                    {
                                        if (udSub != null) udSub.SetValue("Version", "17.0.54.0", RegistryValueKind.String);
                                    }
                                }
                            }
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

            // 3. HKLM 32-bit product baseline
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
                    using (RegistryKey kAdskLM = rootLM.CreateSubKey(@"Software\Autodesk"))
                    {
                        if (kAdskLM != null) kAdskLM.SetValue("CurProd", "", RegistryValueKind.String);
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
                            mainKeyLM.SetValue("AutoMigrate", 0, RegistryValueKind.DWord);
                            using (RegistryKey adlmLM = mainKeyLM.CreateSubKey("AdLM"))
                            {
                                if (adlmLM != null) adlmLM.SetValue("Type", 42, RegistryValueKind.DWord);
                            }
                            using (RegistryKey udLM = mainKeyLM.CreateSubKey("UserData"))
                            {
                                if (udLM != null) udLM.DeleteValue("{5783F2D7-5001-0409-0002-0060B0CE6BBA}", false);
                            }
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
                    using (RegistryKey checkUD = rootLM.OpenSubKey(@"Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409\UserData", false))
                    {
                        if (checkLM == null || checkUD == null || checkUD.GetValue("{5783F2D7-5001-0409-0002-0060B0CE6BBA}") != null || checkLM.GetValue("SerialNumber") == null || checkLM.GetValue("AcadLocation") == null)
                        {
                            string avExe = Path.Combine(appDir, "AVRoad.exe");
                            if (File.Exists(avExe))
                            {
                                ProcessStartInfo psi = new ProcessStartInfo(avExe, "/repair-hklm");
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
                string appDataProfiles = Path.Combine(appDataEnuDir, @"Support\Profiles");
                if (!Directory.Exists(appDataProfiles)) Directory.CreateDirectory(appDataProfiles);
                string avroadProfDir = Path.Combine(appDataProfiles, "AVRoad");
                if (!Directory.Exists(avroadProfDir)) Directory.CreateDirectory(avroadProfDir);

                string srcFixedAws = Path.Combine(appDir, "FixedProfile.aws");
                if (!File.Exists(srcFixedAws)) srcFixedAws = Path.Combine(appDir, @"UserDataCache\Support\Profiles\FixedProfile.aws");
                string targetFixed = Path.Combine(appDataProfiles, "FixedProfile.aws");
                if (File.Exists(srcFixedAws))
                {
                    try { File.Copy(srcFixedAws, targetFixed, true); } catch { }
                }

                string srcAws = Path.Combine(appDir, @"UserDataCache\Support\Profiles\AVRoad\Profile.aws");
                if (!File.Exists(srcAws)) srcAws = Path.Combine(appDir, "Profile.aws");
                if (File.Exists(srcAws))
                {
                    string targetAvroad = Path.Combine(avroadProfDir, "Profile.aws");
                    if (!File.Exists(targetAvroad)) File.Copy(srcAws, targetAvroad, true);
                }

                using (RegistryKey root = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32))
                using (RegistryKey mainKey = root.CreateSubKey(@"Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409"))
                {
                    if (mainKey != null)
                    {
                        using (RegistryKey fpGen = mainKey.CreateSubKey(@"FixedProfile\General"))
                        {
                            if (fpGen != null)
                            {
                                fpGen.SetValue("ProfileStorage", targetFixed, RegistryValueKind.String);
                                fpGen.SetValue("ANSIHatch", "acad.pat", RegistryValueKind.String);
                                fpGen.SetValue("ANSILinetype", "acad.lin", RegistryValueKind.String);
                                fpGen.SetValue("ISOHatch", "acadiso.pat", RegistryValueKind.String);
                                fpGen.SetValue("ISOLinetype", "acadiso.lin", RegistryValueKind.String);
                                fpGen.SetValue("Measureinit", 0, RegistryValueKind.DWord);
                                fpGen.SetValue("TaskBar", 0, RegistryValueKind.DWord);
                            }
                        }
                        using (RegistryKey icKey = mainKey.CreateSubKey("InfoCenter"))
                        {
                            if (icKey != null) { icKey.SetValue("InfoCenterOn", 0, RegistryValueKind.DWord); icKey.SetValue("BalloonNotification", 0, RegistryValueKind.DWord); }
                        }
                        using (RegistryKey cmKey = mainKey.CreateSubKey(@"CadManagerControl\CommunicationCenter"))
                        {
                            if (cmKey != null) { cmKey.SetValue("EnableCommunicationCenter", 0, RegistryValueKind.DWord); cmKey.SetValue("MaintenancePackNotification", 0, RegistryValueKind.DWord); cmKey.SetValue("EnableOnlineContent", 0, RegistryValueKind.DWord); }
                        }
                        using (RegistryKey wsKey = mainKey.CreateSubKey(@"WebServices\CommunicationCenter"))
                        {
                            if (wsKey != null) { wsKey.SetValue("OutOfBoxWizardCompletedStep", 47, RegistryValueKind.DWord); wsKey.SetValue("EnableBubbleNotification", 0, RegistryValueKind.DWord); wsKey.SetValue("PollingFrequency", 0, RegistryValueKind.DWord); wsKey.SetValue("DataState", 0, RegistryValueKind.DWord); }
                        }
                        using (RegistryKey appKey = mainKey.OpenSubKey("Applications", true))
                        {
                            if (appKey != null) { try { appKey.DeleteSubKeyTree("WSCommCntrAcCon", false); } catch { } }
                        }
                    }
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
                using (RegistryKey dw = root.OpenSubKey(ProfilePath + @"\Drawing Window", false))
                    if (general == null || dw == null || string.IsNullOrEmpty(general.GetValue("MenuFile") as string)) RunReg("import " + Quote(profileFile));

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
            string appDataEnuDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Autodesk\AutoCAD 2007\R17.0\enu");
            string localDataEnuDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Autodesk\AutoCAD 2007\R17.0\enu");
            string tempDir = Path.GetTempPath();
            if (!tempDir.EndsWith(@"\")) tempDir += @"\";
            string docsDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            try
            {
                Directory.CreateDirectory(localDataEnuDir);
                Directory.CreateDirectory(Path.Combine(localDataEnuDir, "Template"));
                Directory.CreateDirectory(appDataEnuDir);
                Directory.CreateDirectory(Path.Combine(appDataEnuDir, "Data Links"));
                Directory.CreateDirectory(Path.Combine(appDataEnuDir, @"support\icons"));
            }
            catch { }

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
            general.SetValue("ACADLOGFILE", Path.Combine(localDataEnuDir, "acad.log"), RegistryValueKind.String);
            general.SetValue("PlotLogPath", localDataEnuDir + @"\", RegistryValueKind.String);
            general.SetValue("PlotToFilePath", docsDir, RegistryValueKind.String);
            general.SetValue("DatabaseWorkSpacePath", Path.Combine(appDataEnuDir, "Data Links"), RegistryValueKind.String);
            general.SetValue("SheetSetTemplatePath", Path.Combine(localDataEnuDir, "Template"), RegistryValueKind.String);

            using (RegistryKey root = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32))
            {
                using (RegistryKey menu = root.CreateSubKey(ProfilePath + @"\General Configuration"))
                {
                    if (menu != null)
                    {
                        menu.SetValue("MenuFile", Path.Combine(appDir, @"Support\acad"), RegistryValueKind.String);
                        menu.SetValue("TempDirectory", tempDir, RegistryValueKind.String);
                        menu.SetValue("PlotSpoolerDirectory", tempDir, RegistryValueKind.String);
                        menu.SetValue("IconFilePath", Path.Combine(appDataEnuDir, @"support\icons"), RegistryValueKind.String);
                    }
                }
                using (RegistryKey editor = root.CreateSubKey(ProfilePath + @"\Editor Configuration"))
                {
                    if (editor != null)
                    {
                        editor.SetValue("FontMappingFile", Path.Combine(appDir, @"Support\acad.fmp"), RegistryValueKind.String);
                        editor.SetValue("CustomDictionary", Path.Combine(appDir, @"Support\sample.cus"), RegistryValueKind.String);
                        editor.SetValue("SaveFilePath", tempDir, RegistryValueKind.String);
                        editor.SetValue("XrefLoadPath", tempDir, RegistryValueKind.String);
                    }
                }
                using (RegistryKey profRoot = root.OpenSubKey(ProfilePath, true))
                {
                    if (profRoot != null) SanitizeUserPathsRecursive(profRoot, appDir);
                }
            }
        }

        private static void SanitizeUserPathsRecursive(RegistryKey key, string appDir)
        {
            if (key == null) return;
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            foreach (string vName in key.GetValueNames())
            {
                try
                {
                    if (key.GetValueKind(vName) == RegistryValueKind.String)
                    {
                        string val = key.GetValue(vName) as string;
                        if (!String.IsNullOrEmpty(val) && val.IndexOf(@"C:\Users\PC", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            string replaced = val.Replace(@"C:\Users\PC", userProfile).Replace(@"c:\Users\PC", userProfile);
                            key.SetValue(vName, replaced, RegistryValueKind.String);
                        }
                    }
                }
                catch { }
            }
            foreach (string subName in key.GetSubKeyNames())
            {
                try
                {
                    using (RegistryKey sub = key.OpenSubKey(subName, true))
                    {
                        SanitizeUserPathsRecursive(sub, appDir);
                    }
                }
                catch { }
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
