using AvaloniaApplication3.Models;
using AvaloniaApplication3.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AvaloniaApplication3.ViewModels
{
    public partial class QuizSelectionViewModel : ViewModelBase
    {
        private readonly IQuizService _quizService;
        private readonly Action<Quiz> _onQuizSelected;

        [ObservableProperty]
        private ObservableCollection<Quiz> _quizzes = new();

        [ObservableProperty]
        private Quiz? _selectedQuiz;

        public ICommand StartQuizCommand { get; }

        public QuizSelectionViewModel(IQuizService quizService, Action<Quiz> onQuizSelected)
        {
            _quizService = quizService;
            _onQuizSelected = onQuizSelected;

            StartQuizCommand = new RelayCommand(StartQuiz, CanStartQuiz);
            LoadQuizzesAsync();
        }

        private async void LoadQuizzesAsync()
        {
            var quizList = await _quizService.GetAllQuizzesAsync();
            Quizzes = new ObservableCollection<Quiz>(quizList);
        }

        private bool CanStartQuiz() => SelectedQuiz != null;

        private async void StartQuiz()
        {
            if (SelectedQuiz != null)
            {
                var fullQuiz = await _quizService.GetFullQuizAsync(SelectedQuiz.Id);
                if (fullQuiz != null)
                {
                    Console.WriteLine($"[DEBUG] Quiz: {fullQuiz.Title}");
                    Console.WriteLine($"[DEBUG] Questions loaded: {fullQuiz.Questions.Count}");

                    foreach (var question in fullQuiz.Questions)
                    {
                        Console.WriteLine($"[DEBUG] Question {question.Id}: {question.Text}");
                        Console.WriteLine($"[DEBUG] -> Answers loaded: {question.Answers.Count}");
                    }

                    _onQuizSelected.Invoke(fullQuiz);
                }
            }
        }



        partial void OnSelectedQuizChanged(Quiz? value)
        {
            (StartQuizCommand as RelayCommand)?.NotifyCanExecuteChanged();
        }

    }
}
