using AvaloniaApplication3.Models;
using AvaloniaApplication3.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;

namespace AvaloniaApplication3.ViewModels
{
    public partial class LoginViewModel : ViewModelBase
    {
        private readonly ILoginService _loginService;
        private readonly Action<Models.User> _onLoginSuccess;

        [ObservableProperty]
        private string? usernameInput;

        [ObservableProperty]
        private string? passwordInput;

        public LoginViewModel(ILoginService loginService, Action<Models.User> onLoginSuccess)
        {
            _loginService = loginService;
            _onLoginSuccess = onLoginSuccess;
        }

        [RelayCommand]
        private async Task Login()
        {
            var user = await _loginService.AuthenticateAsync(UsernameInput, PasswordInput);
            if (user is not null)
            {
                _onLoginSuccess(user);
            }
            else
            {
                UsernameInput = string.Empty;
                PasswordInput = string.Empty;
            }
        }

    }
}
