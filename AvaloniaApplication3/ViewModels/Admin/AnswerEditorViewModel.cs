using AvaloniaApplication3.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace AvaloniaApplication3.ViewModels.Admin
{
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;

    public partial class AnswerEditorViewModel : ViewModelBase
    {
        // When Answer changes, also raise PC for Text and IsCorrect
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Text))]
        [NotifyPropertyChangedFor(nameof(IsCorrect))]
        private Answer answer;

        public Action<AnswerEditorViewModel>? RemoveRequested { get; set; }

        public AnswerEditorViewModel(Answer answer)
        {
            this.answer = answer;
        }

        // Generated IRelayCommand RemoveSelfCommand
        [RelayCommand]
        private void RemoveSelf() => RemoveRequested?.Invoke(this);

        // Proxy properties to a non-notifying model
        public string Text
        {
            get => Answer.Text;
            set
            {
                if (Answer.Text == value) return;
                Answer.Text = value;
                OnPropertyChanged(); // nameof(Text)
            }
        }

        public bool IsCorrect
        {
            get => Answer.IsCorrect;
            set
            {
                if (Answer.IsCorrect == value) return;
                Answer.IsCorrect = value;
                OnPropertyChanged(); // nameof(IsCorrect)
            }
        }
    }

}
