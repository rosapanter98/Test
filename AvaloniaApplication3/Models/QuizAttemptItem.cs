using System.Collections.Generic;

namespace AvaloniaApplication3.Models
{
    public class QuizAttemptItem
    {
        public int Id { get; set; }

        public int QuizAttemptId { get; set; }
        public QuizAttempt? QuizAttempt { get; set; }

        // Snapshot of the question at attempt time
        public int QuestionId { get; set; }                  // original FK (reference only)
        public string QuestionText { get; set; } = "";
        public QuestionType QuestionType { get; set; }
        public int OrderIndex { get; set; }                  // 0..N-1 order in this attempt

        // Evaluation
        public bool Submitted { get; set; }
        public bool IsCorrect { get; set; }

        // Snapshot of answers at attempt time + user selection
        public List<QuizAttemptItemAnswer> Answers { get; set; } = new();
    }

    public class QuizAttemptItemAnswer
    {
        public int Id { get; set; }

        public int QuizAttemptItemId { get; set; }
        public QuizAttemptItem? QuizAttemptItem { get; set; }

        public int AnswerId { get; set; }        // original FK (reference only)
        public string Text { get; set; } = "";
        public bool IsCorrect { get; set; }      // snapshot
        public bool IsSelected { get; set; }     // user choice
    }
}
