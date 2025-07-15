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

        public Task<Quiz?> GetQuizByIdAsync(int id)
        {
            return _quizRepository.GetQuizByIdAsync(id);
        }

        public async Task<Quiz?> GetQuizWithQuestionsAsync(int id)
        {
            var quiz = await _quizRepository.GetQuizByIdAsync(id);
            if (quiz == null)
                return null;

            // Assuming the quiz has a navigation property `Questions`
            // and the context is tracking correctly, this might already be loaded
            // If not, you might need to use EF.Include in the repository layer
            return quiz;
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
