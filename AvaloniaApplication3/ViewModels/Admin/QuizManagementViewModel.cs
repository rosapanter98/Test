using AvaloniaApplication3.Models;
using AvaloniaApplication3.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace AvaloniaApplication3.ViewModels.Admin
{
    public partial class QuizManagementViewModel : ViewModelBase
    {
        private readonly IQuizService _quizService;

        [ObservableProperty]
        private ObservableCollection<Quiz> quizzes = new();

        [ObservableProperty]
        private string newQuizTitle = string.Empty;

        [ObservableProperty]
        private string? newQuizDescription;

        [ObservableProperty]
        private bool showCreateForm;

        [ObservableProperty]
        private QuizEditorViewModel? currentQuizEditor;

        public IAsyncRelayCommand? SaveCommand =>
            (IAsyncRelayCommand?)(CurrentQuizEditor is QuizEditorViewModel editor ? editor.SaveQuizCommand : null);

        public IRelayCommand ToggleCreateFormCommand { get; }
        public IRelayCommand CreateQuizCommand { get; }
        public IRelayCommand<Quiz> EditQuizCommand { get; }
        public IRelayCommand<Quiz> DeleteQuizCommand { get; }

        public QuizManagementViewModel(IQuizService quizService)
        {
            _quizService = quizService;

            ToggleCreateFormCommand = new RelayCommand(() => ShowCreateForm = !ShowCreateForm);
            CreateQuizCommand = new RelayCommand(CreateQuiz);
            EditQuizCommand = new RelayCommand<Quiz>(OpenEditor);
            DeleteQuizCommand = new AsyncRelayCommand<Quiz>(DeleteQuizAsync);

            _ = LoadQuizzes();
        }

        private async Task LoadQuizzes()
        {
            var loaded = await _quizService.GetAllQuizzesAsync();
            Quizzes = new ObservableCollection<Quiz>(loaded);
        }

        private async void CreateQuiz()
        {
            if (string.IsNullOrWhiteSpace(NewQuizTitle)) return;

            var quiz = new Quiz
            {
                Title = NewQuizTitle,
                Description = NewQuizDescription
            };

            await _quizService.CreateQuizAsync(quiz);

            NewQuizTitle = string.Empty;
            NewQuizDescription = string.Empty;

            await LoadQuizzes();
        }

        private async Task DeleteQuizAsync(Quiz? quiz)
        {
            if (quiz == null) return;

            await _quizService.DeleteQuizAsync(quiz.Id);

            if (CurrentQuizEditor?.Quiz.Id == quiz.Id)
                CurrentQuizEditor = null;

            await LoadQuizzes();
        }

        private async void OpenEditor(Quiz? quiz)
        {
            if (quiz == null) return;

            var fullQuiz = await _quizService.GetFullQuizAsync(quiz.Id);
            if (fullQuiz != null)
            {
                var editor = new QuizEditorViewModel(fullQuiz, _quizService);
                editor.RemoveRequested = HandleQuizEditorClosed;
                CurrentQuizEditor = editor;
            }
        }

        private void HandleQuizEditorClosed(QuizEditorViewModel vm)
        {
            if (CurrentQuizEditor == vm)
                CurrentQuizEditor = null;

            _ = LoadQuizzes();
        }
    }
}
