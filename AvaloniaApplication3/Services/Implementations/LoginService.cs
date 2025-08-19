using AvaloniaApplication3.Models;
using AvaloniaApplication3.Models.Enums;
using AvaloniaApplication3.Repositories;
using AvaloniaApplication3.Utility;
using System.Threading.Tasks;

namespace AvaloniaApplication3.Services
{
    public class LoginService : ILoginService
    {
        private readonly IUserRepository _userRepository;

        public LoginService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User?> AuthenticateAsync(string? username, string? password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;

            var user = await _userRepository.GetUserByUsernameAsync(username.Trim());
            if (user is not null && PasswordHelper.VerifyPassword(password, user.PasswordHash))
                return user;

            return null;
        }

        public async Task<bool> RegisterAsync(string username, string password)
        {
            if (await _userRepository.UserExistsAsync(username))
                return false;

            var user = new User
            {
                Username = username.Trim(),
                DisplayName = username.Trim(),
                Email = $"{username.Trim()}@local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = UserRole.Member
            };
             try
            {
                await _userRepository.AddUserAsync(user);
                return true;
            }
            catch
            {
                // In case of race, the unique index will throw; convert to false.
                return false;
            }
        }
    }
}
