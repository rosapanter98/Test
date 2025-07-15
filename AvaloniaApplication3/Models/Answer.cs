using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvaloniaApplication3.Models
{
    public class Answer
    {
        public int Id { get; set; }

        [Required]
        public string Text { get; set; } = string.Empty;

        public bool IsCorrect { get; set; }

        // Foreign key
        public int QuestionId { get; set; }

        // Navigation
        public Question Question { get; set; } = null!;

        [NotMapped]
        public bool IsSelected { get; set; } // Used at runtime for tracking selected state in multiple choice
    }
}
