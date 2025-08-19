using Avalonia.Threading;
using AvaloniaApplication3.Models;
using AvaloniaApplication3.Services;
using AvaloniaApplication3.Utility;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace AvaloniaApplication3.ViewModels
{
    public partial class QuizSelectionViewModel : ViewModelBase
    {
        private readonly IQuizService _quizService;
        private readonly Action<Quiz> _onQuizSelected;

        [ObservableProperty]
        private ObservableCollection<Quiz> quizzes = new();

        [ObservableProperty]
        private Quiz? selectedQuiz;

        public QuizSelectionViewModel(IQuizService quizService, Action<Quiz> onQuizSelected)
        {
            _quizService = quizService;
            _onQuizSelected = onQuizSelected;

            _ = LoadQuizzesAsync(); // fire-and-forget initial load
        }

        [RelayCommand]
        private async Task LoadQuizzesAsync()
        {
            var list = await _quizService.GetAllQuizzesAsync();

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Quizzes.Clear();
                foreach (var q in list) Quizzes.Add(q);
            });
        }


        public Task ReloadAsync() => LoadQuizzesAsync();

        private bool CanStartQuiz() => SelectedQuiz is not null;

        
        
        [RelayCommand(CanExecute = nameof(CanStartQuiz))]
        private async Task StartQuizAsync()
        {
            if (SelectedQuiz is null) return;

            var fullQuiz = await _quizService.GetFullQuizAsync(SelectedQuiz.Id);
            if (fullQuiz is null) return;

            // optional debug
            Console.WriteLine($"[DEBUG] Quiz: {fullQuiz.Title} (Questions: {fullQuiz.Questions.Count})");

            _onQuizSelected(fullQuiz);
        }

        partial void OnSelectedQuizChanged(Quiz? value)
        {
            StartQuizCommand.NotifyCanExecuteChanged();
        }
    }
}
