using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CertPrep.Features.ExamCatalog;
using CertPrep.Features.ExamCatalog.Importing;
using CertPrep.Features.Practice;
using CertPrep.Features.Progress;
using CertPrep.Features.Rewards;
using CertPrep.Infrastructure.Persistence;
using CertPrep.Infrastructure.Settings;
using CertPrep.Shared;
using CertPrep.Shell;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CertPrep;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _serviceProvider = BuildServices();

            _serviceProvider.GetRequiredService<AppearanceService>().Initialize();
            _serviceProvider.GetRequiredService<RankPresentationService>().Initialize();

            _serviceProvider.GetRequiredService<DatabaseInitializer>()
                .InitializeAsync()
                .GetAwaiter()
                .GetResult();

            _serviceProvider.GetRequiredService<RewardService>()
                .ReconcileAsync()
                .GetAwaiter()
                .GetResult();

            var shell = _serviceProvider.GetRequiredService<ShellViewModel>();
            shell.InitializeAsync().GetAwaiter().GetResult();

            desktop.MainWindow = new MainWindow { DataContext = shell };
            desktop.Exit += OnDesktopExit;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static ServiceProvider BuildServices()
    {
        var services = new ServiceCollection();
        var connectionString = $"Data Source={AppPaths.GetDatabasePath()}";

        services.AddPooledDbContextFactory<StudyDbContext>(options => options.UseSqlite(connectionString));
        services.AddSingleton<QuestionBankPackageReader>();
        services.AddSingleton<QuestionBankMerger>();
        services.AddSingleton<QuestionBankImportService>();
        services.AddSingleton<DatabaseSeeder>();
        services.AddSingleton(new DatabaseBackupService(
            AppPaths.GetDatabasePath(),
            AppPaths.GetBackupsDirectory()));
        services.AddSingleton<DatabaseInitializer>();
        services.AddSingleton(new AppSettingsStore(AppPaths.GetSettingsPath()));
        services.AddSingleton<AppearanceService>();
        services.AddSingleton<ExamCatalogRepository>();
        services.AddSingleton<PracticeRepository>();
        services.AddSingleton<ProgressRepository>();
        services.AddSingleton<MasteryCalculator>();
        services.AddSingleton<MasteryService>();
        services.AddSingleton<LevelCalculator>();
        services.AddSingleton<RewardRepository>();
        services.AddSingleton<RewardService>();
        services.AddSingleton<RankPresentationService>();
        services.AddSingleton<TimeProvider>(TimeProvider.System);
        services.AddSingleton(Random.Shared);
        services.AddSingleton<PracticeSessionService>();
        services.AddSingleton<ShellViewModel>();

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
    }

    private void OnDesktopExit(object? sender, ControlledApplicationLifetimeExitEventArgs args)
    {
        _serviceProvider?.Dispose();
        _serviceProvider = null;
    }
}
