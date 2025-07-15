using AvaloniaApplication3.Models;
using AvaloniaApplication3.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvaloniaApplication3.Services
{
    public class QuestionService : IQuestionService
    {
        private readonly IQuestionRepository _questionRepository;

        public QuestionService(IQuestionRepository questionRepository)
        {
            _questionRepository = questionRepository;
        }

        public Task<List<Question>> GetQuestionsByQuizIdAsync(int quizId)
        {
            return _questionRepository.GetQuestionsByQuizIdAsync(quizId);
        }

        public Task<Question?> GetQuestionByIdAsync(int id)
        {
            return _questionRepository.GetQuestionByIdAsync(id);
        }

        public async Task<bool> CreateQuestionAsync(Question question)
        {
            try
            {
                await _questionRepository.AddQuestionAsync(question);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateQuestionAsync(Question question)
        {
            try
            {
                await _questionRepository.UpdateQuestionAsync(question);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteQuestionAsync(int id)
        {
            try
            {
                await _questionRepository.DeleteQuestionAsync(id);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
