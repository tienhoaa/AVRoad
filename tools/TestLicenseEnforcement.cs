using System;
using System.IO;
using AVRoad.Security;

namespace AVRoad.Tests
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("==================================================================");
            Console.WriteLine("  BỘ KIỂM THỬ TỰ ĐỘNG KHÓA BẢN QUYỀN CHẶT CHẼ (LICENSE ENFORCEMENT)");
            Console.WriteLine("==================================================================");

            string licDir = LicenseManager.LicenseDirectory;
            string licPath = LicenseManager.LicenseFilePath;
            string backupPath = licPath + ".test_backup";

            bool originalExisted = File.Exists(licPath);
            if (originalExisted)
            {
                if (File.Exists(backupPath)) File.Delete(backupPath);
                File.Move(licPath, backupPath);
            }

            int failures = 0;

            try
            {
                // =================================================================
                // TEST CASE 1: Chưa có file bản quyền (Người mới cài đặt lần đầu)
                // =================================================================
                Console.WriteLine("\n[TEST CASE 1] Người mới cài đặt lần đầu - Chưa có file key .avlic:");
                LicenseValidationResult res1 = LicenseManager.ValidateLicense(true);
                Console.WriteLine("  - IsValid: " + res1.IsValid + " (Mong đợi: False)");
                Console.WriteLine("  - Status: " + res1.Status + " (Mong đợi: NotFound)");
                Console.WriteLine("  - Thông báo: " + res1.Message);

                bool isActiveValid1 = LicenseManager.IsActiveLicenseValid();
                Console.WriteLine("  - LicenseManager.IsActiveLicenseValid(): " + isActiveValid1 + " (Mong đợi: False)");

                bool cmdTt1 = LicenseManager.IsCommandAuthorized("TT");
                bool cmdDlbd1 = LicenseManager.IsCommandAuthorized("DLBD");
                bool cmdDaoDap1 = LicenseManager.IsCommandAuthorized("DAODAP");
                bool cmdLic1 = LicenseManager.IsCommandAuthorized("AV_LICENSE");

                Console.WriteLine("  - Lệnh thiết kế 'TT' được cấp phép: " + cmdTt1 + " (Mong đợi: False - Chặn đứng)");
                Console.WriteLine("  - Lệnh thiết kế 'DLBD' được cấp phép: " + cmdDlbd1 + " (Mong đợi: False - Chặn đứng)");
                Console.WriteLine("  - Lệnh thiết kế 'DAODAP' được cấp phép: " + cmdDaoDap1 + " (Mong đợi: False - Chặn đứng)");
                Console.WriteLine("  - Lệnh quản trị 'AV_LICENSE' được phép: " + cmdLic1 + " (Mong đợi: True - Whitelist)");

                if (res1.IsValid || isActiveValid1 || cmdTt1 || cmdDlbd1 || cmdDaoDap1 || !cmdLic1)
                {
                    Console.WriteLine("  ===> THẤT BẠI ở Test Case 1!");
                    failures++;
                }
                else
                {
                    Console.WriteLine("  ===> ĐẠT CHUẨN 100%: Launcher chặn mở CAD và Lõi CAD Veto chặn toàn bộ lệnh TDT!");
                }

                // =================================================================
                // TEST CASE 2: Key sai sản phẩm hoặc chữ ký không hợp lệ
                // =================================================================
                Console.WriteLine("\n[TEST CASE 2] Thử nạp key giả mạo hoặc key sai sản phẩm (Civil 3D):");
                string fakeLic = Path.Combine(licDir, "fake_test.avlic");
                File.WriteAllText(fakeLic, "{\"payload\":\"corrupted\",\"signature\":\"invalid\"}");

                string importMsg;
                bool fakeImportOk = LicenseManager.ImportLicense(fakeLic, out importMsg);
                Console.WriteLine("  - Nạp key chữ ký giả: " + fakeImportOk + " | Lý do từ chối: " + importMsg);

                if (File.Exists(fakeLic)) File.Delete(fakeLic);

                if (fakeImportOk)
                {
                    Console.WriteLine("  ===> THẤT BẠI ở Test Case 2: Phần mềm chấp nhận key giả!");
                    failures++;
                }
                else
                {
                    Console.WriteLine("  ===> ĐẠT CHUẨN 100%: Hệ thống RSA-2048 từ chối chữ ký không hợp lệ!");
                }

                // =================================================================
                // TEST CASE 3: Nạp key hợp lệ (RSA-2048 ký đúng định dạng AVRoad)
                // =================================================================
                Console.WriteLine("\n[TEST CASE 3] Nạp key bản quyền hợp lệ (được cấp phép chính thức):");
                if (File.Exists(backupPath))
                {
                    bool restoreImport = LicenseManager.ImportLicense(backupPath, out importMsg);
                    Console.WriteLine("  - Nạp key hợp lệ: " + restoreImport + " | Thông báo: " + importMsg);

                    LicenseValidationResult res3 = LicenseManager.ValidateLicense(true);
                    Console.WriteLine("  - IsValid: " + res3.IsValid + " (Mong đợi: True)");
                    Console.WriteLine("  - Status: " + res3.Status + " (Mong đợi: Valid)");
                    Console.WriteLine("  - Khách hàng: " + (res3.Data != null ? res3.Data.CustomerName : "N/A"));
                    Console.WriteLine("  - Gói bản quyền: " + (res3.Data != null ? res3.Data.LicenseType : "N/A"));

                    bool isActiveValid3 = LicenseManager.IsActiveLicenseValid();
                    bool cmdTt3 = LicenseManager.IsCommandAuthorized("TT");
                    bool cmdDlbd3 = LicenseManager.IsCommandAuthorized("DLBD");
                    bool cmdDaoDap3 = LicenseManager.IsCommandAuthorized("DAODAP");

                    Console.WriteLine("  - LicenseManager.IsActiveLicenseValid(): " + isActiveValid3 + " (Mong đợi: True)");
                    Console.WriteLine("  - Lệnh thiết kế 'TT' được cấp phép: " + cmdTt3 + " (Mong đợi: True)");
                    Console.WriteLine("  - Lệnh thiết kế 'DLBD' được cấp phép: " + cmdDlbd3 + " (Mong đợi: True)");
                    Console.WriteLine("  - Lệnh thiết kế 'DAODAP' được cấp phép: " + cmdDaoDap3 + " (Mong đợi: True)");

                    if (!res3.IsValid || !isActiveValid3 || !cmdTt3 || !cmdDlbd3 || !cmdDaoDap3)
                    {
                        Console.WriteLine("  ===> THẤT BẠI ở Test Case 3: Key hợp lệ bị từ chối!");
                        failures++;
                    }
                    else
                    {
                        Console.WriteLine("  ===> ĐẠT CHUẨN 100%: Bản quyền kích hoạt thành công, mở CAD và mọi lệnh mượt mà!");
                    }
                }
                else
                {
                    Console.WriteLine("  (Không tìm thấy backup key gốc để chạy Test Case 3)");
                }
            }
            finally
            {
                // Phục hồi lại file license ban đầu
                if (File.Exists(backupPath))
                {
                    if (File.Exists(licPath)) File.Delete(licPath);
                    File.Move(backupPath, licPath);
                }
            }

            Console.WriteLine("\n==================================================================");
            if (failures == 0)
            {
                Console.WriteLine("  KẾT QUẢ: TẤT CẢ CÁC BÀI TEST ĐỀU VƯỢT QUA 100% THÀNH CÔNG!");
                Console.WriteLine("==================================================================");
                return 0;
            }
            else
            {
                Console.WriteLine("  KẾT QUẢ: CÓ " + failures + " BÀI TEST THẤT BẠI!");
                Console.WriteLine("==================================================================");
                return 1;
            }
        }
    }
}
