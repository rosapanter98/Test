using System;
using System.Windows.Input;
using AvaloniaApplication3.Utility;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaApplication3.ViewModels
{
    public partial class QuizResultsViewModel : ViewModelBase
    {
        [ObservableProperty] private int correctAnswers;
        [ObservableProperty] private int totalQuestions;
        [ObservableProperty] private string quizTitle;

        public string ScoreMessage => $"You got {CorrectAnswers} out of {TotalQuestions} correct.";

        public ICommand ReturnToMenuCommand { get; }
        private readonly Action _onReturn;

        // Optional: store attempt id if you want to drill into details later
        public int? AttemptId { get; }

        public QuizResultsViewModel(int correctAnswers, int totalQuestions, string quizTitle, Action onReturn, int? attemptId = null)
        {
            CorrectAnswers = correctAnswers;
            TotalQuestions = totalQuestions;
            QuizTitle = quizTitle;
            _onReturn = onReturn;
            AttemptId = attemptId;

            ReturnToMenuCommand = new RelayCommand(() => _onReturn?.Invoke());
        }

        partial void OnCorrectAnswersChanged(int value) => OnPropertyChanged(nameof(ScoreMessage));
        partial void OnTotalQuestionsChanged(int value) => OnPropertyChanged(nameof(ScoreMessage));
    }
}
