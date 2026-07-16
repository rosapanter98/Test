using System.Collections.ObjectModel;
using CertPrep.Features.ExamCatalog;
using CertPrep.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CertPrep.Features.Practice;

public partial class MixedPracticeSetupViewModel : ViewModelBase
{
    private readonly Func<IReadOnlyList<int>, PracticeMode, int, Task> _start;
    private readonly Func<Task> _back;

    public MixedPracticeSetupViewModel(
        IReadOnlyList<ExamSummary> exams,
        Func<IReadOnlyList<int>, PracticeMode, int, Task> start,
        Func<Task> back)
    {
        _start = start;
        _back = back;
        ModeOptions = PracticeSetupOptions.BuildModes();
        SelectedMode = ModeOptions[0];

        for (var index = 0; index < exams.Count; index++)
        {
            Exams.Add(new ExamSelectionViewModel(exams[index], index < 2, RefreshSelection));
        }

        RefreshSelection();
    }

    public ObservableCollection<ExamSelectionViewModel> Exams { get; } = [];
    public ObservableCollection<QuestionCountOption> QuestionCountOptions { get; } = [];
    public IReadOnlyList<PracticeModeOption> ModeOptions { get; }
    public bool CanStart => SelectedExamCount >= 2 && SelectedQuestionCount is not null;
    public string SelectionSummary => SelectedExamCount < 2
        ? "Select at least two exams."
        : $"{SelectedExamCount} exams • {AvailableQuestionCount} available questions";

    [ObservableProperty]
    private QuestionCountOption selectedQuestionCount = null!;

    [ObservableProperty]
    private PracticeModeOption selectedMode;

    [ObservableProperty]
    private int selectedExamCount;

    [ObservableProperty]
    private int availableQuestionCount;

    [RelayCommand(CanExecute = nameof(CanStart))]
    private async Task StartAsync()
    {
        var selectedIds = Exams.Where(exam => exam.IsSelected).Select(exam => exam.Id).ToList();
        await RunBusyAsync(() => _start(selectedIds, SelectedMode.Mode, SelectedQuestionCount.Count));
    }

    [RelayCommand]
    private async Task BackAsync() => await _back();

    private void RefreshSelection()
    {
        var previousCount = SelectedQuestionCount?.Count;
        SelectedExamCount = Exams.Count(exam => exam.IsSelected);
        AvailableQuestionCount = Exams.Where(exam => exam.IsSelected).Sum(exam => exam.QuestionCount);

        var options = PracticeSetupOptions.BuildQuestionCounts(AvailableQuestionCount, SelectedExamCount);
        QuestionCountOptions.Clear();
        foreach (var option in options)
        {
            QuestionCountOptions.Add(option);
        }

        SelectedQuestionCount = options.FirstOrDefault(option => option.Count == previousCount)
            ?? options.FirstOrDefault()
            ?? null!;
        OnPropertyChanged(nameof(SelectionSummary));
        OnPropertyChanged(nameof(CanStart));
        StartCommand.NotifyCanExecuteChanged();
    }
}

public partial class ExamSelectionViewModel : ObservableObject
{
    private readonly Action _changed;

    public ExamSelectionViewModel(ExamSummary exam, bool isSelected, Action changed)
    {
        _changed = changed;
        Id = exam.Id;
        Code = exam.Code;
        Title = exam.Title;
        QuestionCount = exam.QuestionCount;
        this.isSelected = isSelected;
    }

    public int Id { get; }
    public string Code { get; }
    public string Title { get; }
    public int QuestionCount { get; }
    public string QuestionCountText => $"{QuestionCount} questions";

    [ObservableProperty]
    private bool isSelected;

    partial void OnIsSelectedChanged(bool value) => _changed();
}
