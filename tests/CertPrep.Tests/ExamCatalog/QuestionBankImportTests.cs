using CertPrep.Features.ExamCatalog;
using CertPrep.Features.ExamCatalog.Importing;
using CertPrep.Features.Practice;
using CertPrep.Infrastructure.Persistence;
using CertPrep.Tests.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CertPrep.Tests.ExamCatalog;

public sealed class QuestionBankImportTests
{
    [Fact]
    public async Task Embedded_exam_banks_follow_the_content_quality_contract()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var package = await new QuestionBankPackageReader().ReadEmbeddedAsync(cancellationToken);

        QuestionBankMerger.Validate(package);

        var expectedCounts = new Dictionary<string, int>
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
        };

        Assert.Equal(
            expectedCounts,
            package.Exams.ToDictionary(exam => exam.Code, exam => exam.Questions.Count));

        var prompts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var exam in package.Exams)
        {
            Assert.All(exam.Objectives, objective =>
                Assert.Contains(exam.Questions, question => question.ObjectiveKey == objective.ContentKey));
            Assert.All(exam.Questions, question =>
            {
                Assert.True(prompts.Add(question.Prompt), $"Duplicate prompt: {question.Prompt}");
                Assert.InRange(question.Choices.Count, 4, 6);
                Assert.Equal(
                    question.Choices.Count,
                    question.Choices.Select(choice => choice.Text).Distinct(StringComparer.OrdinalIgnoreCase).Count());
                Assert.Equal("https", new Uri(question.SourceUrl).Scheme);
                Assert.Equal("learn.microsoft.com", new Uri(question.SourceUrl).Host);
                Assert.DoesNotContain(question.Choices, choice =>
                    choice.Text.Equals("All of the above", StringComparison.OrdinalIgnoreCase) ||
                    choice.Text.Equals("None of the above", StringComparison.OrdinalIgnoreCase));
                if (question.Kind == QuestionKind.MultipleChoice)
                {
                    var correctCount = question.Choices.Count(choice => choice.IsCorrect);
                    Assert.True(correctCount >= 2);
                    var countWord = correctCount switch
                    {
                        2 => "two",
                        3 => "three",
                        4 => "four",
                        _ => correctCount.ToString()
                    };
                    Assert.True(
                        question.Prompt.Contains(countWord, StringComparison.OrdinalIgnoreCase) ||
                        question.Prompt.Contains(correctCount.ToString(), StringComparison.OrdinalIgnoreCase),
                        $"Multiple-choice question '{exam.Code}/{question.ContentKey}' does not state its answer count.");
                }
            });

            var expectedScenarioCount = exam.Code switch
            {
                "AZ-104" or "AZ-700" => 10,
                "AZ-900" or "SC-900" => 5,
                _ => 8
            };
            Assert.True(
                exam.Questions.Count(question => question.ContentKey.Contains("-scenario-", StringComparison.OrdinalIgnoreCase)) >= expectedScenarioCount,
                $"{exam.Code} does not contain the expected scenario-question coverage.");
            Assert.Contains(exam.Questions, question => question.Choices.Count == 5);
            Assert.Contains(exam.Questions, question => question.Choices.Count == 6);

            if (exam.Code is not ("AZ-900" or "SC-900"))
            {
                Assert.True(
                    exam.Questions.Count(question => question.ContentKey.Contains("-order-", StringComparison.OrdinalIgnoreCase)) >= 2,
                    $"{exam.Code} needs at least two ordering scenarios.");
            }
        }
    }

    [Fact]
    public async Task Compatible_database_merges_catalog_by_content_key_without_importing_history()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var sourceDirectory = Path.Combine(Path.GetTempPath(), "CertPrep.Tests", Guid.NewGuid().ToString("N"));
        var sourcePath = Path.Combine(sourceDirectory, "source.sqlite");
        Directory.CreateDirectory(sourceDirectory);

        try
        {
            var sourceFactory = await CreateSourceDatabaseAsync(sourcePath, cancellationToken);
            await using (var source = sourceFactory.CreateDbContext())
            {
                var exam = await source.Exams
                    .Include(item => item.Objectives)
                    .Include(item => item.Questions)
                    .SingleAsync(item => item.Code == "AZ-900", cancellationToken);
                exam.Title = "Microsoft Azure Fundamentals — imported revision";
                exam.ContentVersion = "Imported bank 2.0";
                exam.Questions.Single(item => item.ContentKey == "azure-dns").Explanation =
                    "Imported explanation revision.";

                var objective = exam.Objectives.Single(item => item.ContentKey == "azure-architecture");
                exam.Questions.Add(new Question
                {
                    Exam = exam,
                    Objective = objective,
                    ContentKey = "resource-group-boundary",
                    Prompt = "Which Azure construct groups related resources for lifecycle management?",
                    Kind = QuestionKind.SingleChoice,
                    Difficulty = QuestionDifficulty.Foundation,
                    Explanation = "A resource group is a logical container for related Azure resources.",
                    SourceName = "Azure resource management",
                    SourceUrl = "https://learn.microsoft.com/azure/azure-resource-manager/management/manage-resource-groups-portal",
                    Choices =
                    [
                        new AnswerChoice { Text = "Resource group", IsCorrect = true, SortOrder = 0 },
                        new AnswerChoice { Text = "Availability zone", IsCorrect = false, SortOrder = 1 }
                    ]
                });
                source.PracticeSessions.Add(new PracticeSession
                {
                    Exam = exam,
                    ExamCodeSnapshot = exam.Code,
                    ExamTitleSnapshot = exam.Title,
                    Scope = PracticeSessionScope.SingleExam,
                    Mode = PracticeMode.Study,
                    Status = PracticeSessionStatus.Completed,
                    StartedAt = DateTimeOffset.UtcNow,
                    CompletedAt = DateTimeOffset.UtcNow
                });
                await source.SaveChangesAsync(cancellationToken);
            }

            await using var target = await TestDatabase.CreateAsync(cancellationToken: cancellationToken);
            int initialQuestionCount;
            await using (var context = target.ContextFactory.CreateDbContext())
            {
                initialQuestionCount = await context.Questions.CountAsync(cancellationToken);
            }
            var services = target.CreateServices();
            var first = await services.Importer.ImportSqliteAsync(sourcePath, cancellationToken);

            Assert.Equal(0, first.ExamsAdded);
            Assert.Equal(1, first.ExamsUpdated);
            Assert.Equal(1, first.QuestionsAdded);
            Assert.Equal(1, first.QuestionsUpdated);

            await using (var context = target.ContextFactory.CreateDbContext())
            {
                var importedExam = await context.Exams.SingleAsync(item => item.Code == "AZ-900", cancellationToken);
                Assert.Equal("Microsoft Azure Fundamentals — imported revision", importedExam.Title);
                Assert.Equal("Imported bank 2.0", importedExam.ContentVersion);
                Assert.Equal(initialQuestionCount + 1, await context.Questions.CountAsync(cancellationToken));
                Assert.Equal("Imported explanation revision.",
                    await context.Questions
                        .Where(item => item.ContentKey == "azure-dns")
                        .Select(item => item.Explanation)
                        .SingleAsync(cancellationToken));
                Assert.Equal(0, await context.PracticeSessions.CountAsync(cancellationToken));
            }

            var second = await services.Importer.ImportSqliteAsync(sourcePath, cancellationToken);
            Assert.Equal(new QuestionBankMergeResult(0, 0, 0, 0), second);
        }
        finally
        {
            if (Directory.Exists(sourceDirectory))
            {
                Directory.Delete(sourceDirectory, true);
            }
        }
    }

    [Fact]
    public async Task Incompatible_database_is_rejected_before_target_changes()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var sourceDirectory = Path.Combine(Path.GetTempPath(), "CertPrep.Tests", Guid.NewGuid().ToString("N"));
        var sourcePath = Path.Combine(sourceDirectory, "incompatible.sqlite");
        Directory.CreateDirectory(sourceDirectory);

        try
        {
            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = sourcePath,
                Pooling = false
            }.ToString();
            await using (var connection = new SqliteConnection(connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                await using var command = connection.CreateCommand();
                command.CommandText = "CREATE TABLE Exams (Id INTEGER PRIMARY KEY, Code TEXT NOT NULL);";
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            await using var target = await TestDatabase.CreateAsync(cancellationToken: cancellationToken);
            int initialExamCount;
            int initialQuestionCount;
            int initialDistinctQuestionCount;
            await using (var context = target.ContextFactory.CreateDbContext())
            {
                initialExamCount = await context.Exams.CountAsync(cancellationToken);
                initialQuestionCount = await context.Questions.CountAsync(cancellationToken);
                initialDistinctQuestionCount = await context.Questions
                    .Select(item => new { item.ExamId, item.ContentKey })
                    .Distinct()
                    .CountAsync(cancellationToken);
            }
            var services = target.CreateServices();
            await Assert.ThrowsAsync<QuestionBankValidationException>(
                () => services.Importer.ImportSqliteAsync(sourcePath, cancellationToken));

            await using var verificationContext = target.ContextFactory.CreateDbContext();
            Assert.Equal(initialExamCount, await verificationContext.Exams.CountAsync(cancellationToken));
            Assert.Equal(initialQuestionCount, await verificationContext.Questions.CountAsync(cancellationToken));
            Assert.Equal(initialDistinctQuestionCount, await verificationContext.Questions
                .Select(item => new { item.ExamId, item.ContentKey })
                .Distinct()
                .CountAsync(cancellationToken));
        }
        finally
        {
            if (Directory.Exists(sourceDirectory))
            {
                Directory.Delete(sourceDirectory, true);
            }
        }
    }

    private static async Task<TestDbContextFactory> CreateSourceDatabaseAsync(
        string path,
        CancellationToken cancellationToken)
    {
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = path,
            Pooling = false
        }.ToString();
        var options = new DbContextOptionsBuilder<StudyDbContext>()
            .UseSqlite(connectionString)
            .Options;
        var factory = new TestDbContextFactory(options);
        await using var context = factory.CreateDbContext();
        await context.Database.MigrateAsync(cancellationToken);
        await new DatabaseSeeder(new QuestionBankPackageReader(), new QuestionBankMerger())
            .SeedAsync(context, cancellationToken);
        return factory;
    }
}
