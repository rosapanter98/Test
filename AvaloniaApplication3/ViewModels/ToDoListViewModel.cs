using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System;

namespace AvaloniaApplication3.ViewModels
{
    public partial class ToDoListViewModel : ViewModelBase
    {
        public ObservableCollection<ToDoItemViewModel> ToDoItems { get; } = new();

        private readonly Action<ToDoItemViewModel> _navigateToItem;

        public ToDoListViewModel(Action<ToDoItemViewModel> navigateToItem)
        {
            _navigateToItem = navigateToItem;
        }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddItemCommand))]
        private string? newItemContent;

        private bool CanAddItem() => !string.IsNullOrWhiteSpace(NewItemContent);

        [RelayCommand(CanExecute = nameof(CanAddItem))]
        private void AddItem()
        {
            var item = new ToDoItemViewModel
            {
                Content = NewItemContent,
                OpenItemCommand = OpenItemCommand,
                RemoveItemCommand = RemoveItemCommand
            };

            ToDoItems.Add(item);
            NewItemContent = string.Empty;
        }

        [RelayCommand]
        public void RemoveItem(ToDoItemViewModel item)
        {
            ToDoItems.Remove(item);
        }

        [RelayCommand]
        public void OpenItem(ToDoItemViewModel item)
        {
            _navigateToItem(item);
        }

        public void PrepareItem(ToDoItemViewModel item, Action<object> setCurrentView)
        {
            item.IsChecked = false;
            item.GoBack = () => setCurrentView(this);
            item.OnConfirmDone = (doneItem) =>
            {
                ToDoItems.Remove(doneItem);
                setCurrentView(this);
            };
        }
    }
}
