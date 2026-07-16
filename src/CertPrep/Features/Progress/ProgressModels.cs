namespace CertPrep.Features.Progress;

public sealed record StudyOverview(
    int ExamCount,
    int CompletedSessions,
    int AnsweredQuestions,
    int AccuracyPercent,
    StudyMomentum Momentum);

public sealed record StudyMomentum(
    string Title,
    string Message,
    int ProgressPercent,
    string ProgressText,
    IReadOnlyList<string> UnlockedBadges);

public sealed record ObjectiveProgress(
    string ContentKey,
    string Name,
    int AnsweredQuestions,
    int CorrectAnswers,
    int AccuracyPercent);

public sealed record ExamProgress(
    int ExamId,
    string ExamCode,
    string ExamTitle,
    int CompletedSessions,
    int AnsweredQuestions,
    int AccuracyPercent,
    int? BestScorePercent,
    IReadOnlyList<ObjectiveProgress> Objectives);
