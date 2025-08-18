using AvaloniaApplication3.Models;
using AvaloniaApplication3.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaApplication3.ViewModels
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
        private readonly System.Action<Quiz> _onStartQuiz;

        public QuizSelectionViewModel Selection { get; }
        public AttemptsHistoryViewModel History { get; }

        public QuizSessionViewModel(
            User user,
            IQuizService quizService,
            IQuizAttemptService attempts,
            System.Action<Quiz> onStartQuiz)
        {
            _user = user;
            _quizService = quizService;
            _attempts = attempts;
            _onStartQuiz = onStartQuiz;

            Selection = new QuizSelectionViewModel(_quizService, StartQuiz);
            History = new AttemptsHistoryViewModel(_attempts);

            _ = History.LoadAsync(_user.Id);
        }

        private void StartQuiz(Quiz quiz) => _onStartQuiz(quiz);

        [RelayCommand]
        private void RefreshHistory() => _ = History.RefreshAsync();
    }
}
