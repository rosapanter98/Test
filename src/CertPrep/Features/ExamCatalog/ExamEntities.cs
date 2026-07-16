namespace CertPrep.Features.ExamCatalog;

public sealed class Exam
{
    public int Id { get; set; }
    public string Provider { get; set; } = "Microsoft";
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string ContentVersion { get; set; } = string.Empty;
    public bool IsArchived { get; set; }

    public List<ExamObjective> Objectives { get; set; } = [];
    public List<Question> Questions { get; set; } = [];
}

public sealed class ExamObjective
{
    public int Id { get; set; }
    public int ExamId { get; set; }
    public Exam Exam { get; set; } = null!;
    public string ContentKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public List<Question> Questions { get; set; } = [];
}

public enum QuestionKind
{
    SingleChoice,
    MultipleChoice
}

public enum QuestionDifficulty
{
    Foundation,
    Intermediate,
    Advanced
}

public sealed class Question
{
    public int Id { get; set; }
    public int ExamId { get; set; }
    public Exam Exam { get; set; } = null!;
    public int ExamObjectiveId { get; set; }
    public ExamObjective Objective { get; set; } = null!;
    public string ContentKey { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public QuestionKind Kind { get; set; }
    public QuestionDifficulty Difficulty { get; set; }
    public string Explanation { get; set; } = string.Empty;
    public string SourceName { get; set; } = string.Empty;
    public string SourceUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public List<AnswerChoice> Choices { get; set; } = [];
}

public sealed class AnswerChoice
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public Question Question { get; set; } = null!;
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int SortOrder { get; set; }
}
