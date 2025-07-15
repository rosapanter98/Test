using AvaloniaApplication3.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvaloniaApplication3.Services
{
    public interface IQuestionService
    {
        Task<List<Question>> GetQuestionsByQuizIdAsync(int quizId);
        Task<Question?> GetQuestionByIdAsync(int id);

        Task<bool> CreateQuestionAsync(Question question);
        Task<bool> UpdateQuestionAsync(Question question);
        Task<bool> DeleteQuestionAsync(int id);
    }
}
