using CertPrep.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CertPrep.Features.ExamCatalog;

public sealed class ExamCatalogRepository(IDbContextFactory<StudyDbContext> contextFactory)
{
    public async Task<IReadOnlyList<ExamSummary>> GetExamSummariesAsync(
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

        var sessions = await context.PracticeSessions
            .AsNoTracking()
            .Where(session => session.CompletedAt != null)
            .Include(session => session.Items)
            .ToListAsync(cancellationToken);
        sessions = sessions.OrderByDescending(session => session.CompletedAt).ToList();

        return exams.Select(exam =>
        {
            var examSessions = sessions
                .Where(session => session.Items.Any(item => item.SourceExamId == exam.Id))
                .ToList();
            var scores = examSessions.Select(session => ScorePercent(session, exam.Id)).ToList();

            return new ExamSummary(
                exam.Id,
                exam.Provider,
                exam.Code,
                exam.Title,
                exam.Summary,
                exam.ContentVersion,
                exam.Questions.Count,
                exam.Objectives.OrderBy(objective => objective.SortOrder).Select(objective => objective.Name).ToList(),
                examSessions.Count,
                scores.Count == 0 ? null : scores.Max(),
                scores.Count == 0 ? null : scores[0]);
        }).ToList();
    }

    private static int ScorePercent(Practice.PracticeSession session, int examId)
    {
        var examItems = session.Items.Where(item => item.SourceExamId == examId).ToList();
        if (examItems.Count == 0)
        {
            return 0;
        }

        return (int)Math.Round(
            examItems.Count(item => item.IsCorrect == true) * 100d / examItems.Count,
            MidpointRounding.AwayFromZero);
    }
}
