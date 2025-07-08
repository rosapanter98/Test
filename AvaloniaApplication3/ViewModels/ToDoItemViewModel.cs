using AvaloniaApplication3.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace AvaloniaApplication3.ViewModels
{
    public partial class ToDoItemViewModel : ViewModelBase
    {
        [ObservableProperty]
        private bool _isChecked;

        partial void OnIsCheckedChanged(bool value)
        {
            ConfirmDoneCommand.NotifyCanExecuteChanged();
        }

        [ObservableProperty]
        private string? _content;

        public Action? GoBack { get; set; }

        public Action<ToDoItemViewModel>? OnConfirmDone { get; set; }

        public IRelayCommand? OpenItemCommand { get; set; }
        public IRelayCommand? RemoveItemCommand { get; set; }

        [RelayCommand]
        public void GoBackCommand()
        {
            GoBack?.Invoke();
        }

        [RelayCommand(CanExecute = nameof(CanConfirmDone))]
        public void ConfirmDone()
        {
            OnConfirmDone?.Invoke(this);
        }

        private bool CanConfirmDone() => IsChecked;
    }
}
