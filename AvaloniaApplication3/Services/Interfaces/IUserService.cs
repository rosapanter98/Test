using AvaloniaApplication3.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvaloniaApplication3.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(string username);
    }
}
