using CertPrep.Features.ExamCatalog;
using CertPrep.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CertPrep.Features.Practice;

public sealed class PracticeRepository(IDbContextFactory<StudyDbContext> contextFactory)
{
    public async Task<IReadOnlyList<Exam>> LoadExamsWithQuestionsAsync(
        IReadOnlyCollection<int> examIds,
        CancellationToken cancellationToken = default)
    {
        var ids = examIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return [];
        }

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Exams
            .AsNoTracking()
            .AsSplitQuery()
            .Where(exam => ids.Contains(exam.Id) && !exam.IsArchived)
            .Include(exam => exam.Questions.Where(question => question.IsActive))
                .ThenInclude(question => question.Objective)
            .Include(exam => exam.Questions.Where(question => question.IsActive))
                .ThenInclude(question => question.Choices)
            .OrderBy(exam => exam.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Question>> LoadQuestionsAsync(
        IReadOnlyCollection<int> questionIds,
        CancellationToken cancellationToken = default)
    {
        var ids = questionIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return [];
        }

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Questions
            .AsNoTracking()
            .AsSplitQuery()
            .Where(question => ids.Contains(question.Id) && question.IsActive && !question.Exam.IsArchived)
            .Include(question => question.Exam)
            .Include(question => question.Objective)
            .Include(question => question.Choices)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<int, QuestionPracticeHistory>> GetQuestionHistoryAsync(
        IReadOnlyCollection<int> examIds,
        CancellationToken cancellationToken = default)
    {
        var ids = examIds.Distinct().ToList();
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var attempts = await context.PracticeSessionItems
            .AsNoTracking()
            .Where(item => ids.Contains(item.SourceExamId) && item.SubmittedAt != null)
            .Select(item => new
            {
                item.SourceQuestionId,
                item.IsCorrect,
                item.SubmittedAt,
                item.Id
            })
            .ToListAsync(cancellationToken);

        return attempts
            .GroupBy(item => item.SourceQuestionId)
            .ToDictionary(
                group => group.Key,
                group =>
                {
                    var ordered = group.OrderBy(item => item.SubmittedAt).ThenBy(item => item.Id).ToList();
                    return new QuestionPracticeHistory(
                        group.Key,
                        ordered.Count,
                        ordered.Count(item => item.IsCorrect != true),
                        ordered[^1].IsCorrect != true);
                });
    }

    public async Task<PracticeSession> SaveNewSessionAsync(
        PracticeSession session,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        context.PracticeSessions.Add(session);
        await context.SaveChangesAsync(cancellationToken);
        return session;
    }

    public async Task<SubmissionCandidate?> GetSubmissionCandidateAsync(
        int sessionItemId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var item = await context.PracticeSessionItems
            .AsNoTracking()
            .Include(candidate => candidate.Choices)
            .SingleOrDefaultAsync(candidate => candidate.Id == sessionItemId, cancellationToken);

        return item is null
            ? null
            : new SubmissionCandidate(
                item.Id,
                item.Kind,
                item.SubmittedAt is not null,
                item.Explanation,
                item.SourceName,
                item.SourceUrl,
                item.Choices.Select(choice => choice.Id).ToHashSet(),
                item.Choices.Where(choice => choice.IsCorrect).Select(choice => choice.Id).ToHashSet());
    }

    public async Task SaveSubmissionAsync(
        int sessionItemId,
        IReadOnlySet<int> selectedChoiceIds,
        bool isCorrect,
        DateTimeOffset submittedAt,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var item = await context.PracticeSessionItems
            .Include(candidate => candidate.Choices)
            .Include(candidate => candidate.PracticeSession)
            .SingleAsync(candidate => candidate.Id == sessionItemId, cancellationToken);

        if (item.SubmittedAt is not null)
        {
            throw new InvalidOperationException("This question has already been submitted.");
        }

        foreach (var choice in item.Choices)
        {
            choice.IsSelected = selectedChoiceIds.Contains(choice.Id);
        }

        item.IsCorrect = isCorrect;
        item.SubmittedAt = submittedAt;
        item.PracticeSession.CurrentItemIndex = Math.Max(
            item.PracticeSession.CurrentItemIndex,
            item.OrderIndex + 1);

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveDraftSelectionAsync(
        int sessionItemId,
        IReadOnlySet<int> selectedChoiceIds,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var item = await context.PracticeSessionItems
            .Include(candidate => candidate.Choices)
            .Include(candidate => candidate.PracticeSession)
            .SingleAsync(candidate => candidate.Id == sessionItemId, cancellationToken);

        if (item.PracticeSession.Status != PracticeSessionStatus.InProgress || item.SubmittedAt is not null)
        {
            return;
        }

        foreach (var choice in item.Choices)
        {
            choice.IsSelected = selectedChoiceIds.Contains(choice.Id);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<PracticeSession?> GetActiveSessionAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var sessions = await context.PracticeSessions
            .AsNoTracking()
            .AsSplitQuery()
            .Where(session => session.Status == PracticeSessionStatus.InProgress)
            .Include(session => session.Items)
                .ThenInclude(item => item.Choices)
            .ToListAsync(cancellationToken);
        return sessions.OrderByDescending(session => session.StartedAt).FirstOrDefault();
    }

    public async Task<PracticeSession?> GetSessionAsync(
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.PracticeSessions
            .AsNoTracking()
            .AsSplitQuery()
            .Include(session => session.Items)
                .ThenInclude(item => item.Choices)
            .SingleOrDefaultAsync(session => session.Id == sessionId, cancellationToken);
    }

    public async Task MarkCompletedAsync(
        int sessionId,
        DateTimeOffset completedAt,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var session = await context.PracticeSessions
            .Include(candidate => candidate.Items)
            .SingleAsync(candidate => candidate.Id == sessionId, cancellationToken);

        if (session.Items.Any(item => item.SubmittedAt is null))
        {
            throw new InvalidOperationException("Complete every question before finishing the session.");
        }

        session.CompletedAt ??= completedAt;
        session.Status = PracticeSessionStatus.Completed;
        session.CurrentItemIndex = session.Items.Count;
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAbandonedAsync(
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var session = await context.PracticeSessions.SingleAsync(
            candidate => candidate.Id == sessionId,
            cancellationToken);

        if (session.Status == PracticeSessionStatus.InProgress)
        {
            session.Status = PracticeSessionStatus.Abandoned;
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
