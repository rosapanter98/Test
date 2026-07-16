using CertPrep.Features.ExamCatalog;
using CertPrep.Features.Practice;
using CertPrep.Infrastructure.Persistence;
using CertPrep.Tests.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace CertPrep.Tests.Scenarios;

public sealed class PracticeScenarioTests
{
    [Fact]
    public async Task Multiple_choice_questions_expose_and_enforce_the_exact_required_answer_count()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var database = await TestDatabase.CreateAsync(cancellationToken: cancellationToken);
        var services = database.CreateServices(randomSeed: 17);
        var exam = (await services.Catalog.GetExamSummariesAsync(cancellationToken))
            .Single(candidate => candidate.Code == "SC-900");
        var run = await services.Practice.StartAsync(
            exam.Id,
            PracticeMode.Study,
            exam.QuestionCount,
            cancellationToken);
        var question = run.Questions.First(candidate => candidate.Kind == QuestionKind.MultipleChoice);
        var choiceIds = question.Choices.Select(choice => choice.SessionChoiceId).ToList();
        var correctIds = await database.GetCorrectChoiceIdsAsync(choiceIds, cancellationToken);

        Assert.Equal(correctIds.Count, question.RequiredAnswerCount);

        var singleQuestionRun = run with
        {
            CurrentQuestionIndex = 0,
            Questions = [question]
        };
        var viewModel = new PracticeViewModel(
            singleQuestionRun,
            services.Practice,
            _ => Task.CompletedTask,
            _ => Task.CompletedTask);
        Assert.Equal($"Select exactly {correctIds.Count} answers.", viewModel.AnswerInstruction);
        Assert.All(viewModel.Choices, choice =>
        {
            Assert.True(choice.UsesCheckboxIndicator);
            Assert.False(choice.UsesRadioIndicator);
        });
        Assert.False(viewModel.CanSubmit);

        foreach (var choice in viewModel.Choices.Take(correctIds.Count))
        {
            choice.IsSelected = true;
        }

