using CertPrep.Features.Progress;
using Xunit;

namespace CertPrep.Tests.Progress;

public sealed class MasteryCalculatorTests
{
    [Fact]
    public void Readiness_is_generic_recently_weighted_and_a_failure_pulls_it_down()
    {
        var calculator = new MasteryCalculator();
        var startedAt = new DateTimeOffset(2026, 7, 1, 8, 0, 0, TimeSpan.Zero);
        var correctAttempts = Enumerable.Range(0, 10)
            .Select(index => new ObjectiveAttempt(
                (index / 4) + 1,
                (index % 5) + 1,
                startedAt.AddHours(index * 3),
                true))
            .ToList();
        var input = new ObjectiveMasteryInput(
            42,
            "CCNA",
            "Cisco Certified Network Associate",
            "routing-fundamentals",
            "Routing fundamentals",
            5,
            correctAttempts);

        var beforeFailure = calculator.Calculate(input);
        var afterFailure = calculator.Calculate(input with
        {
            Attempts =
            [
                .. correctAttempts,
                new ObjectiveAttempt(4, 2, startedAt.AddHours(32), false)
            ]
        });

        Assert.Equal(MasteryTier.Reliable, beforeFailure.Tier);
        Assert.Equal(MasteryTier.Reliable, afterFailure.Tier);
        Assert.True(afterFailure.ReadinessPercent < beforeFailure.ReadinessPercent);
        Assert.True(afterFailure.NeedsReview);
        Assert.Equal("CCNA", afterFailure.ExamCode);
    }

    [Fact]
    public void Mastered_requires_coverage_multiple_sessions_and_delayed_recall()
    {
        var calculator = new MasteryCalculator();
        var startedAt = new DateTimeOffset(2026, 7, 1, 8, 0, 0, TimeSpan.Zero);
        var attempts = Enumerable.Range(0, 12)
            .Select(index => new ObjectiveAttempt(
                (index / 4) + 1,
                (index % 5) + 1,
                startedAt.AddHours(index * 3),
                true))
            .ToList();

        var mastery = calculator.Calculate(new ObjectiveMasteryInput(
            7,
            "GEN-100",
            "Generic Certification",
            "core-category",
            "Core category",
            5,
            attempts));

        Assert.Equal(MasteryTier.Mastered, mastery.Tier);
        Assert.Equal(100, mastery.ReadinessPercent);
        Assert.Equal(100, mastery.CoveragePercent);
        Assert.False(mastery.NeedsReview);
    }

    [Fact]
    public void Exam_readiness_gives_the_weakest_category_real_weight()
    {
        var calculator = new MasteryCalculator();
        var now = DateTimeOffset.UtcNow;
        var strong = Enumerable.Range(0, 6)
            .Select(index => new ObjectiveAttempt(index < 3 ? 1 : 2, index % 3, now.AddMinutes(index), true))
            .ToList();
        var weak = new[] { new ObjectiveAttempt(1, 10, now, false) };

        var exam = calculator.CalculateExam(
        [
            new ObjectiveMasteryInput(1, "GEN-200", "Generic Exam", "strong", "Strong category", 3, strong),
            new ObjectiveMasteryInput(1, "GEN-200", "Generic Exam", "weak", "Weak category", 3, weak)
        ]);

        Assert.True(exam.ReadinessPercent < exam.Objectives.Average(objective => objective.ReadinessPercent));
        Assert.False(exam.BossUnlocked);
        Assert.Contains("weak", exam.Objectives.Single(objective => objective.ObjectiveContentKey == "weak").ObjectiveContentKey);
    }
}
