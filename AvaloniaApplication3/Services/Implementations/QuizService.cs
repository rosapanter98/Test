using AvaloniaApplication3.Models;
using AvaloniaApplication3.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvaloniaApplication3.Services
{
    public class QuizService : IQuizService
    {
        private readonly IQuizRepository _quizRepository;

        public QuizService(IQuizRepository quizRepository)
        {
            _quizRepository = quizRepository;
        }

        public Task<List<Quiz>> GetAllQuizzesAsync()
        {
            return _quizRepository.GetAllQuizzesAsync();
        }

        public Task<Quiz?> GetFullQuizAsync(int id)
        {
            return _quizRepository.GetFullQuizByIdAsync(id);
        }

        public async Task<bool> CreateQuizAsync(Quiz quiz)
        {
            try
            {
                await _quizRepository.AddQuizAsync(quiz);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateQuizAsync(Quiz quiz)
        {
            try
            {
                await _quizRepository.UpdateQuizAsync(quiz);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteQuizAsync(int id)
        {
            try
            {
                await _quizRepository.DeleteQuizAsync(id);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
