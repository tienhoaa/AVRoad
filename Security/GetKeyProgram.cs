using System;
using System.Windows.Forms;
using AVRoad.Security;

namespace AVRoad.GetKeyTool
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                LicenseManager.EnsureDirectoryExists();
                Application.Run(new LicenseDialog());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khởi chạy: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
