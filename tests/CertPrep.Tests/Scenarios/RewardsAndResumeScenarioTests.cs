using CertPrep.Features.Practice;
using CertPrep.Features.Progress;
using CertPrep.Features.Rewards;
using CertPrep.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CertPrep.Tests.Scenarios;

public sealed class RewardsAndResumeScenarioTests
{
    [Fact]
    public async Task Draft_selection_and_position_resume_from_a_new_service_graph()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var database = await TestDatabase.CreateAsync(cancellationToken: cancellationToken);
        var services = database.CreateServices(randomSeed: 9);
        var exam = (await services.Catalog.GetExamSummariesAsync(cancellationToken))
            .Single(item => item.Code == "AZ-104");
        var run = await services.Practice.StartAsync(exam.Id, PracticeMode.Study, 5, cancellationToken);
        var first = run.Questions[0];
        var selectedChoiceId = first.Choices[0].SessionChoiceId;
        await services.Practice.SaveDraftSelectionAsync(first.SessionItemId, [selectedChoiceId], cancellationToken);

        var restartedServices = database.CreateServices(randomSeed: 99);
        var active = await restartedServices.Practice.GetActiveSessionAsync(cancellationToken);
        Assert.NotNull(active);
        Assert.Equal(run.SessionId, active.SessionId);

        var resumed = await restartedServices.Practice.ResumeAsync(run.SessionId, cancellationToken);
        Assert.NotNull(resumed.Run);
        Assert.Equal(0, resumed.Run.CurrentQuestionIndex);
        Assert.True(resumed.Run.Questions[0].Choices.Single(choice => choice.SessionChoiceId == selectedChoiceId).IsSelected);

