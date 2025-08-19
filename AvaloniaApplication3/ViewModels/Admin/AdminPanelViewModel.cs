using AvaloniaApplication3.Models;
using AvaloniaApplication3.Models.Enums;
using AvaloniaApplication3.Services;
using AvaloniaApplication3.Services.Interfaces;
using AvaloniaApplication3.Utility;
using AvaloniaApplication3.ViewModels.Admin;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvaloniaApplication3.ViewModels.Admin
{
    public partial class AdminPanelViewModel : ViewModelBase
    {
        private readonly IQuizService _quizService;
        private readonly IQuizImportService _quizImportService;
        private readonly IUserService _userService;

        [ObservableProperty]
        private object? currentSection;

        [ObservableProperty]
        private string saveStatus = "No changes";

        public List<string> SectionNames => new(_sections.Keys);

        private readonly Dictionary<string, Func<object>> _sections;

        public AdminPanelViewModel(
            User currentUser,
            IQuizService quizService,
            IQuizImportService quizImportService,
            IUserService userService)
        {
            if (currentUser.Role is not (UserRole.Admin or UserRole.Moderator))
                throw new UnauthorizedAccessException("Access denied to Admin Panel.");

            _quizService = quizService;
            _quizImportService = quizImportService;
            _userService = userService;

            _sections = new Dictionary<string, Func<object>>
            {
                { "Quiz Management", CreateQuizManagementViewModel },
                { "User Management", CreateUserManagementViewModel }
            };
        }

        [RelayCommand]
        private void ToggleSection(string? sectionName)
        {
            if (string.IsNullOrWhiteSpace(sectionName) || !_sections.TryGetValue(sectionName, out var factory))
                return;

            var next = factory();
            CurrentSection = CurrentSection?.GetType() == next.GetType() ? null : next;
        }

        [RelayCommand]
        private async Task Save()
        {
            if (CurrentSection is QuizManagementViewModel quizMgmt &&
                quizMgmt.SaveCommand is IAsyncRelayCommand saveCmd)
            {
                SaveStatus = "Saving...";
                await saveCmd.ExecuteAsync(null);
                SaveStatus = "Saved";
            }
            else
            {
                SaveStatus = "Nothing to save";
            }
        }

        private object CreateQuizManagementViewModel()
            => new QuizManagementViewModel(_quizService, _quizImportService);

        private object CreateUserManagementViewModel()
            => new UserManagementViewModel(_userService);
    }
}
