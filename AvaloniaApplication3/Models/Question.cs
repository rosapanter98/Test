using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AvaloniaApplication3.Models
{
    public enum QuestionType
    {
        SingleChoice,
        MultipleChoice,
        TrueFalse
    }

    public class Question
    {
        public int Id { get; set; }

        [Required, MaxLength(1000)]
        public string Text { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Explanation { get; set; }

        public QuestionType Type { get; set; }

        // FK
        public int QuizId { get; set; }

        // Nav
        public Quiz Quiz { get; set; } = null!;
        public List<Answer> Answers { get; set; } = new();
    }
}