        Assert.True(viewModel.CanSubmit);
        var extraChoice = viewModel.Choices[correctIds.Count];
        extraChoice.IsSelected = true;
        Assert.False(viewModel.CanSubmit);
        extraChoice.IsSelected = false;
        await viewModel.FlushDraftAsync();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            services.Practice.SubmitAsync(
                question.SessionItemId,
                [choiceIds[0]],
                cancellationToken));
        Assert.Contains($"exactly {correctIds.Count}", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Initial_migration_and_seed_create_the_embedded_catalog()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var database = await TestDatabase.CreateAsync(cancellationToken: cancellationToken);
        await using var context = database.ContextFactory.CreateDbContext();

        var questionCounts = await context.Exams
            .OrderBy(exam => exam.Code)
            .Select(exam => new { exam.Code, QuestionCount = exam.Questions.Count })
            .ToDictionaryAsync(exam => exam.Code, exam => exam.QuestionCount, cancellationToken);
        Assert.Equal(
            new Dictionary<string, int>
            {
                ["AZ-104"] = 75,
                ["AZ-700"] = 70,
                ["AZ-900"] = 36,
                ["MD-102"] = 69,
                ["MS-102"] = 64,
                ["SC-200"] = 73,
                ["SC-300"] = 73,
                ["SC-401"] = 71,
                ["SC-900"] = 36
            },
            questionCounts);
        Assert.All(await context.Questions.Include(question => question.Choices).ToListAsync(cancellationToken), question =>
        {
            Assert.True(question.Choices.Count >= 4);
            Assert.Contains(question.Choices, choice => choice.IsCorrect);
            Assert.Contains(question.Choices, choice => !choice.IsCorrect);
        });
    }

    [Fact]
    public async Task Full_simulated_run_scores_exact_answers_and_updates_progress()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var database = await TestDatabase.CreateAsync(cancellationToken: cancellationToken);
        var services = database.CreateServices(randomSeed: 7);
        var exam = (await services.Catalog.GetExamSummariesAsync(cancellationToken)).Single(candidate => candidate.Code == "AZ-900");
        var run = await services.Practice.StartAsync(exam.Id, PracticeMode.Study, exam.QuestionCount, cancellationToken);

        for (var index = 0; index < run.Questions.Count; index++)
        {
            var question = run.Questions[index];
            var allIds = question.Choices.Select(choice => choice.SessionChoiceId).ToHashSet();
            var correctIds = await database.GetCorrectChoiceIdsAsync(allIds, cancellationToken);
            IReadOnlyCollection<int> selectedIds = correctIds;

            if (index == 0)
            {
                selectedIds = [allIds.Except(correctIds).First()];
            }

            var feedback = await services.Practice.SubmitAsync(question.SessionItemId, selectedIds, cancellationToken);
            Assert.Equal(index != 0, feedback.IsCorrect);
        }

        var result = await services.Practice.CompleteAsync(run.SessionId, cancellationToken);
        Assert.Equal(run.Questions.Count - 1, result.CorrectAnswers);
        Assert.Equal(run.Questions.Count, result.TotalQuestions);
        var expectedScore = (int)Math.Round(
            (run.Questions.Count - 1) * 100d / run.Questions.Count,
            MidpointRounding.AwayFromZero);
        Assert.Equal(expectedScore, result.ScorePercent);
        var missed = Assert.Single(result.MissedQuestions);
        Assert.False(string.IsNullOrWhiteSpace(missed.Explanation));
        Assert.False(string.IsNullOrWhiteSpace(missed.SourceUrl));
        Assert.NotEmpty(missed.CorrectAnswers);

        var overview = await services.Progress.GetOverviewAsync(cancellationToken);
        Assert.Equal(1, overview.CompletedSessions);
        Assert.Equal(run.Questions.Count, overview.AnsweredQuestions);
        Assert.Equal(expectedScore, overview.AccuracyPercent);

        var examProgress = (await services.Progress.GetExamProgressAsync(cancellationToken))
            .Single(progress => progress.ExamCode == "AZ-900");
        Assert.Equal(expectedScore, examProgress.BestScorePercent);
        Assert.Equal(3, examProgress.Objectives.Count);
    }

    [Fact]
    public async Task Session_snapshot_survives_question_bank_edits()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var database = await TestDatabase.CreateAsync(cancellationToken: cancellationToken);
        var services = database.CreateServices();
        var exam = (await services.Catalog.GetExamSummariesAsync(cancellationToken)).Single(candidate => candidate.Code == "SC-900");
        var run = await services.Practice.StartAsync(exam.Id, PracticeMode.Study, exam.QuestionCount, cancellationToken);
        var sessionBeforeEdit = await services.PracticeRepository.GetSessionAsync(run.SessionId, cancellationToken);
        var snapshotItem = sessionBeforeEdit!.Items.OrderBy(item => item.OrderIndex).First();
        var originalPrompt = snapshotItem.Prompt;

        await using (var context = database.ContextFactory.CreateDbContext())
        {
            var sourceQuestion = await context.Questions.SingleAsync(
                question => question.Id == snapshotItem.SourceQuestionId,
                cancellationToken);
            sourceQuestion.Prompt = "Edited after the session started";
            await context.SaveChangesAsync(cancellationToken);
        }

        var sessionAfterEdit = await services.PracticeRepository.GetSessionAsync(run.SessionId, cancellationToken);
        Assert.Equal(originalPrompt, sessionAfterEdit!.Items.OrderBy(item => item.OrderIndex).First().Prompt);
    }

    [Fact]
    public async Task Exiting_a_session_marks_it_abandoned()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var database = await TestDatabase.CreateAsync(cancellationToken: cancellationToken);
        var services = database.CreateServices();
        var exam = (await services.Catalog.GetExamSummariesAsync(cancellationToken)).First();
        var run = await services.Practice.StartAsync(exam.Id, PracticeMode.Study, 5, cancellationToken);

        await services.Practice.AbandonAsync(run.SessionId, cancellationToken);

        var session = await services.PracticeRepository.GetSessionAsync(run.SessionId, cancellationToken);
        Assert.Equal(PracticeSessionStatus.Abandoned, session!.Status);
        Assert.Null(session.CompletedAt);
    }

    [Fact]
    public async Task Exam_simulation_view_model_hides_feedback_and_completes_the_run()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var database = await TestDatabase.CreateAsync(cancellationToken: cancellationToken);
        var services = database.CreateServices(randomSeed: 31);
        var exam = (await services.Catalog.GetExamSummariesAsync(cancellationToken))
            .Single(candidate => candidate.Code == "SC-900");
        var run = await services.Practice.StartAsync(
            exam.Id,
            PracticeMode.ExamSimulation,
            5,
            cancellationToken);

        PracticeResult? completedResult = null;
        var viewModel = new PracticeViewModel(
            run,
            services.Practice,
            result =>
            {
                completedResult = result;
                return Task.CompletedTask;
            },
            _ => Task.CompletedTask);

        while (completedResult is null)
        {
            var correctIds = await database.GetCorrectChoiceIdsAsync(
                viewModel.Choices.Select(choice => choice.Id),
                cancellationToken);
            foreach (var choice in viewModel.Choices.Where(choice => correctIds.Contains(choice.Id)))
            {
                choice.IsSelected = true;
            }

            await viewModel.SubmitCommand.ExecuteAsync(null);
            Assert.False(viewModel.ShowFeedback);
            Assert.Null(viewModel.ErrorMessage);
        }

        Assert.Equal(100, completedResult.ScorePercent);
        Assert.Equal(5, completedResult.TotalQuestions);
    }

    [Fact]
    public async Task Mixed_session_honors_the_cap_balances_sources_and_randomizes_order()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var database = await TestDatabase.CreateAsync(cancellationToken: cancellationToken);
        var exams = (await database.CreateServices().Catalog.GetExamSummariesAsync(cancellationToken))
            .Where(exam => exam.Code is "AZ-900" or "SC-900")
            .ToList();
        var examIds = exams.Select(exam => exam.Id).ToList();

        var tooShort = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            database.CreateServices().Practice.StartAsync(
                examIds,
                PracticeMode.Study,
                1,
                cancellationToken));
        Assert.Contains("at least one question per exam", tooShort.Message);

        var first = await database.CreateServices(randomSeed: 17).Practice.StartAsync(
            examIds,
            PracticeMode.Study,
            5,
            cancellationToken);
        var second = await database.CreateServices(randomSeed: 71).Practice.StartAsync(
            examIds,
            PracticeMode.Study,
            5,
            cancellationToken);

        Assert.Equal(PracticeSessionScope.MixedExam, first.Scope);
        Assert.Equal("MIXED", first.ExamCode);
        Assert.Equal(5, first.Questions.Count);
        Assert.Equal(5, first.Questions.Select(question => question.SourceQuestionId).Distinct().Count());
        Assert.Equal(2, first.Questions.Select(question => question.ExamCode).Distinct().Count());
        var sourceCounts = first.Questions.GroupBy(question => question.ExamCode).Select(group => group.Count()).ToList();
        Assert.True(sourceCounts.Max() - sourceCounts.Min() <= 1);
        Assert.NotEqual(
            first.Questions.Select(question => question.SourceQuestionId),
            second.Questions.Select(question => question.SourceQuestionId));

        var persisted = await database.CreateServices().PracticeRepository.GetSessionAsync(first.SessionId, cancellationToken);
        Assert.Null(persisted!.ExamId);
        Assert.Equal(PracticeSessionScope.MixedExam, persisted.Scope);
        Assert.All(persisted.Items, item =>
        {
            Assert.True(item.SourceExamId > 0);
            Assert.False(string.IsNullOrWhiteSpace(item.ExamCodeSnapshot));
            Assert.False(string.IsNullOrWhiteSpace(item.ExamTitleSnapshot));
        });
    }

    [Fact]
    public async Task Mixed_exam_review_explains_misses_and_retry_builds_a_study_run()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var database = await TestDatabase.CreateAsync(cancellationToken: cancellationToken);
        var services = database.CreateServices(randomSeed: 23);
        var exams = (await services.Catalog.GetExamSummariesAsync(cancellationToken))
            .Where(exam => exam.Code is "AZ-900" or "SC-900")
            .ToList();
        var run = await services.Practice.StartAsync(
            exams.Select(exam => exam.Id).ToList(),
            PracticeMode.ExamSimulation,
            5,
            cancellationToken);

        for (var index = 0; index < run.Questions.Count; index++)
        {
            var question = run.Questions[index];
            var allIds = question.Choices.Select(choice => choice.SessionChoiceId).ToHashSet();
            var correctIds = await database.GetCorrectChoiceIdsAsync(allIds, cancellationToken);
            IReadOnlyCollection<int> selectedIds = index == 0
                ? [allIds.Except(correctIds).First()]
                : correctIds;
            await services.Practice.SubmitAsync(question.SessionItemId, selectedIds, cancellationToken);
        }

        var result = await services.Practice.CompleteAsync(run.SessionId, cancellationToken);
        Assert.Equal(80, result.ScorePercent);
        var missed = Assert.Single(result.MissedQuestions);
        Assert.NotEmpty(missed.SelectedAnswers);
        Assert.NotEmpty(missed.CorrectAnswers);
        Assert.False(string.IsNullOrWhiteSpace(missed.Explanation));
        Assert.False(string.IsNullOrWhiteSpace(missed.SourceUrl));

        var retry = await services.Practice.StartReviewAsync([missed.SourceQuestionId], cancellationToken);
        Assert.Equal(PracticeMode.Study, retry.Mode);
        var retryQuestion = Assert.Single(retry.Questions);
        Assert.Equal(missed.SourceQuestionId, retryQuestion.SourceQuestionId);
        var retryCorrectIds = await database.GetCorrectChoiceIdsAsync(
            retryQuestion.Choices.Select(choice => choice.SessionChoiceId),
            cancellationToken);
        var retryFeedback = await services.Practice.SubmitAsync(
            retryQuestion.SessionItemId,
            retryCorrectIds,
            cancellationToken);
        Assert.True(retryFeedback.IsCorrect);
        Assert.False(string.IsNullOrWhiteSpace(retryFeedback.Explanation));
        var retryResult = await services.Practice.CompleteAsync(retry.SessionId, cancellationToken);
        Assert.Equal(100, retryResult.ScorePercent);

        var overview = await services.Progress.GetOverviewAsync(cancellationToken);
        Assert.Equal(2, overview.CompletedSessions);
        Assert.Contains("Cross-exam", overview.Momentum.UnlockedBadges);
        Assert.Contains("Clean sweep", overview.Momentum.UnlockedBadges);

        var progress = await services.Progress.GetExamProgressAsync(cancellationToken);
        var selectedExamCodes = exams.Select(exam => exam.Code).ToHashSet();
        Assert.All(
            progress.Where(examProgress => selectedExamCodes.Contains(examProgress.ExamCode)),
            examProgress => Assert.True(examProgress.AnsweredQuestions > 0));
        Assert.All(
            progress.Where(examProgress => !selectedExamCodes.Contains(examProgress.ExamCode)),
            examProgress => Assert.Equal(0, examProgress.AnsweredQuestions));
        var summaries = await services.Catalog.GetExamSummariesAsync(cancellationToken);
        Assert.All(
            summaries.Where(summary => selectedExamCodes.Contains(summary.Code)),
            summary => Assert.True(summary.CompletedSessions > 0));
        Assert.All(
            summaries.Where(summary => !selectedExamCodes.Contains(summary.Code)),
            summary => Assert.Equal(0, summary.CompletedSessions));
    }

    [Fact]
    public async Task Mixed_session_migration_backfills_existing_single_exam_history()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(cancellationToken);
        var options = new DbContextOptionsBuilder<StudyDbContext>().UseSqlite(connection).Options;
        await using var context = new StudyDbContext(options);
        var migrator = context.GetService<IMigrator>();
        await migrator.MigrateAsync("20260716053704_InitialStudySchema", cancellationToken);

        await context.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO Exams (Provider, Code, Title, Summary, ContentVersion, IsArchived)
            VALUES ('Microsoft', 'TEST-100', 'Existing exam', 'Migration fixture', 'v1', 0);

            INSERT INTO PracticeSessions
                (ExamId, ExamCodeSnapshot, ExamTitleSnapshot, Mode, Status, StartedAt, CompletedAt, CurrentItemIndex)
            VALUES
                ((SELECT Id FROM Exams WHERE Code = 'TEST-100'), 'TEST-100', 'Existing exam', 'Study', 'Completed',
                 '2026-07-15 10:00:00+00:00', '2026-07-15 10:01:00+00:00', 1);

            INSERT INTO PracticeSessionItems
                (PracticeSessionId, SourceQuestionId, OrderIndex, ObjectiveName, Prompt, Kind, Difficulty,
                 Explanation, SourceName, SourceUrl, SubmittedAt, IsCorrect)
            VALUES
                ((SELECT Id FROM PracticeSessions), 99, 0, 'Existing objective', 'Existing prompt', 'SingleChoice',
                 'Foundation', 'Existing explanation', 'Existing source', 'https://example.test',
                 '2026-07-15 10:00:30+00:00', 1);
            """,
            cancellationToken);

        await migrator.MigrateAsync(cancellationToken: cancellationToken);
        context.ChangeTracker.Clear();
        var migrated = await context.PracticeSessions
            .AsNoTracking()
            .Include(session => session.Items)
            .SingleAsync(cancellationToken);

        Assert.Equal(PracticeSessionScope.SingleExam, migrated.Scope);
        var item = Assert.Single(migrated.Items);
        Assert.Equal(migrated.ExamId, item.SourceExamId);
        Assert.Equal("TEST-100", item.ExamCodeSnapshot);
        Assert.Equal("Existing exam", item.ExamTitleSnapshot);
    }
}
