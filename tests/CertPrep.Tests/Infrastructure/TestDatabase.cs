using CertPrep.Features.ExamCatalog;
using CertPrep.Features.ExamCatalog.Importing;
using CertPrep.Features.Practice;
using CertPrep.Features.Progress;
using CertPrep.Features.Rewards;
using CertPrep.Infrastructure.Persistence;
using CertPrep.Infrastructure.Settings;
using CertPrep.Shared;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CertPrep.Tests.Infrastructure;

public sealed class TestDatabase : IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    private readonly string _settingsDirectory;

    private TestDatabase(
        SqliteConnection connection,
        TestDbContextFactory contextFactory,
        string settingsDirectory)
    {
        _connection = connection;
        _settingsDirectory = settingsDirectory;
        ContextFactory = contextFactory;
    }

    public TestDbContextFactory ContextFactory { get; }

    public static async Task<TestDatabase> CreateAsync(
        bool seed = true,
        CancellationToken cancellationToken = default)
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(cancellationToken);

        var options = new DbContextOptionsBuilder<StudyDbContext>()
            .UseSqlite(connection)
            .Options;
        var factory = new TestDbContextFactory(options);
        var settingsDirectory = Path.Combine(
            Path.GetTempPath(),
            "CertPrep.Tests",
            Guid.NewGuid().ToString("N"));
        var database = new TestDatabase(connection, factory, settingsDirectory);

        await using var context = factory.CreateDbContext();
        await context.Database.MigrateAsync(cancellationToken);
        if (seed)
        {
            await new DatabaseSeeder(new QuestionBankPackageReader(), new QuestionBankMerger())
                .SeedAsync(context, cancellationToken);
        }

        return database;
    }

    public TestServices CreateServices(int randomSeed = 42)
    {
        var catalog = new ExamCatalogRepository(ContextFactory);
        var practiceRepository = new PracticeRepository(ContextFactory);
        var progress = new ProgressRepository(ContextFactory);
        var masteryCalculator = new MasteryCalculator();
        var mastery = new MasteryService(progress, masteryCalculator);
        var rewardRepository = new RewardRepository(ContextFactory);
        var rewards = new RewardService(
            rewardRepository,
            progress,
            masteryCalculator,
            new LevelCalculator(),
            TimeProvider.System);
        var practice = new PracticeSessionService(
            practiceRepository,
            rewards,
            mastery,
            TimeProvider.System,
            new Random(randomSeed));
        var settings = new AppSettingsStore(Path.Combine(_settingsDirectory, "settings.json"));
        var appearance = new AppearanceService(settings);
        appearance.Initialize();
        var ranks = new RankPresentationService(settings);
        ranks.Initialize();
        var importer = new QuestionBankImportService(
            ContextFactory,
            new QuestionBankPackageReader(),
            new QuestionBankMerger());

        return new TestServices(
            catalog,
            practiceRepository,
            progress,
            mastery,
            rewards,
            practice,
            ranks,
            appearance,
            importer);
    }

    public async Task<HashSet<int>> GetCorrectChoiceIdsAsync(
        IEnumerable<int> choiceIds,
        CancellationToken cancellationToken = default)
    {
        var ids = choiceIds.ToHashSet();
        await using var context = ContextFactory.CreateDbContext();
        return await context.PracticeSessionChoices
            .Where(choice => ids.Contains(choice.Id) && choice.IsCorrect)
            .Select(choice => choice.Id)
            .ToHashSetAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
        if (Directory.Exists(_settingsDirectory))
        {
            Directory.Delete(_settingsDirectory, true);
        }
    }
}

public sealed class TestDbContextFactory(DbContextOptions<StudyDbContext> options)
    : IDbContextFactory<StudyDbContext>
{
    public StudyDbContext CreateDbContext() => new(options);

    public ValueTask<StudyDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
        ValueTask.FromResult(CreateDbContext());
}

public sealed record TestServices(
    ExamCatalogRepository Catalog,
    PracticeRepository PracticeRepository,
    ProgressRepository Progress,
    MasteryService Mastery,
    RewardService Rewards,
    PracticeSessionService Practice,
    RankPresentationService Ranks,
    AppearanceService Appearance,
    QuestionBankImportService Importer);
