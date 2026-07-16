using Avalonia.Controls;

namespace CertPrep.Shell;

public partial class MainWindow : Window
{
    private bool _shutdownReady;

    public MainWindow() => InitializeComponent();

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
