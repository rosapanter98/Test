using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AvaloniaApplication3.Models;
using AvaloniaApplication3.ViewModels;

namespace AvaloniaApplication3.Services
{
    /// <summary>Stateless helpers for quiz evaluation and VM mapping.</summary>
    public static class QuizLogic
    {
        public static ObservableCollection<AnswerOptionViewModel> BuildAnswerVMs(Question question)
        {
            var list = question.Answers
                               .OrderBy(a => a.Id)
                               .Select(a => new AnswerOptionViewModel(a) { IsSelected = false, IsUserCorrect = null });
            return new ObservableCollection<AnswerOptionViewModel>(list);
        }

        public static (bool isCorrect, HashSet<int> selectedIds, HashSet<int> correctIds)
            Evaluate(IReadOnlyCollection<AnswerOptionViewModel> answers)
        {
            var selectedIds = answers.Where(a => a.IsSelected).Select(a => a.Id).ToHashSet();
            var correctIds = answers.Where(a => a.IsCorrect).Select(a => a.Id).ToHashSet();
            var isCorrect = selectedIds.SetEquals(correctIds);
            return (isCorrect, selectedIds, correctIds);
        }

        public static void ApplyPerAnswerFeedback(IReadOnlyCollection<AnswerOptionViewModel> answers)
        {
            foreach (var vm in answers)
                vm.IsUserCorrect = vm.IsSelected ? vm.IsCorrect : (bool?)null;
        }

        public static string BuildFeedbackText(Question question, bool isCorrect)
        {
            if (isCorrect) return "Correct!";
            var correct = question.Answers.Where(a => a.IsCorrect).Select(a => a.Text);
            return $"Wrong. Correct answer(s): {string.Join(", ", correct)}";
        }

        /// <summary>For SingleChoice/TrueFalse: ensure only one answer stays selected.</summary>
        public static void EnforceSingleChoice(ObservableCollection<AnswerOptionViewModel> answers,
                                               AnswerOptionViewModel changed)
        {
            if (!changed.IsSelected) return;
            foreach (var vm in answers)
            {
                if (!ReferenceEquals(vm, changed) && vm.IsSelected)
                    vm.IsSelected = false;
            }
        }
    }
}
