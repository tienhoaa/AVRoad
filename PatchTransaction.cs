using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Xml;

namespace AVRoadDeployment
{
    public static class PatchTransaction
    {
        public static string Hash(string path)
        {
            using (SHA256 sha = SHA256.Create()) using (Stream stream = File.OpenRead(path))
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
        }
        private static string SafePath(string root, string relative)
        {
            if (String.IsNullOrWhiteSpace(relative) || Path.IsPathRooted(relative) || relative.IndexOf(':') >= 0)
                throw new InvalidDataException("Unsafe archive path");
            string fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string path = Path.GetFullPath(Path.Combine(fullRoot, relative));
            if (!path.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase) ||
                relative.Replace('/', '\\').StartsWith(".avroad-rollback", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Archive escaped its destination");
            string current = Path.GetDirectoryName(path);
            while (current != null && current.Length >= fullRoot.TrimEnd('\\').Length)
            {
                if (Directory.Exists(current) && (File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                    throw new InvalidDataException("Reparse-point destination refused");
                current = Path.GetDirectoryName(current);
            }
            return path;
        }
        public static void Validate(string zip, string target, string sha256)
        { ValidateArchive(zip, target, sha256, false); }
        private static void ValidateArchive(string zip, string target, string sha256, bool embeddedInstaller)
        {
            if (sha256 == null || sha256.Length != 64 || !String.Equals(Hash(zip), sha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Patch SHA-256 mismatch or missing checksum");
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (ZipArchive archive = ZipFile.OpenRead(zip))
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (String.IsNullOrEmpty(entry.Name)) continue;
                    SafePath(target, entry.FullName);
                    if (!seen.Add(entry.FullName.Replace('/', '\\'))) throw new InvalidDataException("Duplicate archive entry");
                    // RC patch changes only AVRoad-owned files; no Autodesk binaries.
                    string name = entry.FullName.Replace('/', '\\');
                    if (!embeddedInstaller && name != "AVRoad.exe" && name != "AVUpdater.v2.exe" && name != "AVRoad.Deployment.dll" && name != "version.json")
                        throw new InvalidDataException("File outside v2 RC patch scope: " + name);
                }
            if (!seen.Contains("version.json")) throw new InvalidDataException("Patch has no version.json");
        }
        public static string Apply(string zip, string target, string sha256, bool registerCom)
        { return ApplyInternal(zip, target, sha256, registerCom, false, null); }
        public static string InstallEmbedded(Stream payload, string target, bool registerCom, Action<int, string> progress)
        {
            string temp = Path.Combine(Path.GetTempPath(), "avroad-payload-" + Guid.NewGuid().ToString("N") + ".zip");
            try
            {
                using (Stream file = File.Create(temp)) payload.CopyTo(file);
                return ApplyInternal(temp, target, Hash(temp), registerCom, true, progress);
            }
            finally { if (File.Exists(temp)) File.Delete(temp); }
        }
        private static string ApplyInternal(string zip, string target, string sha256, bool registerCom, bool embeddedInstaller, Action<int, string> progress)
        {
            ValidateArchive(zip, target, sha256, embeddedInstaller);
            string backup = Path.Combine(Path.GetFullPath(target), ".avroad-rollback", "files", DateTime.UtcNow.ToString("yyyyMMddHHmmss") + "-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(backup);
            XmlDocument doc = new XmlDocument(); XmlElement list = doc.CreateElement("Files");
            list.SetAttribute("target", Path.GetFullPath(target)); doc.AppendChild(list);
            string log = Path.Combine(backup, "rollback.xml");
            using (ZipArchive archive = ZipFile.OpenRead(zip))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (String.IsNullOrEmpty(entry.Name)) continue;
                    string dest = SafePath(target, entry.FullName);
                    XmlElement row = doc.CreateElement("File"); row.SetAttribute("name", entry.FullName);
                    row.SetAttribute("existed", File.Exists(dest).ToString());
                    if (File.Exists(dest))
                    {
                        string old = SafePath(Path.Combine(backup, "original"), entry.FullName);
                        Directory.CreateDirectory(Path.GetDirectoryName(old)); File.Copy(dest, old, false);
                    }
                    list.AppendChild(row);
                }
                doc.Save(log); // complete rollback journal before the first overwrite
                try
                {
                    // Commit version metadata last; no false upgrade on partial writes.
                    int completed = 0;
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (String.IsNullOrEmpty(entry.Name) || entry.FullName == "version.json") continue;
                        string dest = SafePath(target, entry.FullName);
                        Directory.CreateDirectory(Path.GetDirectoryName(dest)); entry.ExtractToFile(dest, true);
                        completed++;
                        if (progress != null) progress((int)(100.0 * completed / archive.Entries.Count), entry.FullName);
                    }
                    if (registerCom) Cad7ComRegistration.Apply(target, Path.Combine(backup, "com.xml"));
                    archive.GetEntry("version.json").ExtractToFile(SafePath(target, "version.json"), true);
                    File.WriteAllText(Path.Combine(backup, "committed.txt"), sha256);
                }
                catch (Exception error)
                {
                    try { Rollback(log); }
                    catch (Exception restoreError) { throw new AggregateException("Update and rollback failed; preserve backup at " + backup, error, restoreError); }
                    throw;
                }
            }
            return log;
        }
        public static void Rollback(string log)
        {
            XmlDocument doc = new XmlDocument(); doc.Load(log);
            string backup = Path.GetDirectoryName(Path.GetFullPath(log));
            string target = doc.DocumentElement.GetAttribute("target");
            foreach (XmlElement row in doc.DocumentElement.ChildNodes)
            {
                string dest = SafePath(target, row.GetAttribute("name"));
                if (Boolean.Parse(row.GetAttribute("existed")))
                {
                    string original = SafePath(Path.Combine(backup, "original"), row.GetAttribute("name"));
                    if (!File.Exists(dest) || Hash(original) != Hash(dest)) File.Copy(original, dest, true);
                }
                else if (File.Exists(dest)) File.Delete(dest);
            }
            string com = Path.Combine(backup, "com.xml");
            if (File.Exists(com)) Cad7ComRegistration.Restore(com);
        }
    }
}
