using AvaloniaApplication3.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;

namespace AvaloniaApplication3.ViewModels
{
    public partial class AdminPanelViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string newQuizTitle = string.Empty;

        [ObservableProperty]
        private string? newQuizDescription;

        public ObservableCollection<Quiz> Quizzes { get; } = new();

        [RelayCommand]
        private void CreateQuiz()
        {
            if (!string.IsNullOrWhiteSpace(NewQuizTitle))
            {
                var quiz = new Quiz
                {
                    Title = NewQuizTitle,
                    Description = NewQuizDescription
                };

                Quizzes.Add(quiz);
                NewQuizTitle = string.Empty;
                NewQuizDescription = null;
            }
        }
    }
}
