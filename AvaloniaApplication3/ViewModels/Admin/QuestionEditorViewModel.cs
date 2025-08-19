using AvaloniaApplication3.Models;
using AvaloniaApplication3.Utility;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AvaloniaApplication3.ViewModels.Admin
{
    public partial class QuestionEditorViewModel : ViewModelBase
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Text))]
        [NotifyPropertyChangedFor(nameof(Explanation))]
        [NotifyPropertyChangedFor(nameof(Type))]
        private Question question;

        [ObservableProperty]
        private ObservableCollection<AnswerEditorViewModel> answers;

        public ObservableCollection<QuestionType> QuestionTypeOptions { get; } =
            new((QuestionType[])Enum.GetValues(typeof(QuestionType)));

        public Action<QuestionEditorViewModel>? RemoveRequested { get; set; }

        public QuestionEditorViewModel(Question question)
        {
            Question = question;
            if (Question.Answers == null)
                Question.Answers = new List<Answer>();

            Answers = new ObservableCollection<AnswerEditorViewModel>(
                Question.Answers.Select(a =>
                {
                    var vm = new AnswerEditorViewModel(a);
                    vm.RemoveRequested = RemoveAnswer;
                    return vm;
                }));
        }

        // Proxy properties to the model without manual boilerplate
        public string Text
        {
            get => Question.Text;
            set => SetProperty(Question.Text, value, Question, static (q, v) => q.Text = v);
        }

        public string? Explanation
        {
            get => Question.Explanation;
            set => SetProperty(Question.Explanation, value, Question, static (q, v) => q.Explanation = v);
        }

        public QuestionType Type
        {
            get => Question.Type;
            set => SetProperty(Question.Type, value, Question, static (q, v) => q.Type = v);
        }

        [RelayCommand]
        private void AddAnswer()
        {
            var a = new Answer { Text = "New answer", IsCorrect = false };
            Question.Answers.Add(a);

            var vm = new AnswerEditorViewModel(a);
            vm.RemoveRequested = RemoveAnswer;
            Answers.Add(vm);
        }

        [RelayCommand]
        private void RemoveSelf() => RemoveRequested?.Invoke(this);

        private void RemoveAnswer(AnswerEditorViewModel vm)
        {
            Answers.Remove(vm);
            Question.Answers.Remove(vm.Answer);
        }

        // Keep Answers in sync if Question is replaced
        partial void OnQuestionChanged(Question value)
        {
            if (value.Answers == null)
                value.Answers = new List<Answer>();

            Answers = new ObservableCollection<AnswerEditorViewModel>(
                value.Answers.Select(a =>
                {
                    var vm = new AnswerEditorViewModel(a);
                    vm.RemoveRequested = RemoveAnswer;
                    return vm;
                }));
        }
    }
}
