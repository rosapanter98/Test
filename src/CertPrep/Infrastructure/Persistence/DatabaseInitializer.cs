using Microsoft.EntityFrameworkCore;

namespace CertPrep.Infrastructure.Persistence;

public sealed class DatabaseInitializer(
    IDbContextFactory<StudyDbContext> contextFactory,
    DatabaseSeeder seeder,
    DatabaseBackupService backupService)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var hasPendingMigrations = false;
        await using (var inspectionContext = await contextFactory.CreateDbContextAsync(cancellationToken))
        {
            hasPendingMigrations = (await inspectionContext.Database
                .GetPendingMigrationsAsync(cancellationToken)).Any();
        }

        if (hasPendingMigrations)
        {
            backupService.CreateBeforeMigration();
        }

        await using var migrationContext = await contextFactory.CreateDbContextAsync(cancellationToken);
        await migrationContext.Database.MigrateAsync(cancellationToken);
        await seeder.SeedAsync(migrationContext, cancellationToken);
    }
}
