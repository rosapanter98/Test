namespace CertPrep.Features.Progress;

public enum MasteryTier
{
    Unseen,
    Learning,
    Reliable,
    Mastered
}

public sealed record ObjectiveAttempt(
    int SessionId,
    int SourceQuestionId,
    DateTimeOffset SubmittedAt,
    bool IsCorrect);

public sealed record ObjectiveMasteryInput(
    int ExamId,
    string ExamCode,
    string ExamTitle,
    string ObjectiveContentKey,
    string ObjectiveName,
    int ActiveQuestionCount,
    IReadOnlyList<ObjectiveAttempt> Attempts);

public sealed record ObjectiveMastery(
    int ExamId,
    string ExamCode,
    string ObjectiveContentKey,
    string ObjectiveName,
    MasteryTier Tier,
    int ReadinessPercent,
    int AccuracyPercent,
    int CoveragePercent,
    int AnsweredQuestions,
    int DistinctQuestions,
    int SessionCount,
    bool NeedsReview,
    string NextMilestone);

public sealed record ExamReadiness(
    int ExamId,
    string ExamCode,
    string ExamTitle,
    int ReadinessPercent,
    string Status,
    bool BossUnlocked,
    IReadOnlyList<ObjectiveMastery> Objectives)
{
    public ObjectiveMastery? NearestPromotion => Objectives
        .Where(objective => objective.Tier != MasteryTier.Mastered)
        .OrderByDescending(objective => objective.ReadinessPercent)
        .ThenBy(objective => objective.ObjectiveName)
        .FirstOrDefault();
}

public sealed record MasteryPromotion(
    int ExamId,
    string ExamCode,
    string ObjectiveContentKey,
    string ObjectiveName,
    MasteryTier From,
    MasteryTier To);
