using AvaloniaApplication3.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace AvaloniaApplication3.ViewModels
{
    public partial class AnswerOptionViewModel : ViewModelBase
    {
        public AnswerOptionViewModel(Answer model)
        {
             Model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public Answer Model { get; }
        public int Id => Model.Id;
        public string Text => Model.Text;
        public bool IsCorrect => Model.IsCorrect;

        [ObservableProperty]
        private bool _isSelected;

        [ObservableProperty]
        private bool? isUserCorrect;

    }
}
