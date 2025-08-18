using AvaloniaApplication3.Models.Enums;
using System;
using System.Collections.Generic;

namespace AvaloniaApplication3.Models
{
    public class QuizAttempt
    {
        public int Id { get; set; }

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }

        public TimeSpan? TimeTaken => CompletedAt.HasValue ? CompletedAt.Value - StartedAt : null;

        public int CurrentIndex { get; set; }          // 0-based index into Items
        public AttemptStatus Status { get; set; } = AttemptStatus.InProgress;

        // Foreign keys (for reporting/filtering)
        public int QuizId { get; set; }
        public int UserId { get; set; }

        // Navigation to original (optional)
        public Quiz Quiz { get; set; } = null!;
        public User User { get; set; } = null!;

        // Snapshot of all items in this attempt (immutable content)
        public List<QuizAttemptItem> Items { get; set; } = new();
    }
}
