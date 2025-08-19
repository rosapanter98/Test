using AvaloniaApplication3.Models;
using AvaloniaApplication3.Services;
using AvaloniaApplication3.Utility;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;

namespace AvaloniaApplication3.ViewModels.Quizzes
{
    /// <summary>
    /// Hosts quiz selection (left) + attempts history (right).
    /// When a quiz is started, delegates navigation to the shell.
    /// </summary>
    public partial class QuizSessionViewModel : ViewModelBase
    {
        private readonly User _user;
        private readonly IQuizService _quizService;
        private readonly IQuizAttemptService _attempts;
        private readonly Action<Quiz> _onStartQuiz;

        public QuizSelectionViewModel Selection { get; }
        public AttemptsHistoryViewModel History { get; }

        public QuizSessionViewModel(
            User user,
            IQuizService quizService,
            IQuizAttemptService attempts,
            Action<Quiz> onStartQuiz)
        {
            _user = user;
            _quizService = quizService;
            _attempts = attempts;
            _onStartQuiz = onStartQuiz;

            Selection = new QuizSelectionViewModel(_quizService, StartQuiz);
            History = new AttemptsHistoryViewModel(_attempts);
            _ = History.LoadAsync(_user.Id)
             .ContinueWith(t =>
             {
                 Console.WriteLine($"[SESSION VM] History.LoadAsync completed. Status={t.Status}");
                 if (t.Exception != null) Console.WriteLine(t.Exception);
             }, TaskScheduler.Default);
                }

        [RelayCommand]
        private void StartQuiz(Quiz quiz) => _onStartQuiz(quiz);

        [RelayCommand]
        private void RefreshHistory() => _ = History.RefreshAsync();
    }
}
