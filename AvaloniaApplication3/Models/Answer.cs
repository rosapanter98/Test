using System.ComponentModel.DataAnnotations;

namespace AvaloniaApplication3.Models
{
    public class Answer
    {
        public int Id { get; set; }

        [Required, MaxLength(500)]
        public string Text { get; set; } = string.Empty;

        public bool IsCorrect { get; set; }

        // FK
        public int QuestionId { get; set; }

        // Nav
        public Question Question { get; set; } = null!;
    }
}
