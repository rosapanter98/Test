using CertPrep.Features.Progress;

namespace CertPrep.Features.Rewards;

public sealed record RewardLine(string Description, int Amount);

public sealed record LevelProgress(
    int Level,
    int TotalXp,
    int XpIntoLevel,
    int XpForNextLevel,
    int ProgressPercent);

public sealed record RewardOutcome(
    int EarnedXp,
    LevelProgress Before,
    LevelProgress After,
    IReadOnlyList<RewardLine> Lines,
    IReadOnlyList<MasteryPromotion> Promotions)
{
    public bool LevelledUp => After.Level > Before.Level;
}

public sealed record RewardOverview(LevelProgress Level, int RewardedSessions);
