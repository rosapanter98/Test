using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AvaloniaApplication3.Models
{
    public class Quiz
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        // Nav
        public List<Question> Questions { get; set; } = new();
        public List<QuizAttempt> Attempts { get; set; } = new();
    }
}
