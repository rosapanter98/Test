using System.IO;
using System.Threading.Tasks;
using AvaloniaApplication3.Utility;
using AvaloniaApplication3.Services;

namespace AvaloniaApplication3.Services
{
    public class QuizImportService : IQuizImportService
    {
        private readonly AppDbContext _context;

        public QuizImportService(AppDbContext context)
        {
            _context = context;
        }

        public Task<int> ImportFromFileAsync(string path, bool replaceIfTitleExists = true)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Quiz file not found.", path);

            return QuizJsonImporter.ImportFromFileAsync(_context, path, replaceIfTitleExists);
        }

        public Task<int> ImportFromJsonAsync(string json, bool replaceIfTitleExists = true)
        {
            return QuizJsonImporter.ImportFromJsonAsync(_context, json, replaceIfTitleExists);
        }
    }
}
