using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AvaloniaApplication3.Models;
using AvaloniaApplication3.Repositories;

namespace AvaloniaApplication3.Services
{
    public class QuizAttemptService : IQuizAttemptService
    {
        private readonly IQuizAttemptRepository _repo;
        public QuizAttemptService(IQuizAttemptRepository repo) => _repo = repo;

        public async Task<QuizAttempt> StartAttemptAsync(User user, Quiz quiz, IReadOnlyList<Question> orderedQuestions, CancellationToken ct = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (quiz == null) throw new ArgumentNullException(nameof(quiz));
            if (orderedQuestions == null || orderedQuestions.Count == 0) throw new ArgumentException("No questions.", nameof(orderedQuestions));

            var attempt = new QuizAttempt
            {
                QuizId = quiz.Id,
                UserId = user.Id,
                StartedAt = DateTime.UtcNow,
                TotalQuestions = orderedQuestions.Count,
                Items = orderedQuestions.Select((q, idx) => new QuizAttemptItem
                {
                    QuestionId = q.Id,
                    QuestionText = q.Text,
                    QuestionType = q.Type,
                    OrderIndex = idx,
                    Answers = q.Answers
                        .OrderBy(a => a.Id)
                        .Select(a => new QuizAttemptItemAnswer
                        {
                            AnswerId = a.Id,
                            Text = a.Text,
                            IsCorrect = a.IsCorrect,
                            IsSelected = false
                        })
                        .ToList()
                }).ToList()
            };

            return await _repo.StartAttemptAsync(attempt, ct);
        }

        public async Task SubmitItemAsync(int attemptItemId, IReadOnlyCollection<int> selectedAnswerIds, CancellationToken ct = default)
        {
            var item = await _repo.GetItemAsync(attemptItemId, ct);
            if (item == null) return;
            await _repo.SubmitItemAsync(item, selectedAnswerIds, ct);
        }

        public Task<QuizAttempt> CompleteAttemptAsync(int attemptId, CancellationToken ct = default)
            => _repo.CompleteAttemptAsync(attemptId, ct);

        public Task<QuizAttempt?> GetAttemptAsync(int attemptId, bool includeItems = true, CancellationToken ct = default)
            => _repo.GetAttemptAsync(attemptId, includeItems, ct);

        public Task<List<QuizAttempt>> GetAttemptsByUserAsync(int userId, CancellationToken ct = default)
            => _repo.GetAttemptsByUserIdAsync(userId, ct);
    }
}
