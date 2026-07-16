using CertPrep.Features.Practice;
using CertPrep.Features.Progress;
using CertPrep.Features.Rewards;
using CertPrep.Shared;
using CommunityToolkit.Mvvm.Input;

namespace CertPrep.Features.Results;

public partial class ResultsViewModel(
    PracticeResult result,
    IReadOnlyList<ExamReadiness> readiness,
    RankPresentationService rankPresentationService,
    Func<Task> showDashboard,
    Func<Task> showProgress,
    Func<IReadOnlyList<int>, Task> retryMissed) : ViewModelBase
{
    private readonly RankPresentation _rank = rankPresentationService.GetRank(result.Rewards.After);
    private readonly ReactionPresentation _reaction = rankPresentationService.GetReaction(result.ScorePercent);

    public string ExamCode => result.ExamCode;
    public string ExamTitle => result.ExamTitle;
    public int ScorePercent => result.ScorePercent;
    public string ScoreText => $"{result.ScorePercent}%";
    public string CorrectText => $"{result.CorrectAnswers} of {result.TotalQuestions} correct";
    public string DurationText => result.Duration.TotalMinutes < 1
        ? $"{Math.Max(1, (int)result.Duration.TotalSeconds)} sec"
        : $"{(int)result.Duration.TotalMinutes} min {result.Duration.Seconds} sec";
    public string ResultHeading => result.Purpose == PracticeSessionPurpose.Boss
        ? result.ScorePercent >= 80 && result.Objectives.All(objective => objective.ScorePercent >= 60)
            ? "Boss cleared"
            : "Boss incomplete — reinforce the weak objectives"
        : result.ScorePercent >= 80
            ? "Strong run"
            : result.ScorePercent >= 60
                ? "Good base — keep tightening it"
                : "Review the weak areas and run it again";
    public string EarnedXpText => $"+{result.Rewards.EarnedXp} XP";
    public string LevelText => $"Level {result.Rewards.After.Level}";
    public string RankTitle => _rank.Title;
    public string RankDescription => _rank.Description;
    public string ReactionAsset => _reaction.Asset;
    public string ReactionCaption => _reaction.Caption;
    public string LevelProgressText => $"{result.Rewards.After.XpIntoLevel} / {result.Rewards.After.XpForNextLevel} XP";
    public int LevelProgressPercent => result.Rewards.After.ProgressPercent;
    public IReadOnlyList<RewardLineViewModel> RewardLines { get; } =
        result.Rewards.Lines.Select(line => new RewardLineViewModel(line)).ToList();
    public string PromotionText => result.Rewards.Promotions.Count == 0
        ? "No mastery promotion this run. Accuracy still updates readiness."
        : string.Join("  •  ", result.Rewards.Promotions.Select(promotion =>
            $"{promotion.ExamCode} {promotion.ObjectiveName}: {promotion.From} → {promotion.To}"));
    public IReadOnlyList<ObjectiveResultViewModel> Objectives { get; } =
        result.Objectives.Select(objective => new ObjectiveResultViewModel(
            objective,
            readiness
                .FirstOrDefault(exam => exam.ExamCode == objective.ExamCode)?
                .Objectives.FirstOrDefault(mastery =>
                    mastery.ObjectiveContentKey == objective.ObjectiveContentKey))).ToList();
    public IReadOnlyList<MissedQuestionReviewViewModel> MissedQuestions { get; } =
        result.MissedQuestions.Select(question => new MissedQuestionReviewViewModel(question)).ToList();
    public bool HasMissedQuestions => MissedQuestions.Count > 0;
    public string ReviewSummary => HasMissedQuestions
        ? $"{MissedQuestions.Count} missed question{(MissedQuestions.Count == 1 ? string.Empty : "s")} with explanations and correct answers."
        : "Nothing missed in this run.";

    [RelayCommand]
    private async Task DashboardAsync() => await showDashboard();

    [RelayCommand]
    private async Task ProgressAsync() => await showProgress();

    [RelayCommand(CanExecute = nameof(HasMissedQuestions))]
    private async Task RetryMissedAsync() =>
        await retryMissed(MissedQuestions.Select(question => question.SourceQuestionId).ToList());
}

public sealed class ObjectiveResultViewModel(ObjectiveResult result, ObjectiveMastery? mastery)
{
    public string Context => $"{result.ExamCode}  •  {result.Name}";
    public string Name => result.Name;
    public int ScorePercent => result.ScorePercent;
    public string ScoreText => $"{result.ScorePercent}%";
    public string DetailText => mastery is null
        ? $"{result.CorrectAnswers} / {result.TotalQuestions} correct"
        : $"{result.CorrectAnswers} / {result.TotalQuestions} correct  •  {mastery.Tier}  •  {mastery.ReadinessPercent}% readiness";
}

public sealed class RewardLineViewModel(RewardLine line)
{
    public string Description => line.Description;
    public string AmountText => $"+{line.Amount} XP";
}

public sealed class MissedQuestionReviewViewModel(MissedQuestionReview review)
{
    public int SourceQuestionId => review.SourceQuestionId;
    public string Context => $"{review.ExamCode}  •  {review.ObjectiveName}";
    public string Prompt => review.Prompt;
    public string SelectedAnswersText => string.Join("  •  ", review.SelectedAnswers);
    public string CorrectAnswersText => string.Join("  •  ", review.CorrectAnswers);
    public string Explanation => review.Explanation;
    public string SourceText => $"{review.SourceName} — {review.SourceUrl}";
}
