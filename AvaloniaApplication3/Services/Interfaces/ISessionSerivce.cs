using AvaloniaApplication3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaApplication3.Services
{
    public interface ISessionService
    {
        bool IsLoggedIn { get; }
        User? CurrentUser { get; }

        event Action<User> LoggedIn;
        event Action LoggedOut;

        // Typically called from LoginViewModel after credentials validated
        void CompleteLogin(User user);
        void Logout();
    }
}