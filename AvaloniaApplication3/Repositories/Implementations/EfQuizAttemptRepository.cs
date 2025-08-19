using AvaloniaApplication3.Models;
using AvaloniaApplication3.Models.Enums;
using AvaloniaApplication3.Utility;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AvaloniaApplication3.Repositories
{
    public class EfQuizAttemptRepository : IQuizAttemptRepository
    {
        private readonly AppDbContext _context;
        public EfQuizAttemptRepository(AppDbContext context) => _context = context;

        public async Task<QuizAttempt> StartAttemptAsync(QuizAttempt attempt, CancellationToken ct = default)
        {
            _context.QuizAttempts.Add(attempt);
            await _context.SaveChangesAsync(ct);
            return attempt;
        }

        public async Task<QuizAttemptItem?> GetItemAsync(int attemptItemId, CancellationToken ct = default)
        {
            return await _context.QuizAttemptItems
                .Include(i => i.Answers)
                .FirstOrDefaultAsync(i => i.Id == attemptItemId, ct);
        }

        public async Task SubmitItemAsync(QuizAttemptItem item, IEnumerable<int> selectedAnswerIds, CancellationToken ct = default)
        {
            var selected = selectedAnswerIds.ToHashSet();
            foreach (var ans in item.Answers)
                ans.IsSelected = selected.Contains(ans.AnswerId);

            var sel = item.Answers.Where(a => a.IsSelected).Select(a => a.AnswerId).ToHashSet();
            var cor = item.Answers.Where(a => a.IsCorrect).Select(a => a.AnswerId).ToHashSet();
            item.IsCorrect = sel.SetEquals(cor);
            item.Submitted = true;

            await _context.SaveChangesAsync(ct);
        }

        public async Task<QuizAttempt> CompleteAttemptAsync(int attemptId, CancellationToken ct = default)
        {
            var attempt = await _context.QuizAttempts
                .Include(a => a.Items)
                .ThenInclude(i => i.Answers)
                .FirstAsync(a => a.Id == attemptId, ct);

            attempt.CorrectAnswers = attempt.Items.Count(i => i.Submitted && i.IsCorrect);
            attempt.CompletedAt = System.DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
            return attempt;
        }

        public async Task<QuizAttempt?> GetAttemptAsync(int attemptId, bool includeItems, CancellationToken ct = default)
        {
            Console.WriteLine($"[DB] Fetching at{attemptId}, includeItems={includeItems}");
            IQueryable<QuizAttempt> q = _context.QuizAttempts;
            if (includeItems)
                q = q.Include(a => a.Items).ThenInclude(i => i.Answers);

            return await q.FirstOrDefaultAsync(a => a.Id == attemptId, ct);
        }

        public async Task<List<QuizAttempt>> GetAttemptsByUserIdAsync(int userId, CancellationToken ct = default)
        {
            return await _context.QuizAttempts
                .Include(a => a.Quiz) // <-- add this
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CompletedAt ?? a.StartedAt)
                .ToListAsync();
        }
        public async Task DeleteAttemptAsync(int id, CancellationToken ct = default)
        {
            var entity = await _context.QuizAttempts.FindAsync(new object[] { id }, ct);
            if (entity is null) return;                // already gone / wrong id
            _context.QuizAttempts.Remove(entity);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<QuizAttempt?> GetInProgressAttemptAsync(int userId, int quizId)
    => await _context.QuizAttempts
        .Include(a => a.Items).ThenInclude(i => i.Answers)
        .FirstOrDefaultAsync(a => a.UserId == userId && a.QuizId == quizId && a.Status == AttemptStatus.InProgress);

        public async Task<QuizAttempt?> StartAttemptAsync(QuizAttempt attempt)
        {
            _context.QuizAttempts.Add(attempt);
            await _context.SaveChangesAsync();
            return attempt;
        }

        public Task SaveAsync() => _context.SaveChangesAsync();


    }
}
