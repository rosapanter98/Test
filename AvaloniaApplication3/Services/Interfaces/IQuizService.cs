using AvaloniaApplication3.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvaloniaApplication3.Services
{
    public interface IQuizService
    {
        Task<List<Quiz>> GetAllQuizzesAsync();
        Task<Quiz?> GetFullQuizAsync(int id);
        Task<bool> CreateQuizAsync(Quiz quiz);
        Task<bool> UpdateQuizAsync(Quiz quiz);
        Task<bool> DeleteQuizAsync(int id);
    }
}
