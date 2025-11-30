
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NSSImporter.Helpers;

namespace NSSImporter.Models;

public class ImportHandler
{
    private readonly NssFileManager _mgr;
    public ImportHandler(NssFileManager mgr) => _mgr = mgr;

    public (string importedFullPath, List<string> duplicates) CopyIntoSnippets(string sourceFileFullPath)
    {
        Directory.CreateDirectory(_mgr.SnippetsDir);
        var filename = Path.GetFileName(sourceFileFullPath);
        var dest = Path.Combine(_mgr.SnippetsDir, filename);
        var dups = Directory.EnumerateFiles(_mgr.ImportsDir, filename, SearchOption.AllDirectories).ToList();

        File.Copy(sourceFileFullPath, dest, true);
        return (dest, dups);
    }

    public static List<string> RenameAllToBackup(IEnumerable<string> paths, string backupDir, string filename)
    {
        var outList = new List<string>();
        foreach (var p in paths)
        {
            if (!File.Exists(p)) continue;
            var newFull = FileBackupUtils.NextOldName(backupDir, Path.GetFileName(p));
            File.Move(p, newFull);
            outList.Add(newFull);
        }
        return outList;
    }

    public List<string> RemoveAllImportsOf(string filename)
    {
        var relativeCandidates = new List<string>();
        foreach (var f in Directory.EnumerateFiles(_mgr.ImportsDir, "*.nss", SearchOption.AllDirectories))
        {
            var shortRel = NssFileManager.Normalize(Path.GetRelativePath(_mgr.ImportsDir, f));
            if (Path.GetFileName(shortRel).Equals(filename, StringComparison.OrdinalIgnoreCase))
                relativeCandidates.Add(shortRel);
        }

        var changed = new List<string>();
        foreach (var cfg in _mgr.FindConfigTargets())
        {
            foreach (var rel in relativeCandidates)
            {
                if (NssFileManager.HasImportLine(cfg, rel))
                {
                    NssFileManager.RemoveImportLine(cfg, rel);
                    changed.Add(cfg);
                }
            }
        }
        return changed.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    public void AddImportToTarget(string importedFullPath, string targetConfigFullPath, string indexOrEnd = "end")
    {
        var rel = NssFileManager.Normalize(Path.GetRelativePath(_mgr.ImportsDir, importedFullPath));
        if (!NssFileManager.HasImportLine(targetConfigFullPath, rel))
        {
            NssFileManager.AddImportLine(targetConfigFullPath, rel, indexOrEnd);
        }
    }
}
