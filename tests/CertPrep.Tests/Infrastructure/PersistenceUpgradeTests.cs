using CertPrep.Features.Practice;
using CertPrep.Features.Rewards;
using CertPrep.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace CertPrep.Tests.Infrastructure;

public sealed class PersistenceUpgradeTests
{
    [Fact]
    public async Task Existing_session_is_backfilled_and_survives_the_rewards_migration()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(cancellationToken);
        var options = new DbContextOptionsBuilder<StudyDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var context = new StudyDbContext(options);
        var migrator = context.GetService<IMigrator>();
        await migrator.MigrateAsync("20260716070958_StableCatalogContentKeys", cancellationToken);

        await context.Database.ExecuteSqlRawAsync("""
            INSERT INTO Exams (Id, Provider, Code, Title, Summary, ContentVersion, IsArchived)
            VALUES (1, 'Cisco', 'CCNA', 'Cisco Certified Network Associate', 'Networking', '1.0', 0);
            INSERT INTO ExamObjectives (Id, ExamId, ContentKey, Name, SortOrder)
            VALUES (1, 1, 'routing', 'Routing fundamentals', 0);
            INSERT INTO Questions (Id, ExamId, ExamObjectiveId, ContentKey, Prompt, Kind, Difficulty, Explanation, SourceName, SourceUrl, IsActive)
            VALUES (1, 1, 1, 'route-selection', 'How is a route selected?', 'SingleChoice', 'Foundation', 'Longest prefix.', 'Cisco', 'https://example.test/routing', 1);
            INSERT INTO PracticeSessions (Id, ExamId, ExamCodeSnapshot, ExamTitleSnapshot, Scope, Mode, Status, StartedAt, CompletedAt, CurrentItemIndex)
            VALUES (1, 1, 'CCNA', 'Cisco Certified Network Associate', 'SingleExam', 'Study', 'InProgress', '2026-07-01 08:00:00+00:00', NULL, 0);
            INSERT INTO PracticeSessionItems (Id, PracticeSessionId, SourceQuestionId, SourceExamId, ExamCodeSnapshot, ExamTitleSnapshot, OrderIndex, ObjectiveName, Prompt, Kind, Difficulty, Explanation, SourceName, SourceUrl, SubmittedAt, IsCorrect)
            VALUES (1, 1, 1, 1, 'CCNA', 'Cisco Certified Network Associate', 0, 'Routing fundamentals', 'How is a route selected?', 'SingleChoice', 'Foundation', 'Longest prefix.', 'Cisco', 'https://example.test/routing', NULL, NULL);
            """, cancellationToken);

        await migrator.MigrateAsync(cancellationToken: cancellationToken);
        context.ChangeTracker.Clear();
        var session = await context.PracticeSessions.SingleAsync(cancellationToken);
        var item = await context.PracticeSessionItems.SingleAsync(cancellationToken);

        Assert.Equal(PracticeSessionPurpose.Standard, session.Purpose);
        Assert.Equal("routing", item.ObjectiveContentKeySnapshot);
        Assert.Equal(PracticeSessionStatus.InProgress, session.Status);
        Assert.Equal(0, await context.RewardLedgerEntries.CountAsync(cancellationToken));
    }

    [Fact]
    public async Task Pre_migration_backup_is_a_readable_sqlite_copy()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var directory = Path.Combine(Path.GetTempPath(), "CertPrep.Tests", Guid.NewGuid().ToString("N"));
        var databasePath = Path.Combine(directory, "certprep.db");
        var backupDirectory = Path.Combine(directory, "backups");
        Directory.CreateDirectory(directory);

        try
        {
            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = databasePath,
                Pooling = false
            }.ToString();
            await using (var connection = new SqliteConnection(connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                await using var command = connection.CreateCommand();
                command.CommandText = "CREATE TABLE Marker (Value TEXT NOT NULL); INSERT INTO Marker VALUES ('preserved');";
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            var backupPath = new DatabaseBackupService(databasePath, backupDirectory).CreateBeforeMigration();
            Assert.NotNull(backupPath);
            Assert.True(File.Exists(backupPath));

            await using var backup = new SqliteConnection(new SqliteConnectionStringBuilder
            {
                DataSource = backupPath,
                Mode = SqliteOpenMode.ReadOnly,
                Pooling = false
            }.ToString());
            await backup.OpenAsync(cancellationToken);
            await using var verify = backup.CreateCommand();
            verify.CommandText = "SELECT Value FROM Marker;";
            Assert.Equal("preserved", await verify.ExecuteScalarAsync(cancellationToken));
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }
    }
}
