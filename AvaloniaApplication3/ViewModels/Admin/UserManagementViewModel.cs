using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvaloniaApplication3.Models;
using AvaloniaApplication3.Services;
using AvaloniaApplication3.Utility;

namespace AvaloniaApplication3.ViewModels.Admin
{
    public partial class UserManagementViewModel : ViewModelBase
    {
        private readonly IUserService _userService;

        [ObservableProperty]
        private ObservableCollection<User> users = new();

        [ObservableProperty]
        private string? status;

        [ObservableProperty]
        private UserEditorViewModel? selectedUserEditor;

        public UserManagementViewModel(IUserService userService)
        {
            _userService = userService;
            _ = LoadUsersAsync(); // initial load
        }

        // Generates: IAsyncRelayCommand LoadUsersAsyncCommand
        [RelayCommand]
        private async Task LoadUsersAsync()
        {
            var all = await _userService.GetAllUsersAsync();
            Users = new ObservableCollection<User>(all);
        }

        // Generates: IAsyncRelayCommand DeleteUserAsyncCommand
        [RelayCommand]
        private async Task DeleteUserAsync(User? user)
        {
            if (user is null) return;

            await _userService.DeleteUserAsync(user.Username);
            await LoadUsersAsync();
            Status = $"Deleted user {user.Username}";
        }

        // Generates: IRelayCommand ToggleUserEditorCommand
        [RelayCommand]
        private void ToggleUserEditor(User? user)
        {
            if (user is null) return;

            if (SelectedUserEditor?.User == user)
                SelectedUserEditor = null; // toggle off
            else
                SelectedUserEditor = new UserEditorViewModel(user, _userService);
        }
    }
}
