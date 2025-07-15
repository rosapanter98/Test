using AvaloniaApplication3.Models;
using AvaloniaApplication3.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvaloniaApplication3.Services
{
    public class QuizAttemptService : IQuizAttemptService
    {
        private readonly IQuizAttemptRepository _repository;

        public QuizAttemptService(IQuizAttemptRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<QuizAttempt>> GetAttemptsByUserAsync(int userId)
        {
            return await _repository.GetAttemptsByUserIdAsync(userId);
        }

        public async Task<QuizAttempt?> GetAttemptByIdAsync(int attemptId)
        {
            return await _repository.GetAttemptByIdAsync(attemptId);
        }

        public async Task<bool> RecordAttemptAsync(QuizAttempt attempt)
        {
            try
            {
                // Basic safety check
                if (attempt.TotalQuestions == 0 || attempt.CorrectAnswers < 0 || attempt.UserId <= 0)
                    return false;

                attempt.CompletedAt ??= DateTime.UtcNow;

                await _repository.AddAttemptAsync(attempt);
                return true;
            }
            catch
            {
                // Consider logging here
                return false;
            }
        }
    }
}
