using AvaloniaApplication3.Services.Interfaces;
using AvaloniaApplication3.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaApplication3.Services.Implementations
{
    public class QuizImportService : IQuizImportService
    {
        private readonly AppDbContext _context;
        public QuizImportService(AppDbContext context)
        {
            _context = context;
        }

        public Task<int> ImportFromFileAsync(string path, bool overwriteExisting = true)
        {
            if (!File.Exists(path)) throw new FileNotFoundException("Quiz file not found.", path);
            return QuizJsonImporter.ImportFromFileAsync(_context, path, overwriteExisting);
        }
    }
}
