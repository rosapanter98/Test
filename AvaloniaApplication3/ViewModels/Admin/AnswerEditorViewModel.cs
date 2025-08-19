using System;
using AvaloniaApplication3.Models;
using AvaloniaApplication3.Utility;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaApplication3.ViewModels.Admin
{
    public partial class AnswerEditorViewModel : ViewModelBase
    {
        // Model
        [ObservableProperty]
        private Answer answer;

        // Bindable proxy properties (toolkit notifies automatically)
        [ObservableProperty]
        private string text = string.Empty;

        [ObservableProperty]
        private bool isCorrect;

        // Callback for parent to remove this VM
        public Action<AnswerEditorViewModel>? RemoveRequested { get; set; }

        public AnswerEditorViewModel(Answer answer)
        {
            Answer = answer ?? throw new ArgumentNullException(nameof(answer));
            // Initialize proxies from model
            text = Answer.Text;
            isCorrect = Answer.IsCorrect;
        }

        // Keep proxies -> model
        partial void OnTextChanged(string value)
        {
            if (Answer.Text != value)
                Answer.Text = value;
        }

        partial void OnIsCorrectChanged(bool value)
        {
            if (Answer.IsCorrect != value)
                Answer.IsCorrect = value;
        }

        // If the model instance is swapped, resync proxies
        partial void OnAnswerChanged(Answer value)
        {
            Text = value?.Text ?? string.Empty;
            IsCorrect = value?.IsCorrect ?? false;
        }

        // Command (generated: RemoveSelfCommand)
        [RelayCommand]
        private void RemoveSelf() => RemoveRequested?.Invoke(this);
    }
}
