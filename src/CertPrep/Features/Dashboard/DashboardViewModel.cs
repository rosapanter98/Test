using System.Collections.ObjectModel;
using CertPrep.Features.ExamCatalog;
using CertPrep.Features.Practice;
using CertPrep.Features.Progress;
using CertPrep.Features.Rewards;
using CertPrep.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CertPrep.Features.Dashboard;

public partial class DashboardViewModel(
    ExamCatalogRepository catalogRepository,
    ProgressRepository progressRepository,
    PracticeSessionService practiceService,
    MasteryService masteryService,
    RewardService rewardService,
    RankPresentationService rankPresentationService,
    Action<ExamSummary> openExam,
    Action<IReadOnlyList<ExamSummary>> openMixedPractice,
    Func<int, Task> startBoss,
    Func<int, Task> resumeSession,
    Func<int, Task> abandonSession) : ViewModelBase
{
    private readonly List<ExamSummary> _loadedExams = [];
    private LevelProgress? _levelProgress;

    public ObservableCollection<ExamCardViewModel> Exams { get; } = [];
    public ObservableCollection<ActiveSessionCardViewModel> ActiveSessions { get; } = [];
    public bool CanStartMixed => _loadedExams.Count >= 2;
    public bool HasActiveSessions => ActiveSessions.Count > 0;

    [ObservableProperty]
    private int completedSessions;

    [ObservableProperty]
    private int answeredQuestions;

    [ObservableProperty]
    private int accuracyPercent;

    [ObservableProperty]
    private string momentumTitle = "Ready when you are";

    [ObservableProperty]
    private string momentumMessage = "Complete a session to start building momentum.";

    [ObservableProperty]
    private int momentumProgressPercent;

    [ObservableProperty]
    private string momentumProgressText = "0 / 10 correct";

    [ObservableProperty]
    private string unlockedBadgesText = "No badges yet";

    [ObservableProperty]
    private string mixedPoolText = "Load at least two exams to mix them.";

    [ObservableProperty]
    private string levelTitle = "Level 1";

    [ObservableProperty]
    private string rankTitle = "Freshly Spawned Idiot";

    [ObservableProperty]
    private string rankDescription = "Has achieved absolutely nothing. Impressive.";

    [ObservableProperty]
    private bool roastModeEnabled = rankPresentationService.IsRoastModeEnabled;

    [ObservableProperty]
    private string levelMessage = "Complete a session to earn the first XP.";

    [ObservableProperty]
    private int levelProgressPercent;

    [ObservableProperty]
    private string levelProgressText = "0 / 200 XP";

    public string AccuracyText => AnsweredQuestions == 0 ? "—" : $"{AccuracyPercent}%";
    public string SessionsText => CompletedSessions.ToString();
    public string QuestionsText => AnsweredQuestions.ToString();

    public async Task LoadAsync()
    {
        await RunBusyAsync(async () =>
        {
            var exams = await catalogRepository.GetExamSummariesAsync();
            var overview = await progressRepository.GetOverviewAsync();
            var activeSessions = await practiceService.GetActiveSessionsAsync();
            var readiness = await masteryService.GetExamReadinessAsync();
            var rewardOverview = await rewardService.GetOverviewAsync();
            var readinessByExam = readiness.ToDictionary(item => item.ExamId);

            Exams.Clear();
            ActiveSessions.Clear();
            _loadedExams.Clear();
            foreach (var session in activeSessions)
            {
                ActiveSessions.Add(new ActiveSessionCardViewModel(
                    session,
                    resumeSession,
                    abandonSession));
            }

            foreach (var exam in exams)
            {
                _loadedExams.Add(exam);
                Exams.Add(new ExamCardViewModel(
                    exam,
                    readinessByExam[exam.Id],
                    openExam,
                    startBoss));
            }

            CompletedSessions = overview.CompletedSessions;
            AnsweredQuestions = overview.AnsweredQuestions;
            AccuracyPercent = overview.AccuracyPercent;
            MomentumTitle = overview.Momentum.Title;
            MomentumMessage = overview.Momentum.Message;
            MomentumProgressPercent = overview.Momentum.ProgressPercent;
            MomentumProgressText = overview.Momentum.ProgressText;
            UnlockedBadgesText = overview.Momentum.UnlockedBadges.Count == 0
                ? "No badges yet — finish a session to unlock the first."
                : $"Unlocked: {string.Join("  •  ", overview.Momentum.UnlockedBadges)}";
            MixedPoolText = CanStartMixed
                ? $"{_loadedExams.Count} exams • {_loadedExams.Sum(exam => exam.QuestionCount)} questions available"
                : "Load at least two exams to mix them.";
            LevelTitle = $"Level {rewardOverview.Level.Level}";
            _levelProgress = rewardOverview.Level;
            UpdateRank();
            LevelProgressPercent = rewardOverview.Level.ProgressPercent;
            LevelProgressText = $"{rewardOverview.Level.XpIntoLevel} / {rewardOverview.Level.XpForNextLevel} XP";
            var nearest = readiness
                .Select(exam => (Exam: exam, Objective: exam.NearestPromotion))
                .Where(item => item.Objective is not null)
                .OrderByDescending(item => item.Objective!.ReadinessPercent)
                .FirstOrDefault();
            LevelMessage = nearest.Objective is null
                ? "Every objective has reached Mastered. Keep readiness healthy."
                : $"Next mastery: {nearest.Exam.ExamCode} • {nearest.Objective.ObjectiveName}. {nearest.Objective.NextMilestone}";
            OnPropertyChanged(nameof(AccuracyText));
            OnPropertyChanged(nameof(SessionsText));
            OnPropertyChanged(nameof(QuestionsText));
            OnPropertyChanged(nameof(CanStartMixed));
            OnPropertyChanged(nameof(HasActiveSessions));
            OpenMixedCommand.NotifyCanExecuteChanged();
        });
    }

    partial void OnRoastModeEnabledChanged(bool value)
    {
        rankPresentationService.SetRoastMode(value);
        UpdateRank();
    }

    private void UpdateRank()
    {
        if (_levelProgress is null)
        {
            return;
        }

        var rank = rankPresentationService.GetRank(_levelProgress);
        RankTitle = rank.Title;
        RankDescription = rank.Description;
    }

    [RelayCommand(CanExecute = nameof(CanStartMixed))]
    private void OpenMixed() => openMixedPractice(_loadedExams);

}

