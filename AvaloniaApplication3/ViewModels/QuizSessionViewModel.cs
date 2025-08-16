using AvaloniaApplication3.Models;
using AvaloniaApplication3.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaApplication3.ViewModels
{
    /// <summary>
    /// Root for the quiz section: hosts selection (left) + attempts history (right).
    /// Optionally shows a runner/results pane on the right via CurrentRightPane.
    /// </summary>
    public partial class QuizSessionViewModel : ViewModelBase
    {
        private readonly User _user;
        private readonly IQuizService _quizService;
        private readonly IQuizAttemptService _attempts;

        public QuizSelectionViewModel Selection { get; }
        public AttemptsHistoryViewModel History { get; }

        [ObservableProperty] private object? currentRightPane; // Runner or Results (optional)

        public QuizSessionViewModel(User user, IQuizService quizService, IQuizAttemptService attempts)
        {
            _user = user;
            _quizService = quizService;
            _attempts = attempts;

            // Reuse existing selection VM; when a quiz is chosen, start the runner in the right pane
            Selection = new QuizSelectionViewModel(_quizService, StartQuiz);

            // Attempts history VM on the right
            History = new AttemptsHistoryViewModel(_attempts);

            // Initial load of attempts for this user
            _ = History.LoadAsync(_user.Id);
        }

        private void StartQuiz(Quiz quiz)
        {
            // Show the runner on the right; when it completes, show results and refresh history
            CurrentRightPane = new QuizRunnerViewModel(_user, quiz, OnQuizCompleted, _attempts);
        }

        private void OnQuizCompleted(string quizTitle, int score, int total)
        {
            CurrentRightPane = new QuizResultsViewModel(
                correctAnswers: score,
                totalQuestions: total,
                quizTitle: quizTitle,
                onReturn: () => CurrentRightPane = null);

            _ = History.RefreshAsync(); // refresh list after completion
        }

        [RelayCommand]
        private void RefreshHistory() => _ = History.RefreshAsync();
    }
}
