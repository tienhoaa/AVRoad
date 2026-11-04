using System;
using System.IO;
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
}