public partial class ActiveSessionCardViewModel(
    ActiveSessionSummary session,
    Func<int, Task> resumeSession,
    Func<int, Task> abandonSession) : ViewModelBase
{
    public int SessionId => session.SessionId;
    public string Title => $"{session.ExamCode}  •  {ModeLabel(session.Mode)}";
    public string Progress => $"{session.AnsweredQuestions} of {session.TotalQuestions} answered  •  started {session.StartedAt.LocalDateTime:g}";

    [RelayCommand]
    private async Task ResumeAsync() => await resumeSession(session.SessionId);

    [RelayCommand]
    private async Task AbandonAsync() => await abandonSession(session.SessionId);

    private static string ModeLabel(PracticeMode mode) =>
        mode == PracticeMode.Study ? "Study mode" : "Exam simulation";
}

public partial class ExamCardViewModel : ViewModelBase
{
    private readonly ExamSummary _exam;
    private readonly Action<ExamSummary> _openExam;
    private readonly Func<int, Task> _startBoss;

    public ExamCardViewModel(
        ExamSummary exam,
        ExamReadiness readiness,
        Action<ExamSummary> openExam,
        Func<int, Task> startBoss)
    {
        _exam = exam;
        _openExam = openExam;
        _startBoss = startBoss;
        ReadinessPercent = readiness.ReadinessPercent;
        ReadinessText = $"{readiness.ReadinessPercent}% readiness • {readiness.Status}";
        BossUnlocked = readiness.BossUnlocked;
        BossStatusText = readiness.BossUnlocked
            ? "Boss Exam unlocked"
            : $"Boss locked • {readiness.Objectives.Count(objective => objective.Tier >= MasteryTier.Reliable)} / {readiness.Objectives.Count} objectives Reliable";
    }

    public string Code => _exam.Code;
    public string Title => _exam.Title;
    public string QuestionCountText => $"{_exam.QuestionCount} questions";
    public string ScoreText => _exam.BestScorePercent is null
        ? "No completed sessions yet"
        : $"Best {_exam.BestScorePercent}%  •  {_exam.CompletedSessions} completed";
    public string ActionText => _exam.CompletedSessions == 0 ? "Start practice" : "Practice again";
    public int ReadinessPercent { get; }
    public string ReadinessText { get; }
    public bool BossUnlocked { get; }
    public string BossStatusText { get; }

    [RelayCommand]
    private void Open() => _openExam(_exam);

    [RelayCommand(CanExecute = nameof(BossUnlocked))]
    private async Task StartBossAsync() => await _startBoss(_exam.Id);
}
