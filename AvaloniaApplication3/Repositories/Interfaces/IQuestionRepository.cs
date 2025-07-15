using AvaloniaApplication3.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvaloniaApplication3.Repositories
{
    public interface IQuestionRepository
    {
        Task<List<Question>> GetQuestionsByQuizIdAsync(int quizId);
        Task<Question?> GetQuestionByIdAsync(int id);
        Task AddQuestionAsync(Question question);
        Task UpdateQuestionAsync(Question question);
        Task DeleteQuestionAsync(int id);
    }
}
