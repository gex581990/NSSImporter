
using System.IO;

namespace NSSImporter.Helpers;

public static class FileBackupUtils
{
    public static string NextOldName(string backupDir, string filename)
    {
        Directory.CreateDirectory(backupDir);
        var name = $"old.{filename}";
        var full = Path.Combine(backupDir, name);
        int i = 1;
        while (File.Exists(full))
        {
            name = $"old{i}.{filename}";
            full = Path.Combine(backupDir, name);
            i++;
        }
        return full;
    }

    public static string NextAltName(string folder, string filenameSansExt, string ext)
    {
        var name = $"alt.{filenameSansExt}{ext}";
        var full = Path.Combine(folder, name);
        int i = 1;
        while (File.Exists(full))
        {
            name = $"alt{i}.{filenameSansExt}{ext}";
            full = Path.Combine(folder, name);
            i++;
        }
        return full;
    }

    public static void SafeBackupFile(string path)
    {
        if (!File.Exists(path)) return;
        var bak = path + ".bak";
        if (File.Exists(bak)) File.Delete(bak);
        File.Move(path, bak);
    }
}
