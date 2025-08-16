using System;
using AvaloniaApplication3.Models;

namespace AvaloniaApplication3.Services
{
    public sealed class SessionService : ISessionService
    {
        public bool IsLoggedIn { get; private set; }
        public User? CurrentUser { get; private set; }

        public event Action<User>? LoggedIn;
        public event Action? LoggedOut;

        public void CompleteLogin(User user)
        {
            CurrentUser = user;
            IsLoggedIn = true;
            LoggedIn?.Invoke(user);
        }

        public void Logout()
        {
            CurrentUser = null;
            IsLoggedIn = false;
            LoggedOut?.Invoke();
        }
    }
}
