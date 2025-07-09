using AvaloniaApplication3.Models;
using AvaloniaApplication3.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaApplication3.Services
{
    public class SimpleLoginService : ILoginService
    {
        private readonly IUserRepository _userRepository;

        public SimpleLoginService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public User? Authenticate(string? username, string? password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;

            var user = _userRepository.GetUserByUsername(username.Trim());

            if (user is not null && user.Password == password) // plaintext for now
                return user;

            return null;
        }
    }
}
