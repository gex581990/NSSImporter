namespace NSSImporter.Models;

public enum GlobalOccurrenceAction { Transfer, Remove, Skip }
public enum DuplicateAction { Overwrite, Backup, Delete, Skip }
public enum InsertMode { Standalone, CreateMenu, ExistingMenu }

public class InsertPlacement
{
    public InsertMode Mode { get; set; } = InsertMode.Standalone;
    public string IndexOrEnd { get; set; } = "end";
    public string? NewMenuTitle { get; set; }
    public string? NewMenuMode { get; set; }
    public string? NewMenuTypes { get; set; }
    public string? ExistingMenuTitle { get; set; }
}

public record DuplicateResolutionResult(bool Success, string Message, string? FinalImportedFullPath);
