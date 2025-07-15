using AvaloniaApplication3.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvaloniaApplication3.Services
{
    public interface IQuizAttemptService
    {
        Task<List<QuizAttempt>> GetAttemptsByUserAsync(int userId);
        Task<QuizAttempt?> GetAttemptByIdAsync(int attemptId);
        Task<bool> RecordAttemptAsync(QuizAttempt attempt);
    }
}
