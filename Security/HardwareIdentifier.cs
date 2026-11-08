using System;
using System.IO;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;

namespace AVRoad.Security
{
    /// <summary>
    /// Thu thập mã định danh phần cứng (Hardware ID) ẩn danh và an toàn.
    /// Tương thích 100% giữa ứng dụng 32-bit (AutoCAD 2007) và 64-bit (Civil 3D, Admin Portal).
    /// </summary>
    public static class HardwareIdentifier
    {
        private static string _cachedHwid;
        private static readonly object _lock = new object();

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegOpenKeyExW")]
        private static extern int RegOpenKeyEx(IntPtr hKey, string subKey, int ulOptions, int samDesired, out IntPtr phkResult);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegQueryValueExW")]
        private static extern int RegQueryValueEx(IntPtr hKey, string lpValueName, int lpReserved, out uint lpType, StringBuilder lpData, ref uint lpcbData);

        [DllImport("advapi32.dll")]
        private static extern int RegCloseKey(IntPtr hKey);

        /// <summary>
        /// Lấy Hardware ID có định dạng: AV-XXXX-XXXX-XXXX-XXXX
        /// </summary>
        public static string GetHardwareId()
        {
            if (!string.IsNullOrEmpty(_cachedHwid))
                return _cachedHwid;

            lock (_lock)
            {
                if (!string.IsNullOrEmpty(_cachedHwid))
                    return _cachedHwid;

                _cachedHwid = ComputeHardwareId();
                return _cachedHwid;
            }
        }

        public static string GetMachineName()
        {
            try
            {
                return Environment.MachineName;
            }
            catch
            {
                return "DESKTOP-UNKNOWN";
            }
        }

        public static string GetOsVersion()
        {
            try
            {
                bool is64 = IntPtr.Size == 8 || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"));
                return string.Format("{0} ({1})", Environment.OSVersion, is64 ? "64-bit" : "32-bit");
            }
            catch
            {
                return "Microsoft Windows";
            }
        }

        private static string ComputeHardwareId()
        {
            try
            {
                string mbSerial = GetMotherboardSerial();
                string cpuId = GetCpuId();
                string diskSerial = GetDiskSerial();
                string machineGuid = GetMachineGuid();

                StringBuilder sb = new StringBuilder();
                sb.Append("MB:").Append(mbSerial != null ? mbSerial : "NONE").Append('|');
                sb.Append("CPU:").Append(cpuId != null ? cpuId : "NONE").Append('|');
                sb.Append("DISK:").Append(diskSerial != null ? diskSerial : "NONE").Append('|');
                sb.Append("GUID:").Append(machineGuid != null ? machineGuid : "NONE");

                if (string.IsNullOrEmpty(mbSerial) && string.IsNullOrEmpty(cpuId) && string.IsNullOrEmpty(diskSerial))
                {
                    string mac = GetMacAddress();
                    sb.Append("|MAC:").Append(mac != null ? mac : "DEFAULT_MAC");
                }

                using (SHA256 sha = SHA256.Create())
                {
                    byte[] hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
                    string hex = BitConverter.ToString(hashBytes).Replace("-", "").ToUpperInvariant();

                    if (hex.Length >= 16)
                    {
                        return string.Format("AV-{0}-{1}-{2}-{3}",
                            hex.Substring(0, 4),
                            hex.Substring(4, 4),
                            hex.Substring(8, 4),
                            hex.Substring(12, 4));
                    }

                    return string.Format("AV-{0}", hex.PadRight(16, '0').Substring(0, 16));
                }
            }
            catch
            {
                string fallbackGuid = GetMachineGuid();
                if (string.IsNullOrEmpty(fallbackGuid)) fallbackGuid = "AV-FALLBACK-GUID-0000";

                using (SHA256 sha = SHA256.Create())
                {
                    byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(fallbackGuid));
                    string hex = BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
                    return string.Format("AV-{0}-{1}-{2}-{3}",
                        hex.Substring(0, 4),
                        hex.Substring(4, 4),
                        hex.Substring(8, 4),
                        hex.Substring(12, 4));
                }
            }
        }

        private static string GetMotherboardSerial()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard"))
                using (ManagementObjectCollection collection = searcher.Get())
                {
                    foreach (ManagementObject obj in collection)
                    {
                        object val = obj["SerialNumber"];
                        if (val != null)
                        {
                            string serial = val.ToString().Trim();
                            if (!string.IsNullOrEmpty(serial) &&
                                !serial.Equals("None", StringComparison.OrdinalIgnoreCase) &&
                                !serial.Equals("Default string", StringComparison.OrdinalIgnoreCase))
                            {
                                return serial;
                            }
                        }
                    }
                }
            }
            catch { }
            return null;
        }

        private static string GetCpuId()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor"))
                using (ManagementObjectCollection collection = searcher.Get())
                {
                    foreach (ManagementObject obj in collection)
                    {
                        object val = obj["ProcessorId"];
                        if (val != null)
                        {
                            string id = val.ToString().Trim();
                            if (!string.IsNullOrEmpty(id))
                            {
                                return id;
                            }
                        }
                    }
                }
            }
            catch { }
            return null;
        }

        private static string GetDiskSerial()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_DiskDrive"))
                using (ManagementObjectCollection collection = searcher.Get())
                {
                    foreach (ManagementObject obj in collection)
                    {
                        object val = obj["SerialNumber"];
                        if (val != null)
                        {
                            string serial = val.ToString().Trim();
                            if (!string.IsNullOrEmpty(serial))
                            {
                                return serial;
                            }
                        }
                    }
                }
            }
            catch { }
            return null;
        }

        private static string GetMachineGuid()
        {
            try
            {
                // Truy cập trực tiếp Registry 64-bit qua Advapi32 kể cả khi chạy trong tiến trình 32-bit (x86)
                IntPtr HKEY_LOCAL_MACHINE = new IntPtr(unchecked((int)0x80000002));
                const int KEY_QUERY_VALUE = 0x0001;
                const int KEY_WOW64_64KEY = 0x0100;
                IntPtr hKey;
                if (RegOpenKeyEx(HKEY_LOCAL_MACHINE, @"SOFTWARE\Microsoft\Cryptography", 0, KEY_QUERY_VALUE | KEY_WOW64_64KEY, out hKey) == 0)
                {
                    uint size = 1024;
                    uint type;
                    StringBuilder sb = new StringBuilder(1024);
                    if (RegQueryValueEx(hKey, "MachineGuid", 0, out type, sb, ref size) == 0)
                    {
                        RegCloseKey(hKey);
                        string guid = sb.ToString().Trim();
                        if (!string.IsNullOrEmpty(guid)) return guid;
                    }
                    RegCloseKey(hKey);
                }
            }
            catch { }

            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography"))
                {
                    if (key != null)
                    {
                        object val = key.GetValue("MachineGuid");
                        if (val != null) return val.ToString().Trim();
                    }
                }
            }
            catch { }

            return null;
        }

        private static string GetMacAddress()
        {
            try
            {
                foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (nic.OperationalStatus == OperationalStatus.Up &&
                        nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    {
                        PhysicalAddress pa = nic.GetPhysicalAddress();
                        if (pa != null)
                        {
                            string mac = pa.ToString();
                            if (!string.IsNullOrEmpty(mac) && mac.Length >= 6)
                                return mac;
                        }
                    }
                }
            }
            catch { }
            return null;
        }
    }
}
