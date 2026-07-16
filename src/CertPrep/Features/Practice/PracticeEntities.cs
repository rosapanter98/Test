using CertPrep.Features.ExamCatalog;

namespace CertPrep.Features.Practice;

public enum PracticeMode
{
    Study,
    ExamSimulation
}

public enum PracticeSessionStatus
{
    InProgress,
    Completed,
    Abandoned
}

public enum PracticeSessionScope
{
    SingleExam,
    MixedExam
}

public enum PracticeSessionPurpose
{
    Standard,
    Review,
    Boss
}

public sealed class PracticeSession
{
    public int Id { get; set; }
    public int? ExamId { get; set; }
    public Exam? Exam { get; set; }
    public string ExamCodeSnapshot { get; set; } = string.Empty;
    public string ExamTitleSnapshot { get; set; } = string.Empty;
    public PracticeSessionScope Scope { get; set; }
    public PracticeMode Mode { get; set; }
    public PracticeSessionPurpose Purpose { get; set; }
    public PracticeSessionStatus Status { get; set; } = PracticeSessionStatus.InProgress;
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public int CurrentItemIndex { get; set; }

    public List<PracticeSessionItem> Items { get; set; } = [];
}

public sealed class PracticeSessionItem
{
    public int Id { get; set; }
    public int PracticeSessionId { get; set; }
    public PracticeSession PracticeSession { get; set; } = null!;
    public int SourceQuestionId { get; set; }
    public int SourceExamId { get; set; }
    public Exam SourceExam { get; set; } = null!;
    public string ExamCodeSnapshot { get; set; } = string.Empty;
    public string ExamTitleSnapshot { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public string ObjectiveContentKeySnapshot { get; set; } = string.Empty;
    public string ObjectiveName { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public QuestionKind Kind { get; set; }
    public QuestionDifficulty Difficulty { get; set; }
    public string Explanation { get; set; } = string.Empty;
    public string SourceName { get; set; } = string.Empty;
    public string SourceUrl { get; set; } = string.Empty;
    public DateTimeOffset? SubmittedAt { get; set; }
    public bool? IsCorrect { get; set; }

    public List<PracticeSessionChoice> Choices { get; set; } = [];
}

public sealed class PracticeSessionChoice
{
    public int Id { get; set; }
    public int PracticeSessionItemId { get; set; }
    public PracticeSessionItem PracticeSessionItem { get; set; } = null!;
    public int SourceAnswerChoiceId { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public bool IsSelected { get; set; }
    public int SortOrder { get; set; }
}
