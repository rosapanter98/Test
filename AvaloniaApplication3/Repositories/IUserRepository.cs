using AvaloniaApplication3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaApplication3.Repositories
{
    public interface IUserRepository
    {
        User? GetUserByUsername(string username);
        IEnumerable<User> GetAllUsers();
    }
}
