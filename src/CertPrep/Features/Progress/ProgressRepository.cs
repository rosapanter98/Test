using CertPrep.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CertPrep.Features.Progress;

public sealed class ProgressRepository(IDbContextFactory<StudyDbContext> contextFactory)
{
    public async Task<IReadOnlyList<ObjectiveMasteryInput>> GetMasteryInputsAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var exams = await context.Exams
            .AsNoTracking()
            .AsSplitQuery()
            .Where(exam => !exam.IsArchived)
            .Include(exam => exam.Objectives)
            .Include(exam => exam.Questions.Where(question => question.IsActive))
            .OrderBy(exam => exam.Code)
            .ToListAsync(cancellationToken);
        var items = await context.PracticeSessionItems
            .AsNoTracking()
            .Where(item => item.SubmittedAt != null)
            .Select(item => new
            {
                item.PracticeSessionId,
                item.SourceQuestionId,
                item.SourceExamId,
                item.ObjectiveContentKeySnapshot,
                item.SubmittedAt,
                item.IsCorrect
            })
            .ToListAsync(cancellationToken);

        return exams.SelectMany(exam => exam.Objectives
            .OrderBy(objective => objective.SortOrder)
            .Select(objective =>
            {
                var attempts = items
                    .Where(item => item.SourceExamId == exam.Id &&
                                   string.Equals(
                                       item.ObjectiveContentKeySnapshot,
                                       objective.ContentKey,
                                       StringComparison.OrdinalIgnoreCase))
                    .Select(item => new ObjectiveAttempt(
                        item.PracticeSessionId,
                        item.SourceQuestionId,
                        item.SubmittedAt!.Value,
                        item.IsCorrect == true))
                    .ToList();
                return new ObjectiveMasteryInput(
                    exam.Id,
                    exam.Code,
                    exam.Title,
                    objective.ContentKey,
                    objective.Name,
                    exam.Questions.Count(question => question.ExamObjectiveId == objective.Id),
                    attempts);
            }))
            .ToList();
    }

    public async Task<StudyOverview> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var examCount = await context.Exams.CountAsync(exam => !exam.IsArchived, cancellationToken);
        var sessions = await context.PracticeSessions
            .AsNoTracking()
            .Include(session => session.Items)
            .ToListAsync(cancellationToken);
        var completedSessions = sessions.Where(session => session.CompletedAt != null).ToList();
        var items = sessions.SelectMany(session => session.Items).Where(item => item.SubmittedAt != null).ToList();
        var correctAnswers = items.Count(item => item.IsCorrect == true);

        return new StudyOverview(
            examCount,
            completedSessions.Count,
            items.Count,
            CalculateAccuracy(correctAnswers, items.Count),
            BuildMomentum(correctAnswers, completedSessions));
    }

    public async Task<IReadOnlyList<ExamProgress>> GetExamProgressAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var exams = await context.Exams
            .AsNoTracking()
            .Where(exam => !exam.IsArchived)
            .Include(exam => exam.Objectives)
            .OrderBy(exam => exam.Code)
            .ToListAsync(cancellationToken);

        var sessions = await context.PracticeSessions
            .AsNoTracking()
            .Include(session => session.Items)
            .ToListAsync(cancellationToken);

        return exams.Select(exam =>
        {
            var examSessions = sessions
                .Where(session => session.Items.Any(item => item.SourceExamId == exam.Id))
                .ToList();
            var completed = examSessions.Where(session => session.CompletedAt != null).ToList();
            var answered = examSessions
                .SelectMany(session => session.Items)
                .Where(item => item.SourceExamId == exam.Id && item.SubmittedAt != null)
                .ToList();
            var scores = completed.Select(session => CalculateAccuracy(
                session.Items.Count(item => item.SourceExamId == exam.Id && item.IsCorrect == true),
                session.Items.Count(item => item.SourceExamId == exam.Id))).ToList();

            var objectiveProgress = exam.Objectives
                .OrderBy(objective => objective.SortOrder)
                .Select(objective =>
                {
                    var objectiveItems = answered.Where(item => string.Equals(
                        item.ObjectiveContentKeySnapshot,
                        objective.ContentKey,
                        StringComparison.OrdinalIgnoreCase)).ToList();
                    var correct = objectiveItems.Count(item => item.IsCorrect == true);
                    return new ObjectiveProgress(
                        objective.ContentKey,
                        objective.Name,
                        objectiveItems.Count,
                        correct,
                        CalculateAccuracy(correct, objectiveItems.Count));
                })
                .ToList();

            return new ExamProgress(
                exam.Id,
                exam.Code,
                exam.Title,
                completed.Count,
                answered.Count,
                CalculateAccuracy(answered.Count(item => item.IsCorrect == true), answered.Count),
                scores.Count == 0 ? null : scores.Max(),
                objectiveProgress);
        }).ToList();
    }

    private static int CalculateAccuracy(int correct, int total) =>
        total == 0
            ? 0
            : (int)Math.Round(correct * 100d / total, MidpointRounding.AwayFromZero);

    private static StudyMomentum BuildMomentum(
        int correctAnswers,
        IReadOnlyCollection<Practice.PracticeSession> completedSessions)
    {
        var badges = new List<string>();
        if (completedSessions.Count > 0)
        {
            badges.Add("First run");
        }

        if (completedSessions.Any(session => session.Scope == Practice.PracticeSessionScope.MixedExam))
        {
            badges.Add("Cross-exam");
        }

        if (completedSessions.Any(session => session.Items.Count > 0 && session.Items.All(item => item.IsCorrect == true)))
        {
            badges.Add("Clean sweep");
        }

        if (correctAnswers >= 25)
        {
            badges.Add("25 correct");
        }

        if (correctAnswers >= 100)
        {
            badges.Add("Century");
        }

        var fixedTargets = new[] { 10, 25, 50, 100 };
        var target = fixedTargets.FirstOrDefault(candidate => candidate > correctAnswers);
        if (target == 0)
        {
            target = ((correctAnswers / 100) + 1) * 100;
        }

        var title = correctAnswers switch
        {
            0 => "Ready when you are",
            < 10 => "Warming up",
            < 25 => "Building momentum",
            < 50 => "Locked in",
            _ => "Knowledge stack growing"
        };

        return new StudyMomentum(
            title,
            $"Next milestone: {target} correct answers.",
            CalculateAccuracy(correctAnswers, target),
            $"{correctAnswers} / {target} correct",
            badges);
    }
}
