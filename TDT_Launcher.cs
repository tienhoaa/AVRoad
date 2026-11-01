using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace TDTLauncher
{
    static class Program
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowText(IntPtr hWnd, string lpString);

        [DllImport("user32.dll")]
        static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
        delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        const uint WM_SETICON = 0x80;
        const int ICON_SMALL = 0;
        const int ICON_BIG = 1;

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string appDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');

            // 1. Configure Registry Profile and Support Search Paths
            PrepareTdtProfile(appDir);

            // 2. Launch AutoCAD with WorkingDirectory strictly set to appDir (Safengine Rlthap.rar fix)
            string acadExe = Path.Combine(appDir, "acad.exe");
            if (!File.Exists(acadExe))
            {
                MessageBox.Show("Không tìm thấy file AutoCAD (acad.exe) trong thư mục: " + appDir, "TDT Solution 9.1", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = acadExe;
            psi.Arguments = "/nologo /p TDT_Solution";
            psi.WorkingDirectory = appDir;
            psi.UseShellExecute = false;

            Process acadProc = null;
            try
            {
                acadProc = Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khởi động TDT Solution 9.1: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (acadProc != null)
            {
                uint targetPid = (uint)acadProc.Id;
                string icoPath = Path.Combine(appDir, "TDT.ico");
                if (!File.Exists(icoPath)) icoPath = Path.Combine(appDir, "AVRoad.ico");

                IntPtr hIconSmall = IntPtr.Zero;
                IntPtr hIconBig = IntPtr.Zero;
                if (File.Exists(icoPath))
                {
                    try
                    {
                        hIconSmall = new Icon(icoPath, 16, 16).Handle;
                        hIconBig = new Icon(icoPath, 32, 32).Handle;
                    }
                    catch { }
                }

                // Brand window title cleanly
                Thread monitorThread = new Thread(() =>
                {
                    for (int i = 0; i < 200; i++)
                    {
                        Thread.Sleep(100);
                        if (acadProc.HasExited) break;

                        EnumWindows((hwnd, lparam) =>
                        {
                            uint pid;
                            GetWindowThreadProcessId(hwnd, out pid);
                            if (pid == targetPid && IsWindowVisible(hwnd))
                            {
                                System.Text.StringBuilder sb = new System.Text.StringBuilder(512);
                                GetWindowText(hwnd, sb, sb.Capacity);
                                string title = sb.ToString();

                                if (!string.IsNullOrEmpty(title))
                                {
                                    if (title.Contains("AutoCAD 2007") || title.Contains("AutoCAD"))
                                    {
                                        string newTitle = title.Replace("AutoCAD 2007", "TDT Solution 9.1").Replace("AutoCAD", "TDT Solution 9.1");
                                        if (newTitle != title) SetWindowText(hwnd, newTitle);

                                        if (hIconSmall != IntPtr.Zero) SendMessage(hwnd, WM_SETICON, (IntPtr)ICON_SMALL, hIconSmall);
                                        if (hIconBig != IntPtr.Zero) SendMessage(hwnd, WM_SETICON, (IntPtr)ICON_BIG, hIconBig);
                                    }
                                }
                            }
                            return true;
                        }, IntPtr.Zero);
                    }
                });
                monitorThread.IsBackground = true;
                monitorThread.Start();

                try
                {
                    acadProc.WaitForExit();
                }
                catch { }
            }
        }

        static void PrepareTdtProfile(string appDir)
        {
            try
            {
                // Register TDT Solution 9.1 install path
                using (RegistryKey tdtKey = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\WOW6432Node\TDT\TDT Solution 9.1"))
                {
                    if (tdtKey != null)
                    {
                        tdtKey.SetValue("Path", appDir + "\\", RegistryValueKind.String);
                        tdtKey.SetValue("Version", "1.0.48", RegistryValueKind.String);
                    }
                }
            }
            catch { }

            try
            {
                using (RegistryKey rk = Registry.CurrentUser.CreateSubKey(@"Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409"))
                {
                    if (rk != null)
                    {
                        rk.SetValue("SerialNumber", "000-00000000");
                        rk.SetValue("Location", appDir);
                        rk.SetValue("ProductName", "TDT Solution 9.1");
                        rk.SetValue("Release", "17.0");
                        rk.SetValue("CurVer", "ACAD-5001:409");
                    }
                }
            }
            catch { }

            // Ensure AcOpm demand-loading is configured for Ctrl + 1
            try
            {
                using (RegistryKey opmKey = Registry.CurrentUser.CreateSubKey(@"Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409\Applications\AcOpm"))
                {
                    if (opmKey != null)
                    {
                        opmKey.SetValue("LOADCTRLS", 14, RegistryValueKind.DWord);
                        opmKey.SetValue("LOADER", Path.Combine(appDir, "acopm.arx"), RegistryValueKind.String);
                        opmKey.SetValue("DESCRIPTION", "AutoCAD Object Property Manager", RegistryValueKind.String);
                        using (RegistryKey cmdKey = opmKey.CreateSubKey("Commands"))
                        {
                            if (cmdKey != null)
                            {
                                cmdKey.SetValue("PROPERTIES", "PROPERTIES", RegistryValueKind.String);
                                cmdKey.SetValue("PROPERTIESCLOSE", "PROPERTIESCLOSE", RegistryValueKind.String);
                            }
                        }
                    }
                }
            }
            catch { }

            // Prepare complete ACAD support search paths so DauCo.dwg, DTKTN.cfg, TKAD, DAODAP always work
            string profilesKeyPath = @"Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409\Profiles";
            try
            {
                using (RegistryKey profilesKey = Registry.CurrentUser.OpenSubKey(profilesKeyPath, true))
                {
                    if (profilesKey != null)
                    {
                        string supportPath = Path.Combine(appDir, "Support");
                        string fontsPath = Path.Combine(appDir, "Fonts");
                        string expressPath = Path.Combine(appDir, "Express");
                        string colorPath = Path.Combine(appDir, @"Support\color");
                        string helpPath = Path.Combine(appDir, "Help");
                        string drvPath = Path.Combine(appDir, "Drv");
                        string templatePath = Path.Combine(appDir, "Template");

                        string cleanAcad = appDir + ";" +
                                           supportPath + ";" + fontsPath + ";" + expressPath + ";" + colorPath + ";" + helpPath + ";" +
                                           Path.Combine(appDir, "Data") + ";" +
                                           Path.Combine(appDir, @"Data\Symbols") + ";" +
                                           Path.Combine(appDir, @"Data\Bien bao") + ";" +
                                           Path.Combine(appDir, @"Data\Color") + ";" +
                                           Path.Combine(appDir, @"Data\Ltype") + ";" +
                                           Path.Combine(appDir, "Lib") + ";" +
                                           Path.Combine(appDir, "CTTK") + ";" +
                                           Path.Combine(appDir, "DCCT") + ";" +
                                           Path.Combine(appDir, "Blocks") + ";" +
                                           Path.Combine(appDir, "Template");

                        using (RegistryKey genKey = profilesKey.CreateSubKey(@"TDT_Solution\General"))
                        {
                            if (genKey != null)
                            {
                                genKey.SetValue("ACAD", cleanAcad);
                                genKey.SetValue("ACADDRV", drvPath);
                                genKey.SetValue("HelpFilePath", helpPath);
                                genKey.SetValue("TemplatePath", templatePath);
                            }
                        }
                        foreach (string pName in profilesKey.GetSubKeyNames())
                        {
                            try
                            {
                                using (RegistryKey subGen = profilesKey.OpenSubKey(pName + @"\General", true))
                                {
                                    if (subGen != null)
                                    {
                                        subGen.SetValue("ACAD", cleanAcad);
                                        subGen.SetValue("ACADDRV", drvPath);
                                    }
                                }
                            }
                            catch { }
                        }
                        using (RegistryKey cfgKey = profilesKey.CreateSubKey(@"TDT_Solution\General Configuration"))
                        {
                            if (cfgKey != null)
                            {
                                cfgKey.SetValue("MenuFile", Path.Combine(supportPath, "acad"));
                            }
                        }
                    }
                }
            }
            catch { }
        }
    }
}
