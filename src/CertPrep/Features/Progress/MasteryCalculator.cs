namespace CertPrep.Features.Progress;

public sealed class MasteryCalculator
{
    private const int ReliableAttempts = 6;
    private const int MasteredAttempts = 12;

    public ObjectiveMastery Calculate(ObjectiveMasteryInput input)
    {
        var attempts = input.Attempts
            .OrderBy(attempt => attempt.SubmittedAt)
            .ThenBy(attempt => attempt.SessionId)
            .ToList();
        var tier = HighestEarnedTier(attempts, input.ActiveQuestionCount);
        var accuracy = WeightedAccuracy(attempts.TakeLast(20).ToList());
        var coverage = Percentage(
            attempts.Select(attempt => attempt.SourceQuestionId).Distinct().Count(),
            input.ActiveQuestionCount);
        var confidence = Math.Min(1d, attempts.Count / (double)ReliableAttempts);
        var readiness = (int)Math.Round(
            ((accuracy * 0.8) + (coverage * 0.2)) * (0.4 + (confidence * 0.6)),
            MidpointRounding.AwayFromZero);
        var recentAttempts = attempts.TakeLast(5).ToList();
        var needsReview = tier >= MasteryTier.Reliable &&
            (readiness < (tier == MasteryTier.Mastered ? 75 : 65) ||
             recentAttempts.Count > 0 && !recentAttempts[^1].IsCorrect);

        return new ObjectiveMastery(
            input.ExamId,
            input.ExamCode,
            input.ObjectiveContentKey,
            input.ObjectiveName,
            tier,
            Math.Clamp(readiness, 0, 100),
            accuracy,
            Math.Clamp(coverage, 0, 100),
            attempts.Count,
            attempts.Select(attempt => attempt.SourceQuestionId).Distinct().Count(),
            attempts.Select(attempt => attempt.SessionId).Distinct().Count(),
            needsReview,
            NextMilestone(tier, attempts, coverage));
    }

    public ExamReadiness CalculateExam(IReadOnlyList<ObjectiveMasteryInput> inputs)
    {
        if (inputs.Count == 0)
        {
            throw new ArgumentException("An exam needs at least one objective.", nameof(inputs));
        }

        var objectives = inputs.Select(Calculate).ToList();
        var weightTotal = inputs.Sum(input => Math.Max(1, input.ActiveQuestionCount));
        var weightedAverage = inputs.Zip(objectives).Sum(pair =>
            pair.Second.ReadinessPercent * Math.Max(1, pair.First.ActiveQuestionCount)) / (double)weightTotal;
        var minimum = objectives.Min(objective => objective.ReadinessPercent);
        var readiness = (int)Math.Round(
            (weightedAverage * 0.75) + (minimum * 0.25),
            MidpointRounding.AwayFromZero);
        var allReliable = objectives.All(objective => objective.Tier >= MasteryTier.Reliable);
        var status = readiness switch
        {
            0 => "Not started",
            < 50 => "Early preparation",
            < 70 => "Building knowledge",
            < 85 => "Nearly ready",
            _ when allReliable => "Exam ready",
            _ => "Close — strengthen weak objectives"
        };

        return new ExamReadiness(
            inputs[0].ExamId,
            inputs[0].ExamCode,
            inputs[0].ExamTitle,
            Math.Clamp(readiness, 0, 100),
            status,
            allReliable,
            objectives);
    }

    private static MasteryTier HighestEarnedTier(
        IReadOnlyList<ObjectiveAttempt> attempts,
        int activeQuestionCount)
    {
        var tier = attempts.Count == 0 ? MasteryTier.Unseen : MasteryTier.Learning;
        for (var count = 1; count <= attempts.Count; count++)
        {
            var evidence = attempts.Take(count).ToList();
            var accuracy = WeightedAccuracy(evidence.TakeLast(20).ToList());
            var coverage = Percentage(
                evidence.Select(attempt => attempt.SourceQuestionId).Distinct().Count(),
                activeQuestionCount);
            var sessions = evidence.Select(attempt => attempt.SessionId).Distinct().Count();
            var evidenceSpan = evidence[^1].SubmittedAt - evidence[0].SubmittedAt;

            if (evidence.Count >= MasteredAttempts &&
                accuracy >= 85 &&
                coverage >= 80 &&
                sessions >= 3 &&
                evidenceSpan >= TimeSpan.FromHours(24))
            {
                tier = MasteryTier.Mastered;
                break;
            }

            if (evidence.Count >= ReliableAttempts &&
                accuracy >= 70 &&
                coverage >= 50 &&
                sessions >= 2)
            {
                tier = MasteryTier.Reliable;
            }
        }

        return tier;
    }

    private static int WeightedAccuracy(IReadOnlyList<ObjectiveAttempt> attempts)
    {
        if (attempts.Count == 0)
        {
            return 0;
        }

        var correctWeight = 0d;
        var totalWeight = 0d;
        for (var index = 0; index < attempts.Count; index++)
        {
            var weight = attempts.Count == 1
                ? 1d
                : 1d + (index / (double)(attempts.Count - 1) * 0.5);
            totalWeight += weight;
            if (attempts[index].IsCorrect)
            {
                correctWeight += weight;
            }
        }

        return (int)Math.Round(correctWeight * 100d / totalWeight, MidpointRounding.AwayFromZero);
    }

    private static string NextMilestone(
        MasteryTier tier,
        IReadOnlyCollection<ObjectiveAttempt> attempts,
        int coveragePercent) =>
        tier switch
        {
            MasteryTier.Unseen => "Answer the first question",
            MasteryTier.Learning => $"Reliable: {Math.Min(attempts.Count, ReliableAttempts)} / {ReliableAttempts} answers, {coveragePercent}% coverage",
            MasteryTier.Reliable => $"Mastered: {Math.Min(attempts.Count, MasteredAttempts)} / {MasteredAttempts} answers, {coveragePercent}% coverage across time",
            _ => "Mastery earned — keep readiness healthy"
        };

    private static int Percentage(int value, int total) =>
        total <= 0
            ? 0
            : (int)Math.Round(value * 100d / total, MidpointRounding.AwayFromZero);
}
