
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;
using NSSImporter.Models;

namespace NSSImporter.Views
{
    public sealed partial class MenuInsertDialog : ContentDialog
    {
        public InsertPlacement Result { get; private set; } = new InsertPlacement { Mode = InsertMode.Standalone, IndexOrEnd = "end" };

        public MenuInsertDialog(IEnumerable<string> previewLines, IEnumerable<string> existingMenuTitles)
        {
            this.InitializeComponent();
            this.PrimaryButtonClick += MenuInsertDialog_PrimaryButtonClick;
            ModeRadios.SelectionChanged += ModeRadios_SelectionChanged;

            int i = 1;
            foreach (var line in previewLines ?? Enumerable.Empty<string>())
                PreviewLines.Items.Add($"{i++,3}| {line}");

            var uniq = new HashSet<string>(existingMenuTitles ?? Enumerable.Empty<string>());
            foreach (var m in uniq) MenuCombo.Items.Add(m);
        }

        private void ModeRadios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var sel = ModeRadios.SelectedIndex;
            CreateMenuPanel.Visibility = sel == 1 ? Visibility.Visible : Visibility.Collapsed;
            ExistingMenuPanel.Visibility = sel == 2 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void MenuInsertDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var sel = ModeRadios.SelectedIndex;
            if (sel == 1)
            {
                if (string.IsNullOrWhiteSpace(TitleBox.Text))
                {
                    args.Cancel = true;
                    return;
                }
                Result.Mode = InsertMode.CreateMenu;
                Result.NewMenuTitle = TitleBox.Text.Trim();
                Result.NewMenuMode = string.IsNullOrWhiteSpace(ModeBox.Text) ? null : ModeBox.Text.Trim();
                Result.NewMenuTypes = string.IsNullOrWhiteSpace(TypesBox.Text) ? null : TypesBox.Text.Trim();
                Result.IndexOrEnd = string.IsNullOrWhiteSpace(IndexBox.Text) ? "end" : IndexBox.Text.Trim();
            }
            else if (sel == 2)
            {
                Result.Mode = InsertMode.ExistingMenu;
                Result.ExistingMenuTitle = MenuCombo.SelectedItem as string;
                Result.IndexOrEnd = string.IsNullOrWhiteSpace(MenuIndexBox.Text) ? "end" : MenuIndexBox.Text.Trim();
            }
            else
            {
                Result.Mode = InsertMode.Standalone;
                Result.IndexOrEnd = string.IsNullOrWhiteSpace(IndexBox.Text) ? "end" : IndexBox.Text.Trim();
            }
        }
    }
}
