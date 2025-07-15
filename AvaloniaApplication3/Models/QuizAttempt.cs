using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        // Foreign keys
        public int QuizId { get; set; }
        public int UserId { get; set; }

        // Navigation
        public Quiz Quiz { get; set; } = null!;
        public User User { get; set; } = null!;


    }
}
