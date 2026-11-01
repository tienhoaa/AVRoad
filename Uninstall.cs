using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace AVRoadUninstaller
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string appDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');

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

            // 3. Remove Windows Registry Uninstall Entry
            try
            {
                Registry.CurrentUser.DeleteSubKeyTree(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\AVRoad2026", false);
            }
            catch { }

            // 4. Delete app directory via a self-deleting cmd batch
            string batchPath = Path.Combine(Path.GetTempPath(), "remove_avroad.bat");
            try
            {
                using (StreamWriter sw = new StreamWriter(batchPath, false, System.Text.Encoding.Default))
                {
                    sw.WriteLine("@echo off");
                    sw.WriteLine("timeout /t 2 /nobreak >nul");
                    sw.WriteLine("rmdir /s /q \"" + appDir + "\"");
                    sw.WriteLine("del \"%~f0\"");
                }

                ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/c \"" + batchPath + "\"");
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                Process.Start(psi);
            }
            catch { }

            MessageBox.Show("Đã gỡ cài đặt hoàn tất AVRoad 2026 Solution khỏi máy tính của bạn!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
