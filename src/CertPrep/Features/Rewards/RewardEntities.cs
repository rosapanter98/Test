using CertPrep.Features.Practice;

namespace CertPrep.Features.Rewards;

public enum RewardKind
{
    SessionCompleted,
    CorrectAnswer,
    Recovery,
    ObjectivePromotion,
    BossClear
}

public sealed class RewardLedgerEntry
{
    public int Id { get; set; }
    public int PracticeSessionId { get; set; }
    public PracticeSession PracticeSession { get; set; } = null!;
    public int? PracticeSessionItemId { get; set; }
    public string ExamCode { get; set; } = string.Empty;
    public string? ObjectiveContentKey { get; set; }
    public RewardKind Kind { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public int RuleVersion { get; set; }
    public int Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset AwardedAt { get; set; }
}
