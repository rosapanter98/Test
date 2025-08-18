using AvaloniaApplication3.Models;
using AvaloniaApplication3.Services;
using AvaloniaApplication3.ViewModels.History;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace AvaloniaApplication3.ViewModels
{
    public partial class AttemptsHistoryViewModel : ViewModelBase
    {
        private readonly IQuizAttemptService _attempts;
        private int _userId;

        // Rows for the view
        [ObservableProperty] private ObservableCollection<AttemptRowViewModel> visibleItems = new();
        [ObservableProperty] private bool isLoading;
        [ObservableProperty] private string? filterText;
        [ObservableProperty] private bool onlyCompleted;
        [ObservableProperty] private string sortBy = "DateDesc";

        public AttemptsHistoryViewModel(IQuizAttemptService attempts) => _attempts = attempts;

        public async Task LoadAsync(int userId)
        {
            _userId = userId;
            await RefreshAsync();
        }

        [RelayCommand]
        public async Task RefreshAsync()
        {
            if (_userId <= 0) return;
            IsLoading = true;
            try
            {
                var list = await _attempts.GetAttemptsByUserAsync(_userId);

                var rows = list.Select(a => new AttemptRowViewModel(a));
                rows = ApplyFilterAndSort(rows);

                VisibleItems = new ObservableCollection<AttemptRowViewModel>(rows);
            }
            finally { IsLoading = false; }
        }

        // AttemptsHistoryViewModel
        [RelayCommand]
        public async Task LoadAttemptDetailsAsync(AttemptRowViewModel? row)
        {
            if (row is null || row.HasDetailsLoaded) return;
            var detailed = await _attempts.GetAttemptAsync(row.AttemptId, includeItems: true);
            if (detailed is null) return;

            row.Items.Clear();
            foreach (var item in detailed.Items)
                row.Items.Add(new AttemptItemRowViewModel(item)); // Observable -> UI updates
        }

        [RelayCommand]
        public async Task DeleteAttemptAsync(AttemptRowViewModel? row)
        {
            if (row is null) return;
            await _attempts.DeleteAttemptAsync(row.AttemptId);
            // simple: refresh list; or remove from VisibleItems directly
            await RefreshAsync();
        }


        partial void OnFilterTextChanged(string? _) => ReapplyFilterAndSort();
        partial void OnOnlyCompletedChanged(bool _) => ReapplyFilterAndSort();
        partial void OnSortByChanged(string _) => ReapplyFilterAndSort();

        private void ReapplyFilterAndSort()
        {
            var rows = ApplyFilterAndSort(VisibleItems);
            // preserve existing row instances when possible
            VisibleItems = new ObservableCollection<AttemptRowViewModel>(rows);
        }

        private IEnumerable<AttemptRowViewModel> ApplyFilterAndSort(IEnumerable<AttemptRowViewModel> rows)
        {
            var q = rows;

            if (OnlyCompleted) q = q.Where(r => r.CompletedAt != null);

            var ft = FilterText?.Trim();
            if (!string.IsNullOrEmpty(ft))
                q = q.Where(r =>
                    r.QuizTitle.Contains(ft, StringComparison.OrdinalIgnoreCase) ||
                    r.StartedAt.ToString("u").Contains(ft, StringComparison.OrdinalIgnoreCase));

            q = SortBy switch
            {
                "DateAsc" => q.OrderBy(r => r.CompletedAt ?? r.StartedAt),
                "ScoreDesc" => q.OrderByDescending(r => r.CorrectAnswers).ThenByDescending(r => r.CompletedAt ?? r.StartedAt),
                "ScoreAsc" => q.OrderBy(r => r.CorrectAnswers).ThenBy(r => r.CompletedAt ?? r.StartedAt),
                _ => q.OrderByDescending(r => r.CompletedAt ?? r.StartedAt),
            };

            return q;
        }
    }
}