        await restartedServices.Practice.SubmitAsync(first.SessionItemId, [selectedChoiceId], cancellationToken);
        var resumedAgain = await database.CreateServices().Practice.ResumeAsync(run.SessionId, cancellationToken);
        Assert.Equal(1, resumedAgain.Run!.CurrentQuestionIndex);
    }

    [Fact]
    public async Task Completed_session_awards_idempotent_xp_and_reconciliation_repairs_missing_entries()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var database = await TestDatabase.CreateAsync(cancellationToken: cancellationToken);
        var services = database.CreateServices(randomSeed: 13);
        var exam = (await services.Catalog.GetExamSummariesAsync(cancellationToken)).First();
        var result = await CompleteCorrectSessionAsync(database, services, exam.Id, cancellationToken);

        Assert.True(result.Rewards.EarnedXp > 20);
        Assert.Contains(result.Rewards.Lines, line => line.Description == "Completed session");
        var repeatedAward = await services.Rewards.AwardCompletedSessionAsync(result.SessionId, cancellationToken);
        Assert.Equal(result.Rewards.EarnedXp, repeatedAward.EarnedXp);
        Assert.Equal(result.Rewards.After.TotalXp, repeatedAward.After.TotalXp);

        await using (var context = database.ContextFactory.CreateDbContext())
        {
            await context.RewardLedgerEntries.ExecuteDeleteAsync(cancellationToken);
        }

        await services.Rewards.ReconcileAsync(cancellationToken);
        var repaired = await services.Rewards.GetOverviewAsync(cancellationToken);
        Assert.Equal(result.Rewards.EarnedXp, repaired.Level.TotalXp);
        Assert.Equal(1, repaired.RewardedSessions);
    }

    [Fact]
    public async Task Recovery_awards_bonus_and_recent_failure_reduces_objective_readiness()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var database = await TestDatabase.CreateAsync(cancellationToken: cancellationToken);
        var services = database.CreateServices(randomSeed: 23);
        var exam = (await services.Catalog.GetExamSummariesAsync(cancellationToken)).First();
        await CompleteCorrectSessionAsync(database, services, exam.Id, cancellationToken);
        var readinessBefore = await services.Mastery.GetExamReadinessAsync(cancellationToken);

        var failedRun = await services.Practice.StartAsync(exam.Id, PracticeMode.Study, 1, cancellationToken);
        var failedQuestion = failedRun.Questions.Single();
        var correctIds = await database.GetCorrectChoiceIdsAsync(
            failedQuestion.Choices.Select(choice => choice.SessionChoiceId),
            cancellationToken);
        var wrongId = failedQuestion.Choices.First(choice => !correctIds.Contains(choice.SessionChoiceId)).SessionChoiceId;
        await services.Practice.SubmitAsync(failedQuestion.SessionItemId, [wrongId], cancellationToken);
        await services.Practice.CompleteAsync(failedRun.SessionId, cancellationToken);

        var readinessAfter = await services.Mastery.GetExamReadinessAsync(cancellationToken);
        var objectiveBefore = readinessBefore.Single(item => item.ExamId == exam.Id)
            .Objectives.Single(item => item.ObjectiveName == failedQuestion.ObjectiveName);
        var objectiveAfter = readinessAfter.Single(item => item.ExamId == exam.Id)
            .Objectives.Single(item => item.ObjectiveName == failedQuestion.ObjectiveName);
        Assert.True(objectiveAfter.ReadinessPercent < objectiveBefore.ReadinessPercent);

        var recovery = await services.Practice.StartReviewAsync([failedQuestion.SourceQuestionId], cancellationToken);
        var recoveryQuestion = recovery.Questions.Single();
        var recoveryCorrectIds = await database.GetCorrectChoiceIdsAsync(
            recoveryQuestion.Choices.Select(choice => choice.SessionChoiceId),
            cancellationToken);
        await services.Practice.SubmitAsync(recoveryQuestion.SessionItemId, recoveryCorrectIds, cancellationToken);
        var recoveryResult = await services.Practice.CompleteAsync(recovery.SessionId, cancellationToken);
        Assert.Contains(recoveryResult.Rewards.Lines, line =>
            line.Description == "Recovered a previously missed question" && line.Amount == 15);
    }

    [Fact]
    public async Task Reliable_categories_unlock_a_balanced_provider_agnostic_boss_exam()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var database = await TestDatabase.CreateAsync(cancellationToken: cancellationToken);
        var services = database.CreateServices(randomSeed: 31);
        var exam = (await services.Catalog.GetExamSummariesAsync(cancellationToken))
            .Single(item => item.Code == "AZ-104");
        for (var session = 0; session < 3; session++)
        {
            await CompleteCorrectSessionAsync(database, services, exam.Id, cancellationToken);
        }

        var readiness = (await services.Mastery.GetExamReadinessAsync(cancellationToken))
            .Single(item => item.ExamId == exam.Id);
        Assert.True(readiness.BossUnlocked);
        Assert.All(readiness.Objectives, objective => Assert.True(objective.Tier >= MasteryTier.Reliable));

        var boss = await services.Practice.StartBossAsync(exam.Id, cancellationToken);
        Assert.Equal(PracticeSessionPurpose.Boss, boss.Purpose);
        Assert.Equal(PracticeMode.ExamSimulation, boss.Mode);
        Assert.Equal(Math.Min(40, exam.QuestionCount), boss.Questions.Count);
        var objectiveCounts = boss.Questions.GroupBy(question => question.ObjectiveName).Select(group => group.Count()).ToList();
        Assert.True(objectiveCounts.Max() - objectiveCounts.Min() <= 1);
    }

    [Fact]
    public async Task Adaptive_sampling_biases_a_recently_failed_question_without_excluding_the_pool()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var database = await TestDatabase.CreateAsync(cancellationToken: cancellationToken);
        var services = database.CreateServices(randomSeed: 47);
        var exam = (await services.Catalog.GetExamSummariesAsync(cancellationToken))
            .Single(item => item.Code == "AZ-900");
        var initial = await services.Practice.StartAsync(exam.Id, PracticeMode.Study, 1, cancellationToken);
        var failedQuestion = initial.Questions.Single();
        var correctIds = await database.GetCorrectChoiceIdsAsync(
            failedQuestion.Choices.Select(choice => choice.SessionChoiceId),
            cancellationToken);
        var wrong = failedQuestion.Choices.First(choice => !correctIds.Contains(choice.SessionChoiceId)).SessionChoiceId;
        await services.Practice.SubmitAsync(failedQuestion.SessionItemId, [wrong], cancellationToken);
        await services.Practice.CompleteAsync(initial.SessionId, cancellationToken);

        var sampledIds = new List<int>();
        for (var sample = 0; sample < 60; sample++)
        {
            var run = await services.Practice.StartAsync(exam.Id, PracticeMode.Study, 1, cancellationToken);
            sampledIds.Add(run.Questions.Single().SourceQuestionId);
        }

        var counts = sampledIds.GroupBy(id => id).ToDictionary(group => group.Key, group => group.Count());
        Assert.True(counts[failedQuestion.SourceQuestionId] > counts
            .Where(pair => pair.Key != failedQuestion.SourceQuestionId)
            .Max(pair => pair.Value));
        Assert.True(counts.Count > 1);
    }

    private static async Task<PracticeResult> CompleteCorrectSessionAsync(
        TestDatabase database,
        TestServices services,
        int examId,
        CancellationToken cancellationToken)
    {
        var exam = (await services.Catalog.GetExamSummariesAsync(cancellationToken)).Single(item => item.Id == examId);
        var run = await services.Practice.StartAsync(
            examId,
            PracticeMode.Study,
            exam.QuestionCount,
            cancellationToken);
        foreach (var question in run.Questions)
        {
            var correctIds = await database.GetCorrectChoiceIdsAsync(
                question.Choices.Select(choice => choice.SessionChoiceId),
                cancellationToken);
            await services.Practice.SubmitAsync(question.SessionItemId, correctIds, cancellationToken);
        }

        return await services.Practice.CompleteAsync(run.SessionId, cancellationToken);
    }
}
