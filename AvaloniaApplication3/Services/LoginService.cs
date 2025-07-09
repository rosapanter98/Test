using AvaloniaApplication3.Models;
using AvaloniaApplication3.Repositories;
using AvaloniaApplication3.Security;
using System;
using System.Security.Cryptography;
using System.Text;
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
            var existing = await _userRepository.GetUserByUsernameAsync(username);
            if (existing != null)
                return false;

            var hash = PasswordHelper.HashPassword(password);
            var user = new User
            {
                Username = username,
                DisplayName = username, // Change if you want separate field
                PasswordHash = hash
            };

            await _userRepository.AddUserAsync(user);
            return true;
        }

    }
}