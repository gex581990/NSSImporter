using NSSImporter.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace NSSImporter.Models
{
public class DuplicateResolver
    {
        private readonly NssFileManager _mgr;
        public DuplicateResolver(NssFileManager mgr) => _mgr = mgr;

        public DuplicateResolutionResult ResolveOne(string sourceNss, string existingPath, DuplicateAction action)
        {
            var filename = Path.GetFileName(sourceNss);
            var backupName = FileBackupUtils.NextOldName(_mgr.BackupDir, filename);
            try
            {
                switch (action)
                {
                    case DuplicateAction.Overwrite:
                        FileBackupUtils.SafeBackupFile(existingPath);
                        File.Move(existingPath, backupName);
                        File.Copy(sourceNss, existingPath, true);
                        return new(true, $"Overwritten; previous moved to {backupName}", existingPath);

                    case DuplicateAction.Backup:
                        File.Move(existingPath, backupName);
                        var dest = Path.Combine(_mgr.SnippetsDir, filename);
                        File.Copy(sourceNss, dest, true);
                        return new(true, $"Existing moved to backup; new copied to snippets", dest);

                    case DuplicateAction.Delete:
                        File.Delete(existingPath);
                        return new(true, "Deleted", null);

                    default:
                        return new(true, "Skipped", null);
                }
            }
            catch (System.Exception ex)
            {
                return new(false, ex.Message, null);
            }
        }

        public List<(string File, string Line)> FindOccurrences(string filename)
        {
            var results = new List<(string File, string Line)>();
            var files = _mgr.FindAllNssUnderImports().ToList();
            if (File.Exists(_mgr.MainShellFile)) files.Add(_mgr.MainShellFile);

            var rx = new Regex(@"import\s+'.*" + Regex.Escape(filename) + @"'", RegexOptions.IgnoreCase);
            foreach (var f in files)
            {
                try
                {
                    foreach (var l in File.ReadLines(f))
                    {
                        if (rx.IsMatch(l))
                        {
                            results.Add((f, l.Trim()));
                            break;
                        }
                    }
                }
                catch
                {
                }
            }
            return results;
        }
    }
}
