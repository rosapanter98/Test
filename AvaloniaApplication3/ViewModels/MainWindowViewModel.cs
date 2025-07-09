using AvaloniaApplication3.Models;
using AvaloniaApplication3.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace AvaloniaApplication3.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private object? currentView;

        [ObservableProperty]
        private bool isLoggedIn;

        [ObservableProperty]
        private string? displayUsername;

        [ObservableProperty]
        private User? currentUser;

        private ToDoListViewModel? _toDoList;
        private readonly ILoginService _loginService;

        public MainWindowViewModel()
        {
            // Swap this out later with a real login service
            var userRepo = new HardcodedUserRepository();
            _loginService = new SimpleLoginService(userRepo);

            ShowLogin();
        }

        private void ShowLogin()
        {
            CurrentView = new LoginViewModel(_loginService, OnLoginSuccess);
            IsLoggedIn = false;
            DisplayUsername = null;
            CurrentUser = null;
        }

        private void OnLoginSuccess(User user)
        {
            CurrentUser = user;
            IsLoggedIn = true;
            DisplayUsername = $"Logged in as: {user.DisplayName} ({user.Username})";

            var todoRepo = new JsonToDoRepository();
            _toDoList = new ToDoListViewModel(user.Username, SwitchToItemView, todoRepo);

            CurrentView = _toDoList;
            UpdateCommands();
        }

        [RelayCommand(CanExecute = nameof(IsLoggedIn))]
        private void ShowToDoList()
        {
            if (_toDoList is not null)
                CurrentView = _toDoList;
        }

        [RelayCommand(CanExecute = nameof(IsLoggedIn))]
        private void GoHome()
        {
            CurrentView = new MainPageViewModel();
        }

        [RelayCommand(CanExecute = nameof(IsLoggedIn))]
        private void Logout()
        {
            CurrentUser = null;
            _toDoList = null;
            ShowLogin();
            UpdateCommands();
        }

        private void SwitchToItemView(ToDoItemViewModel item)
        {
            _toDoList?.PrepareItem(item, view => CurrentView = view);
            CurrentView = item;
        }

        private void UpdateCommands()
        {
            // Notify all RelayCommands that CanExecute may have changed
            ShowToDoListCommand.NotifyCanExecuteChanged();
            GoHomeCommand.NotifyCanExecuteChanged();
            LogoutCommand.NotifyCanExecuteChanged();
        }
    }
}
