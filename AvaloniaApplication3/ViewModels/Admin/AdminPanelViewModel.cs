using AvaloniaApplication3.Models;
using AvaloniaApplication3.Models.Enums;
using AvaloniaApplication3.Repositories;
using AvaloniaApplication3.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvaloniaApplication3.ViewModels.Admin
{
    public partial class AdminPanelViewModel : ViewModelBase
    {
        [ObservableProperty]
        private object? currentSection;

        [ObservableProperty]
        private string saveStatus = "No changes";

        public IRelayCommand<string> ToggleSectionCommand { get; }
        public IRelayCommand SaveCommand { get; }

        public List<string> SectionNames => new(_sections.Keys);

        private readonly Dictionary<string, Func<object>> _sections;

        public AdminPanelViewModel(User currentUser)
        {
            if (currentUser.Role is not (UserRole.Admin or UserRole.Moderator))
                throw new UnauthorizedAccessException("Access denied to Admin Panel.");

            _sections = new Dictionary<string, Func<object>>
                {
                    { "Quiz Management", CreateQuizManagementViewModel },
                    { "User Management", CreateUserManagementViewModel }
                };

            ToggleSectionCommand = new RelayCommand<string>(ToggleSection);
            SaveCommand = new AsyncRelayCommand(SaveCurrentEditorAsync);
        }



        private void ToggleSection(string? sectionName)
        {
            if (string.IsNullOrEmpty(sectionName) || !_sections.TryGetValue(sectionName, out var createSection))
                return;

            var next = createSection();
            CurrentSection = CurrentSection?.GetType() == next.GetType() ? null : next;
        }

        private async Task SaveCurrentEditorAsync()
        {
            if (CurrentSection is QuizManagementViewModel quizMgmt && quizMgmt.CurrentQuizEditor != null)
            {
                SaveStatus = "Saving...";
                await quizMgmt.CurrentQuizEditor.SaveQuizAsync();
                SaveStatus = "Saved";
            }
            else
            {
                SaveStatus = "Nothing to save";
            }
        }

        private object CreateQuizManagementViewModel()
        {
            var quizService = new QuizService(new EfQuizRepository(new AppDbContext()));
            return new QuizManagementViewModel(quizService);
        }

        private object CreateUserManagementViewModel()
        {
            var userService = new UserService(new EfUserRepository(new AppDbContext()));
            return new UserManagementViewModel(userService);
        }
    }
}
