
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;

namespace NSSImporter.Views;

public sealed partial class PreviewDialog : ContentDialog
{
    public class PreviewItem
    {
        public string Line { get; set; } = "";
    }

    public PreviewDialog(IEnumerable<string> lines)
    {
        this.InitializeComponent();
        foreach (var l in lines)
            List.Items.Add(l);
    }
}
