using AvaloniaApplication3.Models;
using AvaloniaApplication3.Services;
using AvaloniaApplication3.ViewModels.Admin;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaApplication3.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty] private object? currentView;
        [ObservableProperty] private bool isLoggedIn;
        [ObservableProperty] private string? displayUsername;
        [ObservableProperty] private User? currentUser;

        public bool CanAccessAdminPanel => IsLoggedIn && CurrentUser?.Role >= UserRole.Moderator;

        private readonly ISessionService _session;
        private readonly ILoginService _loginService;
        private readonly IQuizService _quizService;
        private readonly IQuizAttemptService _quizAttemptService;

        private AdminPanelViewModel? _adminVm;
        private QuizSessionViewModel? _quizSessionVm;

        public MainWindowViewModel(
            ISessionService session,
            ILoginService loginService,
            IQuizService quizService,
            IQuizAttemptService quizAttemptService)
        {
            _session = session;
            _loginService = loginService;
            _quizService = quizService;
            _quizAttemptService = quizAttemptService;

            _session.LoggedIn += OnLoggedIn;
            _session.LoggedOut += OnLoggedOut;

            ShowLogin();
        }

        [RelayCommand]
        private void ShowLogin()
        {
            CurrentView = new LoginViewModel(_loginService, _session.CompleteLogin);
            IsLoggedIn = false;
            DisplayUsername = null;
            CurrentUser = null;
            _adminVm = null;
            _quizSessionVm = null;
        }

        [RelayCommand]
        private void ShowRegister()
        {
            CurrentView = new RegisterViewModel(
                _loginService,
                onRegisterSuccess: () => CurrentView = new LoginViewModel(_loginService, _session.CompleteLogin),
                onBack: ShowLogin);

        }

        [RelayCommand(CanExecute = nameof(IsLoggedIn))]
        private void ShowQuiz()
        {
            // Create (or reuse) the session root for quiz area
            _quizSessionVm ??= new QuizSessionViewModel(CurrentUser!, _quizService, _quizAttemptService);
            CurrentView = _quizSessionVm;
        }

        [RelayCommand(CanExecute = nameof(CanAccessAdminPanel))]
        private void ShowAdminPanel()
        {
            if (_adminVm != null) CurrentView = _adminVm;
        }

        [RelayCommand(CanExecute = nameof(IsLoggedIn))]
        private void Logout() => _session.Logout();

        private void OnLoggedIn(User user)
        {
            CurrentUser = user;
            IsLoggedIn = true;
            DisplayUsername = $"Logged in as: {user.DisplayName} ({user.Username})";

            if (user.Role is UserRole.Admin or UserRole.Moderator)
                _adminVm = new AdminPanelViewModel(user);

            // Optionally land on Quiz immediately; otherwise keep login and let user click "Quiz"
            // ShowQuiz();

            NotifyAllCanExecuteChanged();
            OnPropertyChanged(nameof(CanAccessAdminPanel));
        }

        private void OnLoggedOut()
        {
            ShowLogin();
            NotifyAllCanExecuteChanged();
            OnPropertyChanged(nameof(CanAccessAdminPanel));
        }
    }
}
