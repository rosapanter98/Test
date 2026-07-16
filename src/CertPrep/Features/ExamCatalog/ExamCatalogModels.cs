namespace CertPrep.Features.ExamCatalog;

public sealed record ExamSummary(
    int Id,
    string Provider,
    string Code,
    string Title,
    string Summary,
    string ContentVersion,
    int QuestionCount,
    IReadOnlyList<string> ObjectiveNames,
    int CompletedSessions,
    int? BestScorePercent,
    int? LastScorePercent);
