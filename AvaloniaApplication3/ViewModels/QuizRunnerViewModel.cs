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
        private bool isSingleChoice;

        [ObservableProperty]
        private bool isMultipleChoice;

        [ObservableProperty]
        private bool isTrueFalse;

        [ObservableProperty]
        private bool lastAnswerCorrect;

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

        private void Submit()
        {
            Console.WriteLine($"[DEBUG] Answers count: {CurrentQuestion.Answers.Count()}");
            foreach (var a in CurrentQuestion.Answers)
                Console.WriteLine($"[DEBUG] Answer ID {a.Id} | IsSelected={a.IsSelected}");

            var selectedIds = CurrentQuestion.Answers
                .Where(a => a.IsSelected)
                .Select(a => a.Id)
                .ToHashSet();

            var correctIds = CurrentQuestion.Answers
                .Where(a => a.IsCorrect)
                .Select(a => a.Id)
                .ToHashSet();

            var isCorrect = selectedIds.SetEquals(correctIds);
            if (isCorrect)
                _score++;

            LastAnswerCorrect = isCorrect;

            foreach (var answer in CurrentQuestion.Answers)
            {
                answer.IsUserCorrect = selectedIds.Contains(answer.Id) ? answer.IsCorrect : null;
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
