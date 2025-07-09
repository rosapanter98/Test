using AvaloniaApplication3.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaApplication3.ViewModels
{
    public partial class RegisterViewModel : ViewModelBase
    {
        private readonly ILoginService _loginService;
        private readonly Action _onRegisterSuccess;
        private readonly Action _onBack;

        [ObservableProperty] private string? username;
        [ObservableProperty] private string? password;
        [ObservableProperty] private string? confirmPassword;

        public RegisterViewModel(ILoginService loginService, Action onRegisterSuccess, Action onBack)
        {
            _loginService = loginService;
            _onRegisterSuccess = onRegisterSuccess;
            _onBack = onBack;
        }

        [RelayCommand]
        private async Task Register()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                return;

            if (Password != ConfirmPassword)
                return;

            var success = await _loginService.RegisterAsync(Username.Trim(), Password);
            if (success)
                _onRegisterSuccess();
        }

        [RelayCommand]
        private void BackToLogin() => _onBack();
    }
}