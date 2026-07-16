using System.Collections.ObjectModel;
using CertPrep.Shared;

namespace CertPrep.Features.Progress;

public partial class ProgressViewModel(
    ProgressRepository repository,
    MasteryService masteryService) : ViewModelBase
{
    public ObservableCollection<ExamProgressViewModel> Exams { get; } = [];

    public async Task LoadAsync()
    {
        await RunBusyAsync(async () =>
        {
            var progress = await repository.GetExamProgressAsync();
            var readiness = (await masteryService.GetExamReadinessAsync())
                .ToDictionary(exam => exam.ExamId);
            Exams.Clear();
            foreach (var exam in progress)
            {
                Exams.Add(new ExamProgressViewModel(exam, readiness[exam.ExamId]));
            }
        });
    }
}

public sealed class ExamProgressViewModel(ExamProgress progress, ExamReadiness readiness)
{
    public string ExamCode => progress.ExamCode;
    public string ExamTitle => progress.ExamTitle;
    public string AccuracyText => progress.AnsweredQuestions == 0 ? "—" : $"{progress.AccuracyPercent}%";
    public string BestScoreText => progress.BestScorePercent is null ? "No completed session" : $"Best {progress.BestScorePercent}%";
    public string ReadinessText => $"{readiness.ReadinessPercent}%";
    public string ReadinessStatus => readiness.Status;
    public string ActivityText => $"{progress.CompletedSessions} sessions  •  {progress.AnsweredQuestions} answers";
    public IReadOnlyList<ObjectiveProgressViewModel> Objectives { get; } =
        progress.Objectives.Select(objective => new ObjectiveProgressViewModel(
            objective,
            readiness.Objectives.Single(mastery => mastery.ObjectiveContentKey == objective.ContentKey))).ToList();
}

public sealed class ObjectiveProgressViewModel(ObjectiveProgress progress, ObjectiveMastery mastery)
{
    public string Name => progress.Name;
    public int AccuracyPercent => progress.AccuracyPercent;
    public int ReadinessPercent => mastery.ReadinessPercent;
    public string AccuracyText => progress.AnsweredQuestions == 0 ? "Not practiced" : $"{progress.AccuracyPercent}%";
    public string DetailText => progress.AnsweredQuestions == 0
        ? "No answers yet"
        : $"{progress.CorrectAnswers} of {progress.AnsweredQuestions} correct  •  {mastery.AccuracyPercent}% accuracy";
    public string MasteryText => $"{mastery.Tier}  •  {mastery.ReadinessPercent}% readiness{(mastery.NeedsReview ? "  •  Needs review" : string.Empty)}";
}
