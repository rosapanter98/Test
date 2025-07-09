using AvaloniaApplication3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaApplication3.Services
{
    public interface ILoginService
    {
        User? Authenticate(string? username, string? password);
    }
}
