using CertPrep.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Xunit;

namespace CertPrep.Tests.Infrastructure;

public sealed class DatabaseBackupServiceTests
{
    [Fact]
    public async Task Backup_is_a_readable_sqlite_copy()
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
