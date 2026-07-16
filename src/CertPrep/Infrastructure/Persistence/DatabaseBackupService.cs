using Microsoft.Data.Sqlite;

namespace CertPrep.Infrastructure.Persistence;

public sealed class DatabaseBackupService(string databasePath, string backupsDirectory)
{
    private const int RetainedBackupCount = 3;

    public string? CreateBeforeMigration()
    {
        if (!File.Exists(databasePath))
        {
            return null;
        }

        Directory.CreateDirectory(backupsDirectory);
        var backupPath = Path.Combine(
            backupsDirectory,
            $"certprep-before-migration-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmssfff}.db");

        using var source = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadOnly,
            Pooling = false
        }.ToString());
        using var destination = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = backupPath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Pooling = false
        }.ToString());
        source.Open();
        destination.Open();
        source.BackupDatabase(destination);

        foreach (var staleBackup in new DirectoryInfo(backupsDirectory)
                     .GetFiles("certprep-before-migration-*.db")
                     .OrderByDescending(file => file.CreationTimeUtc)
                     .Skip(RetainedBackupCount))
        {
            staleBackup.Delete();
        }

        return backupPath;
    }
}
