using Avalonia.Controls;
using Avalonia.Platform.Storage;
using UnityProjectManager.ViewModels;
using System.Linq;

namespace UnityProjectManager.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        AddFolderButton.Click += async (s, e) => await SelectFolderAsync();
    }

    private async System.Threading.Tasks.Task SelectFolderAsync()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select Unity Projects Folder",
            AllowMultiple = false
        });

        if (folders.Count > 0)
        {
            var path = folders[0].Path.LocalPath;
            if (DataContext is MainWindowViewModel vm)
            {
                vm.AddWatchFolderCommand.Execute(path);
            }
        }
    }
}