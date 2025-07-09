using AvaloniaApplication3.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AvaloniaApplication3.Repositories
{
    public class HardcodedUserRepository : IUserRepository
    {
        private readonly List<User> _users = new()
    {
        new User { Username = "alice", Password = "1234" },
        new User { Username = "bob", Password = "abcd" }
    };

        public IEnumerable<User> GetAllUsers() => _users;

        public User? GetUserByUsername(string username)
            => _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
    }
}
