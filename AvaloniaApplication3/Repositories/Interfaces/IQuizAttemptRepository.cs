using AvaloniaApplication3.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace AvaloniaApplication3.Repositories
{
    public interface IQuizAttemptRepository
    {
        Task<QuizAttempt> StartAttemptAsync(QuizAttempt attempt, CancellationToken ct = default);
        Task<QuizAttemptItem?> GetItemAsync(int attemptItemId, CancellationToken ct = default);
        Task SubmitItemAsync(QuizAttemptItem item, IEnumerable<int> selectedAnswerIds, CancellationToken ct = default);
        Task<QuizAttempt> CompleteAttemptAsync(int attemptId, CancellationToken ct = default);

        Task<QuizAttempt?> GetAttemptAsync(int attemptId, bool includeItems, CancellationToken ct = default);
        Task<List<QuizAttempt>> GetAttemptsByUserIdAsync(int userId, CancellationToken ct = default);
        Task DeleteAttemptAsync(int id, CancellationToken ct = default);

        Task<QuizAttempt?> GetInProgressAttemptAsync(int userId, int quizId);
        Task SaveAsync();
    }
}
