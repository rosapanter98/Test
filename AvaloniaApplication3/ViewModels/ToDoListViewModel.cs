using AvaloniaApplication3.Models;
using AvaloniaApplication3.Repositories;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace AvaloniaApplication3.ViewModels
{
    public partial class ToDoListViewModel : ViewModelBase
    {
        public ObservableCollection<ToDoItemViewModel> ToDoItems { get; } = new();

        private readonly Action<ToDoItemViewModel> _navigateToItem;
        private readonly IToDoRepository _repository;
        private readonly string _username;

        public ToDoListViewModel(string username, Action<ToDoItemViewModel> navigateToItem, IToDoRepository repository)
        {
            _username = username;
            _navigateToItem = navigateToItem;
            _repository = repository;

            List<ToDoItem> savedItems = _repository.LoadItems(_username);
            foreach (ToDoItem model in savedItems)
            {
                var itemVM = new ToDoItemViewModel
                {
                    Content = model.Content,
                    IsChecked = model.IsChecked,
                    OpenItemCommand = OpenItemCommand,
                    RemoveItemCommand = RemoveItemCommand
                };

                itemVM.PropertyChanged += OnItemPropertyChanged;
                ToDoItems.Add(itemVM);
            }
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
                IsChecked = false,
                OpenItemCommand = OpenItemCommand,
                RemoveItemCommand = RemoveItemCommand
            };

            ToDoItems.Add(item);
            NewItemContent = string.Empty;

            item.PropertyChanged += OnItemPropertyChanged;
            SaveAllItems();
        }

        [RelayCommand]
        public void RemoveItem(ToDoItemViewModel item)
        {
            if (ToDoItems.Contains(item))
            {
                item.PropertyChanged -= OnItemPropertyChanged;
                ToDoItems.Remove(item);
                SaveAllItems();
            }
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
            item.OnConfirmDone = doneItem =>
            {
                RemoveItem(doneItem);
                setCurrentView(this);
            };
        }

        private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ToDoItemViewModel.IsChecked))
            {
                SaveAllItems();
            }
        }

        private void SaveAllItems()
        {
            var itemsToSave = ToDoItems.Select(vm => new ToDoItem
            {
                Content = vm.Content,
                IsChecked = vm.IsChecked
            }).ToList();

            _repository.SaveItems(_username, itemsToSave);
        }
    }
}