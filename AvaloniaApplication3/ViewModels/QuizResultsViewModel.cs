using AvaloniaApplication3.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Input;

namespace AvaloniaApplication3.ViewModels
{
    public partial class QuizResultsViewModel : ViewModelBase
    {
        [ObservableProperty] 
        private int correctAnswers;
        
        [ObservableProperty]
        private int totalQuestions;
        
        [ObservableProperty]
        private string quizTitle;

        public string ScoreMessage => $"You got {CorrectAnswers} out of {TotalQuestions} correct.";

        public ICommand ReturnToMenuCommand { get; }

        private readonly Action _onReturn;

        public QuizResultsViewModel(int correctAnswers, int totalQuestions, string quizTitle, Action onReturn)
        {
            CorrectAnswers = correctAnswers;
            TotalQuestions = totalQuestions;
            QuizTitle = quizTitle;
            _onReturn = onReturn;

            ReturnToMenuCommand = new RelayCommand(() => _onReturn?.Invoke());
        }
        partial void OnCorrectAnswersChanged(int value) => OnPropertyChanged(nameof(ScoreMessage));
        partial void OnTotalQuestionsChanged(int value) => OnPropertyChanged(nameof(ScoreMessage));
    }
}

