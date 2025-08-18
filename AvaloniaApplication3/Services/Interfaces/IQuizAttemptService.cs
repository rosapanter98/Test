using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AvaloniaApplication3.Models;

namespace AvaloniaApplication3.Services
{
    public interface IQuizAttemptService
    {
        Task<QuizAttempt> StartAttemptAsync(User user, Quiz quiz, IReadOnlyList<Question> orderedQuestions, CancellationToken ct = default);
        Task SubmitItemAsync(int attemptItemId, IReadOnlyCollection<int> selectedAnswerIds, CancellationToken ct = default);
        Task<QuizAttempt> CompleteAttemptAsync(int attemptId, CancellationToken ct = default);

        Task<QuizAttempt?> GetAttemptAsync(int attemptId, bool includeItems = true, CancellationToken ct = default);
        Task<List<QuizAttempt>> GetAttemptsByUserAsync(int userId, CancellationToken ct = default);
        Task DeleteAttemptAsync(int id, CancellationToken ct = default);
    }
}
