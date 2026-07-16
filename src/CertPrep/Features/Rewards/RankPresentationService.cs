using CertPrep.Infrastructure.Settings;

namespace CertPrep.Features.Rewards;

public sealed record RankPresentation(string Title, string Description);

public sealed record ReactionPresentation(string Asset, string Caption);

public sealed class RankPresentationService(AppSettingsStore settingsStore)
{
    private static readonly RankPresentation[] CleanRanks =
    [
        new("New recruit", "Complete a session to enter the campaign."),
        new("Rookie", "The fundamentals are beginning to stick."),
        new("Learner", "Practice is turning guesses into recall."),
        new("Operator", "The weak areas are becoming manageable."),
        new("Specialist", "Consistent results are replacing lucky runs."),
        new("Expert", "The exam objectives are starting to look routine."),
        new("Architect", "Broad knowledge with fewer exposed weak spots."),
        new("Legend", "Maintain readiness and keep the mastered areas healthy.")
    ];

    private static readonly RankPresentation[] RoastRanks =
    [
        new("Freshly Spawned Idiot", "Has achieved absolutely nothing. Impressive."),
        new("Certified Fucking Noob", "Currently losing a fistfight with the practice bank."),
        new("Multiple-Choice Goblin", "Clicks answers with increasingly professional confidence."),
        new("Confidently Dangerous", "Knows enough to be dangerous and not enough to shut up."),
        new("Barely Operational", "Could probably survive a support ticket without supervision."),
        new("Suspiciously Competent", "The wrong answers are starting to look nervous."),
        new("Annoyingly Qualified", "HR may have to update the job title. Tragic."),
        new("Exam-Wrecking Bastard", "The certification exam has booked a therapy appointment.")
    ];

    public bool IsRoastModeEnabled { get; private set; } = true;

    public void Initialize() => IsRoastModeEnabled = settingsStore.Load().RoastModeEnabled;

    public void SetRoastMode(bool enabled)
    {
        IsRoastModeEnabled = enabled;
        settingsStore.SaveRoastMode(enabled);
    }

    public RankPresentation GetRank(LevelProgress level)
    {
        var rankIndex = level.TotalXp == 0 ? 0 : Math.Min(level.Level, RoastRanks.Length - 1);
        return (IsRoastModeEnabled ? RoastRanks : CleanRanks)[rankIndex];
    }

    public ReactionPresentation GetReaction(int scorePercent) => scorePercent switch
    {
        < 50 => new(
            "avares://CertPrep/Assets/Reactions/failing-noob.gif",
            IsRoastModeEnabled ? "Academic crime scene." : "Review the weakest objectives and try again."),
        < 70 => new(
            "avares://CertPrep/Assets/Reactions/confused-goblin.gif",
            IsRoastModeEnabled ? "Technically alive. Spiritually buffering." : "A workable base, with obvious gaps to close."),
        < 85 => new(
            "avares://CertPrep/Assets/Reactions/barely-operational.gif",
            IsRoastModeEnabled ? "A pass is a pass, you lucky goblin." : "A solid result. Tighten the remaining weak areas."),
        _ => new(
            "avares://CertPrep/Assets/Reactions/certified-menace.gif",
            IsRoastModeEnabled ? "Disgustingly competent." : "Strong result. Keep the readiness evidence fresh.")
    };
}
