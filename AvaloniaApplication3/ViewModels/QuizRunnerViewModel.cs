using AvaloniaApplication3.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
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

        // Tracking user input
        [ObservableProperty]
        private List<int> selectedAnswerIds = new();

        [ObservableProperty]
        private int? selectedOptionId;

        public int TotalQuestions => _quiz.Questions.Count;
        public int CurrentIndex => _currentIndex + 1;
        public int Score => _score;
        public int TotalCorrect => _score;

        // Reuse feedback text for explanation placeholder
        public string? Explanation => FeedbackText;

        public bool IsSingleChoice => CurrentQuestion?.Type == QuestionType.SingleChoice;
        public bool IsMultipleChoice => CurrentQuestion?.Type == QuestionType.MultipleChoice;
        public bool IsTrueFalse => CurrentQuestion?.Type == QuestionType.TrueFalse;


        // Loosened guard: allow submit as long as feedback not showing
        public bool CanSubmitAnswer => !ShowFeedback;

        // Next only after feedback and not finished
        public bool CanGoNext => ShowFeedback && !QuizCompleted;

        public IRelayCommand SubmitCommand { get; }
        public IRelayCommand NextCommand { get; }

        private readonly List<Question> _questionList;

        public QuizRunnerViewModel(Quiz quiz)
        {
            _quiz = quiz ?? throw new ArgumentNullException(nameof(quiz));
            _questionList = _quiz.Questions.ToList();

            SubmitCommand = new RelayCommand(Submit, () => CanSubmitAnswer);
            NextCommand = new RelayCommand(Next, () => CanGoNext);

            LoadQuestion();
        }

        private void LoadQuestion()
        {
            if (_quiz.Questions == null || !_quiz.Questions.Any())
                throw new InvalidOperationException("Quiz has no questions.");

            CurrentQuestion = _questionList[_currentIndex];

            // reset selection state
            SelectedAnswerIds = new();
            SelectedOptionId = null;
            ShowFeedback = false;
            FeedbackText = null;

            foreach (var answer in CurrentQuestion.Answers)
                answer.IsSelected = false;

            OnPropertyChanged(nameof(IsSingleChoice));
            OnPropertyChanged(nameof(IsMultipleChoice));

            SubmitCommand.NotifyCanExecuteChanged();
            NextCommand.NotifyCanExecuteChanged();
        }

        partial void OnShowFeedbackChanged(bool value)
        {
            SubmitCommand.NotifyCanExecuteChanged();
            NextCommand.NotifyCanExecuteChanged();
        }

        partial void OnSelectedOptionIdChanged(int? value)
        {
            SubmitCommand.NotifyCanExecuteChanged();
        }

        // You could also expose a method that the CheckBox bindings call via Command to refresh CanExecute
        public void NotifySelectionChanged()
        {
            SubmitCommand.NotifyCanExecuteChanged();
        }

        private void Submit()
        {
            // Build selected set
            var selected = CurrentQuestion.Type switch
            {
                QuestionType.SingleChoice => SelectedOptionId.HasValue
                    ? new HashSet<int> { SelectedOptionId.Value }
                    : new HashSet<int>(),

                QuestionType.MultipleChoice => CurrentQuestion.Answers
                    .Where(a => a.IsSelected)
                    .Select(a => a.Id)
                    .ToHashSet(),

                _ => new HashSet<int>()
            };

            // Compare against correct answers
            var correctAnswers = CurrentQuestion.Answers
                .Where(a => a.IsCorrect)
                .Select(a => a.Id)
                .ToHashSet();

            var isCorrect = selected.SetEquals(correctAnswers);

            if (isCorrect)
            {
                _score++;
                FeedbackText = "Correct!";
            }
            else
            {
                var correctText = string.Join(", ",
                    CurrentQuestion.Answers.Where(a => a.IsCorrect).Select(a => a.Text));
                FeedbackText = $"Wrong. Correct answer(s): {correctText}";
            }

            ShowFeedback = true;
        }

        private void Next()
        {
            _currentIndex++;
            if (_currentIndex >= _quiz.Questions.Count)
            {
                QuizCompleted = true;
                NextCommand.NotifyCanExecuteChanged();
            }
            else
            {
                LoadQuestion();
            }
        }
    }
}
