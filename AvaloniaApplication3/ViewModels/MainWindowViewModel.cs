using AvaloniaApplication3.Models;
using AvaloniaApplication3.Repositories;
using AvaloniaApplication3.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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
        private readonly IQuizService _quizService;
        private readonly QuizSelectionViewModel _quizSelectionViewModel;

        public MainWindowViewModel()
        {
            var userRepo = new EfUserRepository(new AppDbContext());
            _loginService = new LoginService(userRepo);

            var quizRepo = new EfQuizRepository(new AppDbContext());
            _quizService = new QuizService(quizRepo);

            _quizSelectionViewModel = new QuizSelectionViewModel(_quizService, ShowQuizRunner);

            ShowLogin();
        }

        [RelayCommand(CanExecute = nameof(NotLoggedIn))]
        private void ShowRegister()
        {
            CurrentView = new RegisterViewModel(_loginService, OnRegisterSuccess, ShowLogin);
        }

        private void OnRegisterSuccess()
        {
            ShowLogin();
        }

        public bool NotLoggedIn => !IsLoggedIn;

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
        private void ShowQuiz()
        {
            CurrentView = _quizSelectionViewModel;
        }

        private void ShowQuizRunner(Quiz quiz)
        {
            CurrentView = new QuizRunnerViewModel(quiz, ShowQuizResults);
        }

        private void ShowQuizResults(string title, int score, int totalQuestions)
        {
            CurrentView = new QuizResultsViewModel(score, totalQuestions, title, () =>
            {
                CurrentView = _quizSelectionViewModel;
            });
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
            ShowToDoListCommand.NotifyCanExecuteChanged();
            GoHomeCommand.NotifyCanExecuteChanged();
            LogoutCommand.NotifyCanExecuteChanged();
            ShowQuizCommand.NotifyCanExecuteChanged();
        }
    }
}
