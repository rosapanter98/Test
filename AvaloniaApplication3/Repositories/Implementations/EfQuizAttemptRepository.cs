using AvaloniaApplication3.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace AvaloniaApplication3.Repositories
{
    public class EfQuizAttemptRepository : IQuizAttemptRepository
    {
        private readonly AppDbContext _context;

        public EfQuizAttemptRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<QuizAttempt>> GetAttemptsByUserIdAsync(int userId)
        {
            return await _context.QuizAttempts
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CompletedAt ?? a.StartedAt)
                .ToListAsync();
        }

        public async Task<QuizAttempt?> GetAttemptByIdAsync(int id)
        {
            return await _context.QuizAttempts
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task AddAttemptAsync(QuizAttempt attempt)
        {
            _context.QuizAttempts.Add(attempt);
            await _context.SaveChangesAsync();
        }
    }
}
