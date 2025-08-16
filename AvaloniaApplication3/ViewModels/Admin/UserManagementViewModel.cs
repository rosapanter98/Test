using AvaloniaApplication3.Models;
using AvaloniaApplication3.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

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


        public IRelayCommand RefreshCommand { get; }
        public IRelayCommand<User> DeleteUserCommand { get; }
        public IRelayCommand<User> EditUserCommand { get; }

        public UserManagementViewModel(IUserService userService)
        {
            _userService = userService;
            RefreshCommand = new AsyncRelayCommand(LoadUsersAsync);
            DeleteUserCommand = new AsyncRelayCommand<User>(DeleteUserAsync);
            EditUserCommand = new RelayCommand<User>(ToggleUserEditor);
            _ = LoadUsersAsync();
        }

        private async Task LoadUsersAsync()
        {
            var all = await _userService.GetAllUsersAsync();
            Users = new ObservableCollection<User>(all);
        }

        private async Task DeleteUserAsync(User? user)
        {
            if (user is null) return;

            await _userService.DeleteUserAsync(user.Username);
            await LoadUsersAsync();
            Status = $"Deleted user {user.Username}";
        }

        private void ToggleUserEditor(User? user)
        {
            if (user is null)
                return;

            if (SelectedUserEditor?.User == user)
            {
                SelectedUserEditor = null; // toggle off
            }
            else
            {
                SelectedUserEditor = new UserEditorViewModel(user, _userService);
            }
        }
    }
}
