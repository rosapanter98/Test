using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaApplication3.Services.Interfaces
{
    public interface IQuizImportService
    {
        Task<int> ImportFromFileAsync(string path, bool overwriteExisting = true);
    }
}
