using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaApplication3.Models;
using AvaloniaApplication3.ViewModels;
using AvaloniaApplication3.ViewModels.Quizzes;

namespace AvaloniaApplication3.Views.Quizzes
{
    public partial class AttemptsHistoryView : UserControl
    {
        public AttemptsHistoryView() => InitializeComponent();

        private async void OnExpanded(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not AttemptsHistoryViewModel vm) return;
            if (sender is not Expander expander) return;
            if (expander.DataContext is not AttemptRowViewModel row) return;

            await vm.LoadAttemptDetailsCommand.ExecuteAsync(row);
            // stays expanded (we didn’t replace the row)
        }
    }
}
