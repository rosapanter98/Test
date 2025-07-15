using AvaloniaApplication3.Models;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace AvaloniaApplication3.Repositories
{
    public interface IQuizAttemptRepository
    {
        Task<List<QuizAttempt>> GetAttemptsByUserIdAsync(int userId);
        Task AddAttemptAsync(QuizAttempt attempt);
        Task<QuizAttempt?> GetAttemptByIdAsync(int id);
    }
}
