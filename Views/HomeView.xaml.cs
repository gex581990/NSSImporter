using System.Collections.Generic;
using System.Text.RegularExpressions;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NSSImporter.Helpers;
using NSSImporter.Models;
using System;
using System.IO;
using System.Linq;
using Windows.Storage.Pickers;

namespace NSSImporter.Views;

public sealed partial class HomeView : UserControl
{
    private Window? _window;
    private NssFileManager? _mgr;

    // === helper added: extract text between single quotes in a line ===
    private static string? ExtractSingleQuoted(string line)
    {
        var first = line.IndexOf('\'');
        if (first < 0) return null;
        var second = line.IndexOf('\'', first + 1);
        if (second <= first) return null;
        return line.Substring(first + 1, second - first - 1);
    }
    // ==================================================================

    public HomeView()
    {
        this.InitializeComponent();
        this.Loaded += HomeView_Loaded;
        BrowseBtn.Click += BrowseBtn_Click;
        StartBtn.Click += StartBtn_Click;
        RestartBtn.Click += RestartBtn_Click;
    }

    private void HomeView_Loaded(object sender, RoutedEventArgs e)
    {
        _window = App.MainWindow;
        var auto = NssFileManager.DetectDefaultShellPath();
        if (!string.IsNullOrWhiteSpace(auto))
        {
            ShellPathBox.Text = auto;
            ShellPathBox.IsEnabled = true;
            StatusText.Text = $"Found Nilesoft Shell at: {auto}";
        }
        else
        {
            StatusText.Text = "Nilesoft Shell not found. Please browse to its folder.";
        }
    }

    private async void BrowseBtn_Click(object sender, RoutedEventArgs e)
    {
        if (_window is null) return;
        var picker = new FolderPicker();
        picker.FileTypeFilter.Add("*");
        Win32Interop.InitializeWithWindow(picker, _window);
        var folder = await picker.PickSingleFolderAsync();
        if (folder != null)
        {
            ShellPathBox.Text = folder.Path;
            StatusText.Text = $"Using: {folder.Path}";
        }
    }

    private async void StartBtn_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ShellPathBox.Text) || !Directory.Exists(ShellPathBox.Text))
            {
                StatusText.Text = "Invalid Nilesoft Shell path.";
                return;
            }
            _mgr = new NssFileManager(ShellPathBox.Text);

            // Pick .nss file
            if (_window is null) return;
            var fp = new FileOpenPicker();
            fp.FileTypeFilter.Add(".nss");
            Win32Interop.InitializeWithWindow(fp, _window);
            var file = await fp.PickSingleFileAsync();
            if (file is null) return;

            var handler = new ImportHandler(_mgr);
            var (importedPath, dupPaths) = handler.CopyIntoSnippets(file.Path);
            Log($"Imported into snippets: {importedPath}");

            var filename = System.IO.Path.GetFileName(importedPath);
            var existingSameName = dupPaths.Where(p => !p.Equals(importedPath, StringComparison.OrdinalIgnoreCase)).ToList();
            if (existingSameName.Count == 1)
            {
                var existing = existingSameName[0];
                var dlg = Helpers.DialogUtils.CreateDialog(_window, "Duplicate Found",
                    $"A file named '{filename}' already exists at:\n{existing}\n\nChoose what to do:",
                    "Overwrite", "Backup", "Cancel");
                var res = await dlg.ShowAsync();
                if (res == ContentDialogResult.Primary)
                {
                    var dr = new DuplicateResolver(_mgr);
                    var r = dr.ResolveOne(importedPath, existing, DuplicateAction.Overwrite);
                    Log(r.Message);
                }
                else if (res == ContentDialogResult.Secondary)
                {
                    var dr = new DuplicateResolver(_mgr);
                    var r = dr.ResolveOne(importedPath, existing, DuplicateAction.Backup);
                    Log(r.Message);
                }
                else
                {
                    Log("Import canceled by user.");
                    return;
                }
            }
            else if (existingSameName.Count > 1)
            {
                var backups = ImportHandler.RenameAllToBackup(existingSameName, _mgr.BackupDir, filename);
                Log($"Backed up {backups.Count} duplicate(s) to Backup folder.");
            }

            var dupr = new DuplicateResolver(_mgr);
            var occ = dupr.FindOccurrences(filename);
            if (occ.Count > 0)
            {
                var dlg = Helpers.DialogUtils.CreateDialog(_window, "Existing imports",
                    $"This .nss ('{filename}') is already imported in {occ.Count} place(s).\n\n" +
                    "Transfer import to a new target (removes old import(s) and add to new), " +
                    "Remove existing imports only, or Keep and skip adding?",
                    "Transfer", "Remove", "Keep");
                var r = await dlg.ShowAsync();

                if (r == ContentDialogResult.Primary)
                {
                    foreach (var (cfg, line) in occ)
                    {
                        var importPath = ExtractSingleQuoted(line);
                        if (importPath != null)
                            NssFileManager.RemoveImportLine(cfg, importPath);
                    }

                    await ChooseTargetAndInsert(importedPath);
                    Log("Existing import(s) removed and transferred.");
                }
                else if (r == ContentDialogResult.Secondary)
                {
                    foreach (var (cfg, line) in occ)
                    {
                        var importPath = ExtractSingleQuoted(line);
                        if (importPath != null)
                            NssFileManager.RemoveImportLine(cfg, importPath);
                    }

                    Log("Existing import(s) removed.");
                    await ChooseTargetAndInsert(importedPath);
                }
                else
                {
                    Log("Kept existing imports; no new import added.");
                }
            }
            else
            {
                await ChooseTargetAndInsert(importedPath);
            }

            StatusText.Text = "Done.";
        }
        catch (Exception ex)
        {
            StatusText.Text = ex.Message;
            Log(ex.ToString());
        }
    }

    private async System.Threading.Tasks.Task ChooseTargetAndInsert(string importedPath)
    {
        if (_mgr is null || _window is null) return;
        var items = _mgr.FindConfigTargets().ToList();
        var dlg = new ContentDialog
        {
            Title = "Choose target file to import into",
            PrimaryButtonText = "Insert",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = _window.Content.XamlRoot
        };

        var list = new ListView();
        foreach (var i in items) list.Items.Add(i);
        dlg.Content = list;

        var res = await dlg.ShowAsync();
        if (res == ContentDialogResult.Primary && list.SelectedItem is string chosen)
        {
            var handler = new ImportHandler(_mgr);
            handler.AddImportToTarget(importedPath, chosen, "end");
            Log($"Added import to: {chosen}");
        }
    }

    private void RestartBtn_Click(object sender, RoutedEventArgs e)
    {
        if (_mgr is null) return;
        var exe = System.IO.Path.Combine(_mgr.ShellRoot, "shell.exe");
        if (File.Exists(exe))
        {
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo(exe, "-restart")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                System.Diagnostics.Process.Start(psi);
                Log("shell.exe -restart invoked.");
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
        }
        else
        {
            Log("shell.exe not found in the chosen Nilesoft Shell path.");
        }
    }

    private void Log(string s) => StatusText.Text += s + Environment.NewLine;
}
