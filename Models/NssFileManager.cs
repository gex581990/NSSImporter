
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace NSSImporter.Models;

public partial class NssFileManager
{
    public string ShellRoot { get; }
    public string ImportsDir => Path.Combine(ShellRoot, "imports");
    public string SnippetsDir => Path.Combine(ImportsDir, "Snippets");
    public string BackupDir => Path.Combine(ImportsDir, "Backup");
    public string MainShellFile => Path.Combine(ShellRoot, "shell.nss");

    public NssFileManager(string shellRoot)
    {
        ShellRoot = shellRoot;
        Directory.CreateDirectory(ImportsDir);
        Directory.CreateDirectory(SnippetsDir);
        Directory.CreateDirectory(BackupDir);
    }

    public static string? DetectDefaultShellPath()
    {
        var p1 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Nilesoft Shell");
        if (Directory.Exists(p1)) return p1;
        var p2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Nilesoft Shell");
        if (Directory.Exists(p2)) return p2;
        return null;
    }

    public IEnumerable<string> FindAllNssUnderImports()
        => Directory.Exists(ImportsDir)
            ? Directory.EnumerateFiles(ImportsDir, "*.nss", SearchOption.AllDirectories)
            : Enumerable.Empty<string>();

    public IEnumerable<string> FindConfigTargets()
    {
        var list = new List<string>();
        if (File.Exists(MainShellFile)) list.Add(MainShellFile);
        list.AddRange(FindAllNssUnderImports());
        return list;
    }

    public static bool HasImportLine(string file, string relativeImportPath)
    {
        if (!File.Exists(file)) return false;
        var rx = ImportLineRegex();
        foreach (var line in File.ReadLines(file))
        {
            var m = rx.Match(line);
            if (m.Success && string.Equals(Normalize(m.Groups[1].Value), Normalize(relativeImportPath), StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    public static string Normalize(string p) => p.Replace('\\', '/');

    public static void AddImportLine(string file, string relativeImportPath, string? indexOrEnd = "end")
    {
        var all = File.Exists(file) ? File.ReadAllLines(file).ToList() : new List<string>();
        var line = $"import '{Normalize(relativeImportPath)}'";
        if (string.Equals(indexOrEnd, "end", StringComparison.OrdinalIgnoreCase))
        {
            all.Add(line);
        }
        else
        {
            if (int.TryParse(indexOrEnd, out int idx) && idx >= 0 && idx <= all.Count)
                all.Insert(idx, line);
            else
                all.Add(line);
        }
        File.WriteAllLines(file, all);
    }

    public static void RemoveImportLine(string file, string importLineOrPath)
    {
        if (!File.Exists(file)) return;
        var rx = ImportLineRegex();
        var all = File.ReadAllLines(file).ToList();
        all.RemoveAll(l =>
        {
            var m = rx.Match(l);
            if (!m.Success) return false;
            var rel = m.Groups[1].Value;
            return string.Equals(Normalize(rel), Normalize(importLineOrPath), StringComparison.OrdinalIgnoreCase);
        });
        File.WriteAllLines(file, all);
    }
}


partial class NssFileManager
{
    [GeneratedRegex("^\\s*import\\s+'([^']+)'", RegexOptions.IgnoreCase)]
    private static partial Regex ImportLineRegex();
}
