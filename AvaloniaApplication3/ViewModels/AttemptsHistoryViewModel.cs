using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AvaloniaApplication3.Models;
using AvaloniaApplication3.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaApplication3.ViewModels
{
    public partial class AttemptsHistoryViewModel : ViewModelBase
    {
        private readonly IQuizAttemptService _attempts;
        private int _userId;

        [ObservableProperty] private bool isLoading;
        [ObservableProperty] private ObservableCollection<QuizAttempt> items = new();

        public AttemptsHistoryViewModel(IQuizAttemptService attempts)
        {
            _attempts = attempts;
        }

        public async Task LoadAsync(int userId)
        {
            _userId = userId;
            await RefreshAsync();
        }

        [RelayCommand]
        public async Task RefreshAsync()
        {
            if (_userId <= 0) return;
            IsLoading = true;
            try
            {
                var list = await _attempts.GetAttemptsByUserAsync(_userId);
                Items = new ObservableCollection<QuizAttempt>(list);
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Placeholder for a future “open attempt detail”
        [RelayCommand]
        private void OpenAttempt(QuizAttempt? attempt) { /* no-op for now */ }
    }
}
