using CertPrep.Features.ExamCatalog;
using CertPrep.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CertPrep.Features.Practice;

public partial class PracticeSetupViewModel : ViewModelBase
{
    private readonly ExamSummary _exam;
    private readonly Func<int, PracticeMode, int, Task> _start;
    private readonly Func<Task> _back;

    public PracticeSetupViewModel(
        ExamSummary exam,
        Func<int, PracticeMode, int, Task> start,
        Func<Task> back)
    {
        _exam = exam;
        _start = start;
        _back = back;

        QuestionCountOptions = PracticeSetupOptions.BuildQuestionCounts(exam.QuestionCount);
        SelectedQuestionCount = QuestionCountOptions[0];
        ModeOptions = PracticeSetupOptions.BuildModes();
        SelectedMode = ModeOptions[0];
    }

    public string ExamCode => _exam.Code;
    public string ExamTitle => _exam.Title;
    public IReadOnlyList<string> Objectives => _exam.ObjectiveNames;
    public IReadOnlyList<QuestionCountOption> QuestionCountOptions { get; }
    public IReadOnlyList<PracticeModeOption> ModeOptions { get; }

    [ObservableProperty]
    private QuestionCountOption selectedQuestionCount;

    [ObservableProperty]
    private PracticeModeOption selectedMode;

    [RelayCommand]
    private async Task StartAsync() =>
        await RunBusyAsync(() => _start(_exam.Id, SelectedMode.Mode, SelectedQuestionCount.Count));

    [RelayCommand]
    private async Task BackAsync() => await _back();

}
