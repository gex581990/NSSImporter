
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace NSSImporter.Helpers;

public static class DialogUtils
{
    public static ContentDialog CreateDialog(Window window, string title, string content, string primary, string? secondary = null, string? close = "Cancel")
    {
        var dlg = new ContentDialog
        {
            Title = title,
            Content = content,
            PrimaryButtonText = primary,
            CloseButtonText = close,
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = window.Content.XamlRoot
        };
        if (!string.IsNullOrWhiteSpace(secondary))
            dlg.SecondaryButtonText = secondary;
        return dlg;
    }
}
