using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace CertPrep.Shell;

public partial class MainWindow : Window
{
    private bool _shutdownReady;

    public MainWindow() => InitializeComponent();

    private async void ImportQuestionBank(object? sender, RoutedEventArgs args)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Import CertPrep question bank",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("SQLite databases")
                {
                    Patterns = ["*.db", "*.sqlite", "*.sqlite3"]
                }
            ]
        });

        var path = files.Count == 1 ? files[0].TryGetLocalPath() : null;
        if (path is not null && DataContext is ShellViewModel shell)
        {
            await shell.ImportQuestionBankAsync(path);
        }
    }

    private async void WindowClosing(object? sender, WindowClosingEventArgs args)
    {
        if (_shutdownReady || DataContext is not ShellViewModel shell)
        {
            return;
        }

        args.Cancel = true;
        try
        {
            await shell.FlushActiveSessionAsync();
        }
        finally
        {
            _shutdownReady = true;
            Close();
        }
    }
}
