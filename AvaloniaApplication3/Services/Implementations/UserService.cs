using AvaloniaApplication3.Models;
using AvaloniaApplication3.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvaloniaApplication3.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllUsersAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _userRepository.GetUserByIdAsync(id);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _userRepository.GetUserByUsernameAsync(username);
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            var existing = await _userRepository.GetUserByIdAsync(user.Id);
            if (existing == null) return false;

            existing.DisplayName = user.DisplayName;
            existing.Email = user.Email;
            existing.Role = user.Role;
            await _userRepository.UpdateUserAsync(existing);

            return true;
        }

        public async Task<bool> DeleteUserAsync(string username)
        {
            var user = await _userRepository.GetUserByUsernameAsync(username);
            if (user == null) return false;

            await _userRepository.DeleteUserAsync(username);
            return true;
        }
    }
}
