using CertPrep.Features.Dashboard;
using CertPrep.Features.ExamCatalog;
using CertPrep.Features.ExamCatalog.Importing;
using CertPrep.Features.Practice;
using CertPrep.Features.Progress;
using CertPrep.Features.Results;
using CertPrep.Features.Rewards;
using CertPrep.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CertPrep.Shell;

public partial class ShellViewModel(
    ExamCatalogRepository catalogRepository,
    ProgressRepository progressRepository,
    PracticeSessionService practiceService,
    MasteryService masteryService,
    RewardService rewardService,
    RankPresentationService rankPresentationService,
    AppearanceService appearanceService,
    QuestionBankImportService questionBankImportService) : ViewModelBase
{
    public IReadOnlyList<AppTheme> ThemeOptions { get; } = Enum.GetValues<AppTheme>();

    [ObservableProperty]
    private ViewModelBase? currentPage;

    [ObservableProperty]
    private string sectionLabel = "STUDY OVERVIEW";

    [ObservableProperty]
    private string pageTitle = "Dashboard";

    [ObservableProperty]
    private string pageSubtitle = "Choose an exam and keep the feedback loop short.";

    [ObservableProperty]
    private bool isNavigationEnabled = true;

    [ObservableProperty]
    private AppTheme selectedTheme = appearanceService.CurrentTheme;

    [ObservableProperty]
    private string? importStatus;

    partial void OnSelectedThemeChanged(AppTheme value) => appearanceService.SetTheme(value);

    public Task InitializeAsync() => ShowDashboardAsync();

    public Task FlushActiveSessionAsync() =>
        CurrentPage is PracticeViewModel practice
            ? practice.FlushDraftAsync()
            : Task.CompletedTask;

    public async Task ImportQuestionBankAsync(string path)
    {
        if (!IsNavigationEnabled)
        {
            return;
        }

        try
        {
            ImportStatus = "Validating question bank…";
            var result = await questionBankImportService.ImportSqliteAsync(path);
            ImportStatus = result.Summary;
            await ShowDashboardAsync();
        }
        catch (Exception exception) when (exception is
            QuestionBankValidationException or
            FileNotFoundException or
            IOException or
            Microsoft.Data.Sqlite.SqliteException)
        {
            ImportStatus = $"Import failed: {exception.Message}";
        }
    }

    [RelayCommand]
    private async Task ShowDashboardAsync()
    {
        if (!IsNavigationEnabled)
        {
            return;
        }

        var dashboard = new DashboardViewModel(
            catalogRepository,
            progressRepository,
            practiceService,
            masteryService,
            rewardService,
            rankPresentationService,
            OpenExam,
            OpenMixedPractice,
            StartBossAsync,
            ResumeSessionAsync,
            AbandonSessionAsync);
        SetPage(dashboard, "STUDY OVERVIEW", "Dashboard", "Choose an exam and keep the feedback loop short.");
        await dashboard.LoadAsync();
    }

    [RelayCommand]
    private async Task ShowProgressAsync()
    {
        if (!IsNavigationEnabled)
        {
            return;
        }

        var progress = new ProgressViewModel(progressRepository, masteryService);
        SetPage(progress, "PERFORMANCE", "Progress", "Accuracy by exam and skills-measured area.");
        await progress.LoadAsync();
    }

    private void OpenExam(ExamSummary exam)
    {
        var setup = new PracticeSetupViewModel(exam, StartPracticeAsync, ShowDashboardAsync);
        SetPage(setup, "PRACTICE SETUP", exam.Code, exam.Title);
    }

    private void OpenMixedPractice(IReadOnlyList<ExamSummary> exams)
    {
        var setup = new MixedPracticeSetupViewModel(exams, StartMixedPracticeAsync, ShowDashboardAsync);
        SetPage(setup, "MIXED PRACTICE", "Mix exams", "Build one balanced, randomized question pool.");
    }

    private async Task StartPracticeAsync(int examId, PracticeMode mode, int questionCount)
    {
        var run = await practiceService.StartAsync(examId, mode, questionCount);
        ShowPractice(run);
    }

    private async Task StartMixedPracticeAsync(
        IReadOnlyList<int> examIds,
        PracticeMode mode,
        int questionCount)
    {
        var run = await practiceService.StartAsync(examIds, mode, questionCount);
        ShowPractice(run);
    }

    private async Task StartBossAsync(int examId)
    {
        var run = await practiceService.StartBossAsync(examId);
        ShowPractice(run);
    }

    private void ShowPractice(PracticeRun run)
    {
        IsNavigationEnabled = false;
        var practice = new PracticeViewModel(run, practiceService, ShowResultsAsync, ExitPracticeAsync);
        SetPage(practice, "ACTIVE SESSION", run.ExamCode, run.ExamTitle);
    }

    private async Task ShowResultsAsync(PracticeResult result)
    {
        IsNavigationEnabled = true;
        var readiness = await masteryService.GetExamReadinessAsync();
        var results = new ResultsViewModel(
            result,
            readiness,
            rankPresentationService,
            ShowDashboardAsync,
            ShowProgressAsync,
            RetryMissedAsync);
        SetPage(results, "SESSION COMPLETE", result.ExamCode, result.ExamTitle);
    }

    private async Task RetryMissedAsync(IReadOnlyList<int> sourceQuestionIds)
    {
        var run = await practiceService.StartReviewAsync(sourceQuestionIds);
        ShowPractice(run);
    }

    private async Task ExitPracticeAsync(int sessionId)
    {
        IsNavigationEnabled = true;
        await ShowDashboardAsync();
    }

    private async Task ResumeSessionAsync(int sessionId)
    {
        var resume = await practiceService.ResumeAsync(sessionId);
        if (resume.CompletedResult is not null)
        {
            await ShowResultsAsync(resume.CompletedResult);
            return;
        }

        ShowPractice(resume.Run!);
    }

    private async Task AbandonSessionAsync(int sessionId)
    {
        await practiceService.AbandonAsync(sessionId);
        await ShowDashboardAsync();
    }

    private void SetPage(ViewModelBase page, string section, string title, string subtitle)
    {
        CurrentPage = page;
        SectionLabel = section;
        PageTitle = title;
        PageSubtitle = subtitle;
    }
}
