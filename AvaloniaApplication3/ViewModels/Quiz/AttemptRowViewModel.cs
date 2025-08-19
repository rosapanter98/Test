// ViewModels/History/AttemptRowViewModel.cs
using System;
using System.Collections.ObjectModel;
using AvaloniaApplication3.Models;

namespace AvaloniaApplication3.ViewModels.Quizzes
{
    public class AttemptRowViewModel
    {
        public int AttemptId { get; }
        public string QuizTitle { get; }
        public DateTimeOffset StartedAt { get; }
        public DateTimeOffset? CompletedAt { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; }

        public bool HasDetailsLoaded => Items.Count > 0;

        public ObservableCollection<AttemptItemRowViewModel> Items { get; } = new();

        public AttemptRowViewModel(QuizAttempt a)
        {
            AttemptId = a.Id;
            QuizTitle = a.Quiz?.Title ?? "(unknown)";
            StartedAt = a.StartedAt;
            CompletedAt = a.CompletedAt;
            CorrectAnswers = a.CorrectAnswers;
            TotalQuestions = a.TotalQuestions;
        }
    }

    public class AttemptItemRowViewModel
    {
        private readonly QuizAttemptItem _item;
        public AttemptItemRowViewModel(QuizAttemptItem item)
        {
            _item = item;
            // copy answers into observable for UI
            foreach (var ans in item.Answers)
                Answers.Add(ans);
        }

        public string QuestionText => _item.QuestionText;
        public bool IsCorrect => _item.IsCorrect;

        // We can bind directly to model answers; they are immutable snapshots.
        public ObservableCollection<QuizAttemptItemAnswer> Answers { get; } = new();
    }
}
