using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using AvaloniaApplication3.Models;
using AvaloniaApplication3.Services;
using AvaloniaApplication3.Utility;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaApplication3.ViewModels
{
    public partial class QuizRunnerViewModel : ViewModelBase
    {
        private readonly User _user;
        private readonly Quiz _quiz;
        private readonly IQuizAttemptService _attempts;
        private readonly Action<string, int, int> _onQuizCompleted;

        private readonly List<Question> _questionList;
        private QuizAttempt _attempt = null!;
        private int _currentIndex = 0;
        private int _score = 0;

        [ObservableProperty] private Question currentQuestion = null!;
        [ObservableProperty] private bool showFeedback;
        [ObservableProperty] private string? feedbackText;
        [ObservableProperty] private bool quizCompleted;
        [ObservableProperty] private bool isTrueFalse, isMultipleChoice, isSingleChoice;
        [ObservableProperty] private bool? lastAnswerCorrect;

        public ObservableCollection<AnswerOptionViewModel> CurrentAnswers { get; } = new();

        public int TotalQuestions => _questionList.Count;
        public int CurrentIndex => _currentIndex + 1;
        public int Score => _score;
        public int TotalCorrect => _score;
        public string? Explanation => FeedbackText;

        public bool CanSubmitAnswer => !ShowFeedback && CurrentAnswers.Any(a => a.IsSelected);
        public bool CanGoNext => ShowFeedback && !QuizCompleted;

        public QuizRunnerViewModel(
            User user,
            Quiz quiz,
            Action<string, int, int> onQuizCompleted,
            IQuizAttemptService attempts)
        {
            _user = user ?? throw new ArgumentNullException(nameof(user));
            _quiz = quiz ?? throw new ArgumentNullException(nameof(quiz));
            _attempts = attempts ?? throw new ArgumentNullException(nameof(attempts));
            _onQuizCompleted = onQuizCompleted;

            // NOTE: later you can randomize/take N here before starting the attempt
            _questionList = _quiz.Questions.ToList();

            // start persisted attempt (snapshot)
            _attempt = _attempts
                .StartAttemptAsync(_user, _quiz, _questionList)
                .GetAwaiter().GetResult();

            LoadQuestion();
        }

        private void LoadQuestion()
        {
            if (_questionList.Count == 0)
                throw new InvalidOperationException("Quiz has no questions.");

            CurrentQuestion = _questionList[_currentIndex];

            ShowFeedback = false;
            FeedbackText = null;
            LastAnswerCorrect = null;

            UnwireAnswerSelectionChanged();
            CurrentAnswers.Clear();
            foreach (var vm in QuizLogic.BuildAnswerVMs(CurrentQuestion))
                CurrentAnswers.Add(vm);
            WireAnswerSelectionChanged();

            IsSingleChoice = CurrentQuestion.Type == QuestionType.SingleChoice;
            IsMultipleChoice = CurrentQuestion.Type == QuestionType.MultipleChoice;
            IsTrueFalse = CurrentQuestion.Type == QuestionType.TrueFalse;

            SubmitCommand.NotifyCanExecuteChanged();
            NextCommand.NotifyCanExecuteChanged();
        }

        private void WireAnswerSelectionChanged()
        {
            foreach (var vm in CurrentAnswers)
                vm.PropertyChanged += Answer_PropertyChanged;
        }

        private void UnwireAnswerSelectionChanged()
        {
            foreach (var vm in CurrentAnswers)
                vm.PropertyChanged -= Answer_PropertyChanged;
        }

        private void Answer_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AnswerOptionViewModel.IsSelected))
            {
                if (IsSingleChoice || IsTrueFalse)
                    QuizLogic.EnforceSingleChoice(CurrentAnswers, (AnswerOptionViewModel)sender!);

                SubmitCommand.NotifyCanExecuteChanged();
            }
        }

        partial void OnShowFeedbackChanged(bool value)
        {
            SubmitCommand.NotifyCanExecuteChanged();
            NextCommand.NotifyCanExecuteChanged();
        }

        // Persist the current item’s selection, then show feedback
        [RelayCommand(CanExecute = nameof(CanSubmitAnswer))]
        private async Task Submit()
        {
            var (isCorrect, selectedIds, _) = QuizLogic.Evaluate(CurrentAnswers);

            // persist this item’s selection
            var item = _attempt.Items[_currentIndex];
            await _attempts.SubmitItemAsync(item.Id, selectedIds);

            if (isCorrect) _score++;
            LastAnswerCorrect = isCorrect;

            QuizLogic.ApplyPerAnswerFeedback(CurrentAnswers);
            FeedbackText = QuizLogic.BuildFeedbackText(CurrentQuestion, isCorrect);
            ShowFeedback = true;
        }

        // Next question; on last, complete the attempt and invoke callback
        [RelayCommand(CanExecute = nameof(CanGoNext))]
        private async Task Next()
        {
            _currentIndex++;
            if (_currentIndex >= _questionList.Count)
            {
                var completed = await _attempts.CompleteAttemptAsync(_attempt.Id);
                QuizCompleted = true;

                // keep your existing signature for now
                _onQuizCompleted.Invoke(_quiz.Title, _score, _questionList.Count);
                // (Optionally pass completed.Id to results VM later if you want to reload)
            }
            else
            {
                LoadQuestion();
            }
        }
    }
}
