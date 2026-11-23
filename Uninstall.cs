using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace AVRoadUninstaller
{
    static class Program
    {
        private static string NormalizeCadDir(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return null;
            try
            {
                string expanded = Environment.ExpandEnvironmentVariables(raw).Trim().Trim('"');
                if (string.IsNullOrEmpty(expanded)) return null;
                if (expanded.EndsWith("acad.exe", StringComparison.OrdinalIgnoreCase))
                {
                    if (File.Exists(expanded)) return Path.GetDirectoryName(Path.GetFullPath(expanded)).TrimEnd('\\');
                    return null;
                }
                string dir = expanded.TrimEnd('\\', '/');
                if (File.Exists(Path.Combine(dir, "acad.exe"))) return Path.GetFullPath(dir).TrimEnd('\\');
            }
            catch { }
            return null;
        }

        private static string ProbeCadHostFromRegistry(RegistryHive hive, RegistryView view)
        {
            try
            {
                using (RegistryKey root = RegistryKey.OpenBaseKey(hive, view))
                using (RegistryKey r17 = root.OpenSubKey(@"Software\Autodesk\AutoCAD\R17.0", false))
                {
                    if (r17 == null) return null;
                    List<string> subs = new List<string>();
                    string curVer = r17.GetValue("CurVer") as string;
                    if (!string.IsNullOrEmpty(curVer)) subs.Add(curVer);
                    if (!subs.Contains("ACAD-5001:409")) subs.Add("ACAD-5001:409");
                    foreach (string s in r17.GetSubKeyNames()) { if (!subs.Contains(s)) subs.Add(s); }
                    foreach (string sub in subs)
                    {
                        using (RegistryKey pk = r17.OpenSubKey(sub, false))
                        {
                            if (pk == null) continue;
                            foreach (string valName in new string[] { "Location", "AcadLocation" })
                            {
                                string cand = NormalizeCadDir(pk.GetValue(valName) as string);
                                if (!string.IsNullOrEmpty(cand)) return cand;
                            }
                        }
                    }
                }
            }
            catch { }
            return null;
        }

        private static string ResolveCadHostDir(string appDir)
        {
            string iniPath = Path.Combine(appDir, "AVRoad.ini");
            if (File.Exists(iniPath))
            {
                try
                {
                    string currentSection = "";
                    string preferred = null;
                    string fallback = null;
                    foreach (string raw in File.ReadAllLines(iniPath))
                    {
                        string line = raw.Trim();
                        if (line.Length == 0 || line.StartsWith(";") || line.StartsWith("#")) continue;
                        if (line.StartsWith("[") && line.EndsWith("]"))
                        {
                            currentSection = line.Substring(1, line.Length - 2).Trim();
                            continue;
                        }
                        int eq = line.IndexOf('=');
                        if (eq <= 0) continue;
                        string k = line.Substring(0, eq).Trim();
                        string v = line.Substring(eq + 1).Trim();
                        if (string.Equals(k, "AutoCADPath", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(v))
                        {
                            if (string.Equals(currentSection, "Settings", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(currentSection, "Paths", StringComparison.OrdinalIgnoreCase))
                            {
                                preferred = v;
                                break;
                            }
                            if (fallback == null) fallback = v;
                        }
                    }
                    string cand = NormalizeCadDir(!string.IsNullOrEmpty(preferred) ? preferred : fallback);
                    if (!string.IsNullOrEmpty(cand)) return cand;
                }
                catch { }
            }

            foreach (string cand in new string[] {
                ProbeCadHostFromRegistry(RegistryHive.LocalMachine, RegistryView.Registry32),
                ProbeCadHostFromRegistry(RegistryHive.LocalMachine, RegistryView.Registry64),
                ProbeCadHostFromRegistry(RegistryHive.CurrentUser, RegistryView.Registry32),
                ProbeCadHostFromRegistry(RegistryHive.CurrentUser, RegistryView.Default),
                NormalizeCadDir(@"C:\Program Files (x86)\AutoCAD 2007"),
                NormalizeCadDir(@"C:\Program Files\AutoCAD 2007")
            })
            {
                if (!string.IsNullOrEmpty(cand)) return cand;
            }
            return appDir;
        }

        private static void CleanupAvRoadHkcuRegistry()
        {
            foreach (RegistryView view in new RegistryView[] { RegistryView.Registry32, RegistryView.Default })
            {
                try
                {
                    using (RegistryKey hkcu = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, view))
                    {
                        List<string> vers = new List<string>();
                        vers.Add("ACAD-5001:409");
                        using (RegistryKey r17 = hkcu.OpenSubKey(@"Software\Autodesk\AutoCAD\R17.0", false))
                        {
                            if (r17 != null)
                            {
                                string cv = r17.GetValue("CurVer") as string;
                                if (!string.IsNullOrEmpty(cv) && !vers.Contains(cv)) vers.Add(cv);
                            }
                        }

                        foreach (string ver in vers)
                        {
                            string profilesPath = @"Software\Autodesk\AutoCAD\R17.0\" + ver + @"\Profiles";
                            try
                            {
                                using (RegistryKey profKey = hkcu.OpenSubKey(profilesPath, true))
                                {
                                    if (profKey != null)
                                    {
                                        try { profKey.DeleteSubKeyTree("AVRoad", false); } catch { }
                                        string active = profKey.GetValue("") as string;
                                        if (string.Equals(active, "AVRoad", StringComparison.OrdinalIgnoreCase))
                                        {
                                            string fallbackProfile = "<<Unnamed Profile>>";
                                            string[] remaining = profKey.GetSubKeyNames();
                                            if (Array.IndexOf(remaining, fallbackProfile) < 0 && remaining.Length > 0)
                                            {
                                                fallbackProfile = remaining[0];
                                            }
                                            profKey.SetValue("", fallbackProfile, RegistryValueKind.String);
                                        }
                                    }
                                }
                            }
                            catch { }

                            string appsPath = @"Software\Autodesk\AutoCAD\R17.0\" + ver + @"\Applications";
                            try
                            {
                                using (RegistryKey appsKey = hkcu.OpenSubKey(appsPath, true))
                                {
                                    if (appsKey != null)
                                    {
                                        try { appsKey.DeleteSubKeyTree("AVRoadUI21", false); } catch { }
                                        try { appsKey.DeleteSubKeyTree("TDTObjects", false); } catch { }
                                    }
                                }
                            }
                            catch { }
                        }

                        try
                        {
                            using (RegistryKey odbxApps = hkcu.OpenSubKey(@"Software\Autodesk\ObjectDBX\R17.0\Applications", true))
                            {
                                if (odbxApps != null) odbxApps.DeleteSubKeyTree("TDTObjects", false);
                            }
                        }
                        catch { }

                        try
                        {
                            using (RegistryKey axKey = hkcu.OpenSubKey(@"Software\Autodesk\ObjectDBX\R17.0\ActiveXCLSID", true))
                            {
                                if (axKey != null)
                                {
                                    axKey.DeleteValue("TdtDbMeasurePointObj", false);
                                    axKey.DeleteValue("TdtDbPointObj", false);
                                }
                            }
                        }
                        catch { }
                    }
                }
                catch { }
            }
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string appDir = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory).TrimEnd('\\');
            string cadHostDir = ResolveCadHostDir(appDir);
            bool isLiteMode = !string.IsNullOrEmpty(cadHostDir) &&
                              !string.Equals(appDir, cadHostDir, StringComparison.OrdinalIgnoreCase);

            DialogResult dr = MessageBox.Show(
                "Bạn có chắc chắn muốn gỡ cài đặt AVRoad 2026 Solution khỏi máy tính không?",
                "Xác Nhận Gỡ Cài Đặt",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (dr != DialogResult.Yes) return;

            // 1. Close CAD processes
            try
            {
                foreach (Process p in Process.GetProcessesByName("acad"))
                {
                    try { p.Kill(); } catch { }
                }
                foreach (Process p in Process.GetProcessesByName("AVRoad"))
                {
                    try { p.Kill(); } catch { }
                }
            }
            catch { }

            // 2. Remove Shortcuts
            try
            {
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string sc1 = Path.Combine(desktop, "AVRoad 2026.lnk");
                string sc2 = Path.Combine(desktop, "AVRoad Settings.lnk");
                if (File.Exists(sc1)) File.Delete(sc1);
                if (File.Exists(sc2)) File.Delete(sc2);

                string startMenu = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
                string avGroup = Path.Combine(startMenu, "AVRoad 2026 Solution");
                if (Directory.Exists(avGroup)) Directory.Delete(avGroup, true);
            }
            catch { }

            // 3. Remove AVRoad HKCU Profile & TDT Registry keys (preserve Host AutoCAD)
            CleanupAvRoadHkcuRegistry();

            // 4. Restore acad.exe.config.avroad.bak in CAD Host directory if present
            if (isLiteMode && !string.IsNullOrEmpty(cadHostDir) && Directory.Exists(cadHostDir))
            {
                try
                {
                    string bakPath = Path.Combine(cadHostDir, "acad.exe.config.avroad.bak");
                    string cfgPath = Path.Combine(cadHostDir, "acad.exe.config");
                    if (File.Exists(bakPath))
                    {
                        File.Copy(bakPath, cfgPath, true);
                        File.Delete(bakPath);
                    }
                }
                catch { }
            }

            // 5. Remove Windows Registry Uninstall Entry
            try { Registry.CurrentUser.DeleteSubKeyTree(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\AVRoad2026", false); } catch { }
            try { Registry.LocalMachine.DeleteSubKeyTree(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\AVRoad2026", false); } catch { }
            try { Registry.LocalMachine.DeleteSubKeyTree(@"Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\AVRoad2026", false); } catch { }

            // 6. Delete AVRoad app directory only (NEVER delete Host AutoCAD 2007 directory!)
            bool isSafeToDeleteAppDir =
                (!isLiteMode || !string.Equals(appDir, cadHostDir, StringComparison.OrdinalIgnoreCase)) &&
                (File.Exists(Path.Combine(appDir, "AVRoad.exe")) ||
                 File.Exists(Path.Combine(appDir, "AVRoad.ini")) ||
                 File.Exists(Path.Combine(appDir, @"UI21\AVRoad.SinglePanel21.dll")));

            if (isSafeToDeleteAppDir)
            {
                string batchPath = Path.Combine(Path.GetTempPath(), "remove_avroad.bat");
                try
                {
                    using (StreamWriter sw = new StreamWriter(batchPath, false, System.Text.Encoding.Default))
                    {
                        sw.WriteLine("@echo off");
                        sw.WriteLine("cd /d \"%TEMP%\"");
                        sw.WriteLine("timeout /t 2 /nobreak >nul");
                        sw.WriteLine("rmdir /s /q \"" + appDir + "\"");
                        sw.WriteLine("del \"%~f0\"");
                    }

                    ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/c \"" + batchPath + "\"");
                    psi.WorkingDirectory = Path.GetTempPath();
                    psi.CreateNoWindow = true;
                    psi.UseShellExecute = false;
                    Process.Start(psi);
                }
                catch { }
            }

            MessageBox.Show("Đã gỡ cài đặt hoàn tất AVRoad 2026 Solution khỏi máy tính của bạn!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
