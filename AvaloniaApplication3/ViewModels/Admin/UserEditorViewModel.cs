using AvaloniaApplication3.Models;
using AvaloniaApplication3.Models.Enums;
using AvaloniaApplication3.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvaloniaApplication3.ViewModels.Admin
{
    public partial class UserEditorViewModel : ViewModelBase
    {
        private readonly IUserService _userService;

        [ObservableProperty]
        private User user;

        [ObservableProperty]
        private string saveStatus = string.Empty;

        [ObservableProperty]
        private UserRole selectedRole;

        public IEnumerable<UserRole> Roles => Enum.GetValues<UserRole>();

        public IRelayCommand SaveCommand { get; }

        public UserEditorViewModel(User user, IUserService userService)
        {
            User = user;
            _userService = userService;
            SelectedRole = User.Role;
            SaveCommand = new AsyncRelayCommand(SaveAsync);

        }

        partial void OnSelectedRoleChanged(UserRole value)
        {
            if (User != null)
                User.Role = value;
        }


        private async Task SaveAsync()
        {
            SaveStatus = "Saving...";
            await _userService.UpdateUserAsync(User);
            SaveStatus = "Saved";
        }
    }
}
