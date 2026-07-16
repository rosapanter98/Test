using CertPrep.Features.Practice;
using CertPrep.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CertPrep.Features.Rewards;

public sealed class RewardRepository(IDbContextFactory<StudyDbContext> contextFactory)
{
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

    public async Task<IReadOnlyList<PracticeSessionItem>> GetSubmittedItemsAsync(
        IReadOnlyCollection<int> examIds,
        CancellationToken cancellationToken = default)
    {
        var ids = examIds.Distinct().ToList();
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var items = await context.PracticeSessionItems
            .AsNoTracking()
            .Where(item => ids.Contains(item.SourceExamId) && item.SubmittedAt != null)
            .ToListAsync(cancellationToken);
        return items.OrderBy(item => item.SubmittedAt).ThenBy(item => item.Id).ToList();
    }

    public async Task AddMissingEntriesAsync(
        IReadOnlyCollection<RewardLedgerEntry> entries,
        CancellationToken cancellationToken = default)
    {
        if (entries.Count == 0)
        {
            return;
        }

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var keys = entries.Select(entry => entry.IdempotencyKey).ToList();
        var existingKeys = await context.RewardLedgerEntries
            .Where(entry => keys.Contains(entry.IdempotencyKey))
            .Select(entry => entry.IdempotencyKey)
            .ToHashSetAsync(cancellationToken);
        context.RewardLedgerEntries.AddRange(entries.Where(entry => !existingKeys.Contains(entry.IdempotencyKey)));
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RewardLedgerEntry>> GetSessionEntriesAsync(
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.RewardLedgerEntries
            .AsNoTracking()
            .Where(entry => entry.PracticeSessionId == sessionId)
            .OrderBy(entry => entry.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalXpAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.RewardLedgerEntries.SumAsync(entry => (int?)entry.Amount, cancellationToken) ?? 0;
    }

    public async Task<IReadOnlyList<int>> GetUnrewardedCompletedSessionIdsAsync(
        int ruleVersion,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var sessions = await context.PracticeSessions
            .AsNoTracking()
            .Where(session => session.Status == PracticeSessionStatus.Completed &&
                              !context.RewardLedgerEntries.Any(entry =>
                                  entry.PracticeSessionId == session.Id &&
                                  entry.Kind == RewardKind.SessionCompleted &&
                                  entry.RuleVersion == ruleVersion))
            .ToListAsync(cancellationToken);
        return sessions.OrderBy(session => session.CompletedAt).Select(session => session.Id).ToList();
    }

    public async Task<int> GetRewardedSessionCountAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.RewardLedgerEntries
            .Where(entry => entry.Kind == RewardKind.SessionCompleted)
            .Select(entry => entry.PracticeSessionId)
            .Distinct()
            .CountAsync(cancellationToken);
    }
}
