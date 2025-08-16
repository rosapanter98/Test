using AvaloniaApplication3.Models;
using AvaloniaApplication3.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace AvaloniaApplication3.ViewModels.Admin
{
    public partial class QuizEditorViewModel : ViewModelBase
    {
        [ObservableProperty] private Quiz quiz;

        [ObservableProperty] private ObservableCollection<QuestionEditorViewModel> questions;

        private readonly IQuizService _quizService;

        public delegate void QuizRemovedCallback(QuizEditorViewModel vm);
        public QuizRemovedCallback? RemoveRequested { get; set; }

        public QuizEditorViewModel(Quiz quiz, IQuizService quizService)
        {
            _quizService = quizService;
            Quiz = quiz;

            quiz.Questions ??= new List<Question>();

            Questions = new ObservableCollection<QuestionEditorViewModel>(
                quiz.Questions.Select(q =>
                {
                    var vm = new QuestionEditorViewModel(q);
                    vm.RemoveRequested = RemoveQuestion;
                    return vm;
                }));


        }

        [RelayCommand]
        private void AddQuestion()
        {
            var newQuestion = new Question
            {
                Text = "New question",
                Explanation = "",
                Type = QuestionType.SingleChoice,
                Answers = new List<Answer>()
            };

            var questionVm = new QuestionEditorViewModel(newQuestion);
            questionVm.RemoveRequested = RemoveQuestion;

            Questions.Add(questionVm);
            Quiz.Questions.Add(newQuestion);
        }

        [RelayCommand]
        public void RemoveQuestion(QuestionEditorViewModel? questionVm)
        {
            if (questionVm == null) return;

            Questions.Remove(questionVm);
            Quiz.Questions.Remove(questionVm.Question);
        }

        [RelayCommand]
        public async Task SaveQuizAsync()
        {
            await _quizService.UpdateQuizAsync(Quiz);
        }
    }
}
