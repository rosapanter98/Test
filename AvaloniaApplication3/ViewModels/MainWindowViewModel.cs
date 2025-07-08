using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaApplication3.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private object? currentView;

        private readonly ToDoListViewModel _toDoList;

        public MainWindowViewModel()
        {
            _toDoList = new ToDoListViewModel(SwitchToItemView); // only once
            GoHome(); // Start on landing page
        }

        [RelayCommand]
        private void GoHome()
        {
            CurrentView = new MainPageViewModel();
        }

        [RelayCommand]
        private void ShowToDoList()
        {
            CurrentView = _toDoList; // reuse existing instance
        }

        private void SwitchToItemView(ToDoItemViewModel item)
        {
            _toDoList.PrepareItem(item, view => CurrentView = view);
            CurrentView = item;
        }
    }
}
