using AvaloniaApplication3.Models.Enums;
using System;
using System.Collections.Generic;

namespace AvaloniaApplication3.Models
{
    public class QuizAttempt
    {
        public int Id { get; set; }

        public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? CompletedAt { get; set; }

        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }

        public TimeSpan? TimeTaken =>
            CompletedAt.HasValue ? CompletedAt.Value - StartedAt : null;

        public int CurrentIndex { get; set; }
        public AttemptStatus Status { get; set; } = AttemptStatus.InProgress;

        // FKs
        public int QuizId { get; set; }
        public int UserId { get; set; }

        // Nav
        public Quiz Quiz { get; set; } = null!;
        public User User { get; set; } = null!;
        public List<QuizAttemptItem> Items { get; set; } = new();
    }
}
