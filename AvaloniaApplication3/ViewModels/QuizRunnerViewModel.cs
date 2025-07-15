using AvaloniaApplication3.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Linq;

namespace AvaloniaApplication3.ViewModels
{
    public partial class QuizRunnerViewModel : ViewModelBase
    {
        private readonly Quiz _quiz;
        private int _currentIndex = 0;
        private int _score = 0;

        [ObservableProperty]
        private Question currentQuestion = null!;

        [ObservableProperty]
        private bool showFeedback;

        [ObservableProperty]
        private string? feedbackText;

        [ObservableProperty]
        private bool quizCompleted;

        [ObservableProperty]
        private List<int> selectedAnswerIds = new();

        [ObservableProperty]
        private int? selectedOptionId;

        public int TotalQuestions => _quiz.Questions.Count;
        public int CurrentIndex => _currentIndex + 1;
        public int Score => _score;

        public string? Explanation => FeedbackText;

        public bool CanSubmit => !ShowFeedback;
        public bool CanGoNext => ShowFeedback && !QuizCompleted;

        public bool IsSingleChoice => CurrentQuestion?.Type == QuestionType.SingleChoice;
        public bool IsMultipleChoice => CurrentQuestion?.Type == QuestionType.MultipleChoice;

        public QuizRunnerViewModel(Quiz quiz)
        {
            _quiz = quiz;
            LoadQuestion();
        }

        private void LoadQuestion()
        {
            CurrentQuestion = _quiz.Questions.ToList()[_currentIndex];
            SelectedAnswerIds = new();
            SelectedOptionId = null;
            ShowFeedback = false;
            FeedbackText = null;

            // Clear any selection state from previous question
            foreach (var answer in CurrentQuestion.Answers)
                answer.IsSelected = false;
        }

        partial void OnShowFeedbackChanged(bool value)
        {
            OnPropertyChanged(nameof(CanSubmit));
            OnPropertyChanged(nameof(CanGoNext));
        }

        partial void OnCurrentQuestionChanged(Question value)
        {
            OnPropertyChanged(nameof(IsSingleChoice));
            OnPropertyChanged(nameof(IsMultipleChoice));
        }

        [RelayCommand]
        private void Submit()
        {
            var correctAnswers = CurrentQuestion.Answers
                .Where(a => a.IsCorrect)
                .Select(a => a.Id)
                .ToHashSet();

            var selected = CurrentQuestion.Type switch
            {
                QuestionType.SingleChoice => SelectedOptionId.HasValue ? new HashSet<int> { SelectedOptionId.Value } : new HashSet<int>(),
                QuestionType.MultipleChoice => CurrentQuestion.Answers.Where(a => a.IsSelected).Select(a => a.Id).ToHashSet(),
                _ => new HashSet<int>()
            };

            var isCorrect = selected.SetEquals(correctAnswers);
            if (isCorrect)
            {
                _score++;
                FeedbackText = "Correct!";
            }
            else
            {
                var correctText = string.Join(", ", CurrentQuestion.Answers.Where(a => a.IsCorrect).Select(a => a.Text));
                FeedbackText = $"Wrong. Correct answer(s): {correctText}";
            }

            ShowFeedback = true;
        }

        [RelayCommand]
        private void Next()
        {
            _currentIndex++;
            if (_currentIndex >= _quiz.Questions.Count)
            {
                QuizCompleted = true;
            }
            else
            {
                LoadQuestion();
            }
        }
    }
}
