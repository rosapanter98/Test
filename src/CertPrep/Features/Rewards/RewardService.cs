using CertPrep.Features.ExamCatalog;
using CertPrep.Features.Practice;
using CertPrep.Features.Progress;

namespace CertPrep.Features.Rewards;

public sealed class RewardService(
    RewardRepository repository,
    ProgressRepository progressRepository,
    MasteryCalculator masteryCalculator,
    LevelCalculator levelCalculator,
    TimeProvider timeProvider)
{
    public const int CurrentRuleVersion = 1;

    public async Task<RewardOutcome> AwardCompletedSessionAsync(
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        var session = await repository.GetSessionAsync(sessionId, cancellationToken)
            ?? throw new InvalidOperationException("The completed session could not be rewarded because it no longer exists.");
        if (session.Status != PracticeSessionStatus.Completed)
        {
            throw new InvalidOperationException("Only completed sessions can award XP.");
        }

        var totalBeforeInsert = await repository.GetTotalXpAsync(cancellationToken);
        var existingSessionEntries = await repository.GetSessionEntriesAsync(sessionId, cancellationToken);
        var proposals = await BuildProposalsAsync(session, cancellationToken);
        await repository.AddMissingEntriesAsync(proposals, cancellationToken);

        var entries = await repository.GetSessionEntriesAsync(sessionId, cancellationToken);
        var totalAfter = await repository.GetTotalXpAsync(cancellationToken);
        var earned = entries.Sum(entry => entry.Amount);
        var totalBefore = existingSessionEntries.Count == 0
            ? totalBeforeInsert
            : totalAfter - earned;
        var promotions = await BuildPromotionsAsync(session, cancellationToken);

        return new RewardOutcome(
            earned,
            levelCalculator.Calculate(totalBefore),
            levelCalculator.Calculate(totalAfter),
            entries.Select(entry => new RewardLine(entry.Description, entry.Amount)).ToList(),
            promotions);
    }

    public async Task ReconcileAsync(CancellationToken cancellationToken = default)
    {
        var sessionIds = await repository.GetUnrewardedCompletedSessionIdsAsync(
            CurrentRuleVersion,
            cancellationToken);
        foreach (var sessionId in sessionIds)
        {
            await AwardCompletedSessionAsync(sessionId, cancellationToken);
        }
    }

    public async Task<RewardOverview> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        var totalXp = await repository.GetTotalXpAsync(cancellationToken);
        var sessions = await repository.GetRewardedSessionCountAsync(cancellationToken);
        return new RewardOverview(levelCalculator.Calculate(totalXp), sessions);
    }

    private async Task<IReadOnlyList<RewardLedgerEntry>> BuildProposalsAsync(
        PracticeSession session,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var entries = new List<RewardLedgerEntry>
        {
            Entry(
                session,
                null,
                RewardKind.SessionCompleted,
                $"session:{session.Id}:complete:v{CurrentRuleVersion}",
                20,
                "Completed session",
                now)
        };
        var submittedHistory = await repository.GetSubmittedItemsAsync(
            session.Items.Select(item => item.SourceExamId).ToList(),
            cancellationToken);

        foreach (var item in session.Items.Where(item => item.SubmittedAt is not null))
        {
            var priorAttempts = submittedHistory
                .Where(previous => previous.SourceQuestionId == item.SourceQuestionId &&
                                   previous.Id != item.Id &&
                                   previous.SubmittedAt < item.SubmittedAt)
                .OrderBy(previous => previous.SubmittedAt)
                .ToList();
            if (item.IsCorrect == true)
            {
                var baseAmount = item.Difficulty switch
                {
                    QuestionDifficulty.Intermediate => 12,
                    QuestionDifficulty.Advanced => 15,
                    _ => 10
                };
                var repeatedRecently = priorAttempts.Any(previous =>
                    previous.IsCorrect == true &&
                    item.SubmittedAt - previous.SubmittedAt <= TimeSpan.FromHours(24));
                var amount = repeatedRecently ? Math.Max(2, baseAmount / 5) : baseAmount;
                entries.Add(Entry(
                    session,
                    item,
                    RewardKind.CorrectAnswer,
                    $"item:{item.Id}:correct:v{CurrentRuleVersion}",
                    amount,
                    repeatedRecently ? "Correct answer — recent repeat" : "Correct answer",
                    now));

                if (priorAttempts.LastOrDefault()?.IsCorrect == false)
                {
                    entries.Add(Entry(
                        session,
                        item,
                        RewardKind.Recovery,
                        $"item:{item.Id}:recovery:v{CurrentRuleVersion}",
                        15,
                        "Recovered a previously missed question",
                        now));
                }
            }
        }

        foreach (var promotion in await BuildPromotionsAsync(session, cancellationToken))
        {
            foreach (var tier in TiersCrossed(promotion.From, promotion.To))
            {
                var amount = tier switch
                {
                    MasteryTier.Reliable => 50,
                    MasteryTier.Mastered => 100,
                    _ => 10
                };
                entries.Add(Entry(
                    session,
                    null,
                    RewardKind.ObjectivePromotion,
                    $"objective:{promotion.ExamId}:{promotion.ObjectiveContentKey}:{tier}:v{CurrentRuleVersion}",
                    amount,
                    $"{promotion.ObjectiveName} → {tier}",
                    now,
                    promotion.ExamCode,
                    promotion.ObjectiveContentKey));
            }
        }

        if (session.Purpose == PracticeSessionPurpose.Boss &&
            session.Items.Count > 0 &&
            Score(session.Items) >= 80 &&
            session.Items.GroupBy(item => item.ObjectiveContentKeySnapshot)
                .All(group => Score(group) >= 60))
        {
            entries.Add(Entry(
                session,
                null,
                RewardKind.BossClear,
                $"boss:{session.ExamId}:{session.ExamCodeSnapshot}:first-clear:v{CurrentRuleVersion}",
                250,
                $"First {session.ExamCodeSnapshot} Boss clear",
                now));
        }

        return entries;
    }

    private async Task<IReadOnlyList<MasteryPromotion>> BuildPromotionsAsync(
        PracticeSession session,
        CancellationToken cancellationToken)
    {
        var inputs = await progressRepository.GetMasteryInputsAsync(cancellationToken);
        var objectiveKeys = session.Items
            .Select(item => (item.SourceExamId, item.ObjectiveContentKeySnapshot))
            .ToHashSet();
        return inputs
            .Where(input => objectiveKeys.Contains((input.ExamId, input.ObjectiveContentKey)))
            .Select(input =>
            {
                var attemptsAtCompletion = input.Attempts
                    .Where(attempt => attempt.SubmittedAt <= session.CompletedAt!.Value)
                    .ToList();
                var before = masteryCalculator.Calculate(input with
                {
                    Attempts = attemptsAtCompletion.Where(attempt => attempt.SessionId != session.Id).ToList()
                });
                var after = masteryCalculator.Calculate(input with { Attempts = attemptsAtCompletion });
                return new MasteryPromotion(
                    input.ExamId,
                    input.ExamCode,
                    input.ObjectiveContentKey,
                    input.ObjectiveName,
                    before.Tier,
                    after.Tier);
            })
            .Where(promotion => promotion.To > promotion.From)
            .ToList();
    }

    private static RewardLedgerEntry Entry(
        PracticeSession session,
        PracticeSessionItem? item,
        RewardKind kind,
        string key,
        int amount,
        string description,
        DateTimeOffset awardedAt,
        string? examCode = null,
        string? objectiveContentKey = null) =>
        new()
        {
            PracticeSessionId = session.Id,
            PracticeSessionItemId = item?.Id,
            ExamCode = examCode ?? item?.ExamCodeSnapshot ?? session.ExamCodeSnapshot,
            ObjectiveContentKey = objectiveContentKey ?? item?.ObjectiveContentKeySnapshot,
            Kind = kind,
            IdempotencyKey = key,
            RuleVersion = CurrentRuleVersion,
            Amount = amount,
            Description = description,
            AwardedAt = awardedAt
        };

    private static IEnumerable<MasteryTier> TiersCrossed(MasteryTier from, MasteryTier to)
    {
        for (var value = (int)from + 1; value <= (int)to; value++)
        {
            yield return (MasteryTier)value;
        }
    }

    private static int Score(IEnumerable<PracticeSessionItem> items)
    {
        var materialized = items.ToList();
        return materialized.Count == 0
            ? 0
            : (int)Math.Round(
                materialized.Count(item => item.IsCorrect == true) * 100d / materialized.Count,
                MidpointRounding.AwayFromZero);
    }
}
