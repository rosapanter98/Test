using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using AvaloniaApplication3.Models;
using AvaloniaApplication3.Services;
using AvaloniaApplication3.Utility;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AvaloniaApplication3.ViewModels.Admin
{
    public partial class QuizManagementViewModel : ViewModelBase
    {
        private readonly IQuizService _quizService;
        private readonly IQuizImportService _quizImportService;

        [ObservableProperty]
        private ObservableCollection<Quiz> allQuizzes = new();

        [ObservableProperty]
        private string newQuizTitle = string.Empty;

        [ObservableProperty]
        private string? newQuizDescription;

        [ObservableProperty]
        private bool showCreateForm;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SaveCommand))]
        private QuizEditorViewModel? currentQuizEditor;

        [ObservableProperty]
        private string? statusMessage;

        // Expose editor's Save command for binding (updates when CurrentQuizEditor changes)
        public IAsyncRelayCommand? SaveCommand =>
            CurrentQuizEditor is { } ed ? ed.SaveQuizCommand : null;

        public QuizManagementViewModel(IQuizService quizService, IQuizImportService quizImportService)
        {
            _quizService = quizService;
            _quizImportService = quizImportService;

            // initial load
            _ = LoadQuizzesAsync();
        }

        [RelayCommand]
        private async Task LoadQuizzesAsync()
        {
            var loaded = await _quizService.GetAllQuizzesAsync();
            AllQuizzes = new ObservableCollection<Quiz>(loaded);
        }

        [RelayCommand]
        private void ToggleCreateForm() => ShowCreateForm = !ShowCreateForm;

        [RelayCommand]
        private async Task CreateQuizAsync()
        {
            var title = (NewQuizTitle ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(title))
                return;

            var quiz = new Quiz
            {
                Title = title,
                Description = (NewQuizDescription ?? string.Empty).Trim()
            };

            var ok = await _quizService.CreateQuizAsync(quiz);
            if (!ok)
            {
                StatusMessage = "Failed to create quiz.";
                return;
            }

            NewQuizTitle = string.Empty;
            NewQuizDescription = string.Empty;
            ShowCreateForm = false;

            await LoadQuizzesAsync();
            StatusMessage = $"Created quiz: {quiz.Title}";
        }

        [RelayCommand]
        private async Task DeleteQuizAsync(Quiz? quiz)
        {
            if (quiz is null) return;

            var ok = await _quizService.DeleteQuizAsync(quiz.Id);
            if (!ok)
            {
                StatusMessage = "Failed to delete quiz.";
                return;
            }

            if (CurrentQuizEditor?.Quiz.Id == quiz.Id)
                CurrentQuizEditor = null;

            await LoadQuizzesAsync();
            StatusMessage = $"Deleted quiz: {quiz.Title}";
        }

        [RelayCommand]
        private async Task EditQuizAsync(Quiz? quiz)
        {
            if (quiz is null) return;

            var fullQuiz = await _quizService.GetFullQuizAsync(quiz.Id);
            if (fullQuiz is null)
            {
                StatusMessage = "Failed to load quiz.";
                return;
            }

            var editor = new QuizEditorViewModel(fullQuiz, _quizService);
            editor.RemoveRequested = HandleQuizEditorClosed;
            CurrentQuizEditor = editor;
            StatusMessage = $"Editing: {fullQuiz.Title}";
        }

        [RelayCommand]
        private async Task ImportQuizAsync()
        {
            var lifetime = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            var window = lifetime?.MainWindow;
            if (window is null) return;

            var top = TopLevel.GetTopLevel(window)!;
            var files = await top.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Import Quiz JSON",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
            new FilePickerFileType("JSON")     { Patterns = new[] { "*.json" } },
            new FilePickerFileType("All files"){ Patterns = new[] { "*" } }
        }
            });

            var file = files.FirstOrDefault();
            if (file is null) return;

            try
            {
                using var stream = await file.OpenReadAsync();
                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync();
                if (string.IsNullOrWhiteSpace(json))
                {
                    StatusMessage = "Import failed: file is empty.";
                    return;
                }

                // NOTE: use the JSON overload, not the file-path overload
                var id = await _quizImportService.ImportFromJsonAsync(json, replaceIfTitleExists: true);

                await LoadQuizzesAsync();
                var imported = AllQuizzes.FirstOrDefault(q => q.Id == id);
                StatusMessage = imported is null ? "Imported quiz." : $"Imported: {imported.Title}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Import failed: {ex.Message}";
            }
        }


        private void HandleQuizEditorClosed(QuizEditorViewModel vm)
        {
            if (CurrentQuizEditor == vm)
                CurrentQuizEditor = null;

            _ = LoadQuizzesAsync();
        }
    }
}
