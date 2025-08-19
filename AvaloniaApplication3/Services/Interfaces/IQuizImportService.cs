using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaApplication3.Services
{
    public interface IQuizImportService
    {
        Task<int> ImportFromFileAsync(string path, bool replaceIfTitleExists = true);

        Task<int> ImportFromJsonAsync(string json, bool replaceIfTitleExists = true);
    }
}
