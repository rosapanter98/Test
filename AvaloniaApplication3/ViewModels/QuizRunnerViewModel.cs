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
        private readonly Action<string, int, int> _onQuizCompleted;
        private readonly List<Question> _questionList;
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

        [ObservableProperty]
        private bool isSingleChoice;

        [ObservableProperty]
        private bool isMultipleChoice;

        [ObservableProperty]
        private bool isTrueFalse;

        public int TotalQuestions => _quiz.Questions.Count;
        public int CurrentIndex => _currentIndex + 1;
        public int Score => _score;
        public int TotalCorrect => _score;
        public string? Explanation => FeedbackText;

        public bool CanSubmitAnswer => !ShowFeedback;
        public bool CanGoNext => ShowFeedback && !QuizCompleted;

        public IRelayCommand SubmitCommand { get; }
        public IRelayCommand NextCommand { get; }

        public QuizRunnerViewModel(Quiz quiz, Action<string, int, int> onQuizCompleted)
        {
            _quiz = quiz ?? throw new ArgumentNullException(nameof(quiz));
            _onQuizCompleted = onQuizCompleted;
            _questionList = _quiz.Questions.ToList();

            SubmitCommand = new RelayCommand(Submit, () => CanSubmitAnswer);
            NextCommand = new RelayCommand(Next, () => CanGoNext);

            LoadQuestion();
        }

        private void LoadQuestion()
        {
            if (_questionList.Count == 0)
                throw new InvalidOperationException("Quiz has no questions.");

            CurrentQuestion = _questionList[_currentIndex];
            SelectedAnswerIds = new();
            SelectedOptionId = null;
            ShowFeedback = false;
            FeedbackText = null;

            foreach (var answer in CurrentQuestion.Answers)
                answer.IsSelected = false;

            IsSingleChoice = CurrentQuestion.Type == QuestionType.SingleChoice;
            IsMultipleChoice = CurrentQuestion.Type == QuestionType.MultipleChoice;
            IsTrueFalse = CurrentQuestion.Type == QuestionType.TrueFalse;

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

        public void NotifySelectionChanged()
        {
            SubmitCommand.NotifyCanExecuteChanged();
        }

        private void Submit()
        {
            HashSet<int> selectedIds;

            if (IsMultipleChoice)
            {
                selectedIds = CurrentQuestion.Answers
                    .Where(a => a.IsSelected)
                    .Select(a => a.Id)
                    .ToHashSet();
            }
            else
            {
                // Single choice or True/False uses SelectedOptionId
                selectedIds = SelectedOptionId.HasValue ? new HashSet<int> { SelectedOptionId.Value } : new HashSet<int>();
            }

            var correctIds = CurrentQuestion.Answers
                .Where(a => a.IsCorrect)
                .Select(a => a.Id)
                .ToHashSet();

            var isCorrect = selectedIds.SetEquals(correctIds);
            if (isCorrect)
                _score++;

            foreach (var answer in CurrentQuestion.Answers)
            {
                if (selectedIds.Contains(answer.Id))
                    answer.IsUserCorrect = answer.IsCorrect;
                else if (answer.IsCorrect)
                    answer.IsUserCorrect = false;
                else
                    answer.IsUserCorrect = null;
            }

            FeedbackText = isCorrect
                ? "Correct!"
                : $"Wrong. Correct answer(s): {string.Join(", ", CurrentQuestion.Answers.Where(a => a.IsCorrect).Select(a => a.Text))}";

            ShowFeedback = true;
        }


        private void Next()
        {
            _currentIndex++;
            if (_currentIndex >= _questionList.Count)
            {
                QuizCompleted = true;
                _onQuizCompleted.Invoke(_quiz.Title, _score, _quiz.Questions.Count);
            }
            else
            {
                LoadQuestion();
            }
        }
    }
}
