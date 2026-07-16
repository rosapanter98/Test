using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace CertPrep.Features.Options.Views;

public partial class OptionsView : UserControl
{
    public OptionsView() => InitializeComponent();

    private async void ImportQuestionBank(object? sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
        {
            return;
        }

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
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
        if (path is not null && DataContext is OptionsViewModel viewModel)
        {
            await viewModel.ImportQuestionBankAsync(path);
        }
    }
}
