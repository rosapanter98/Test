using System.Collections.ObjectModel;
using CertPrep.Features.ExamCatalog;
using CertPrep.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CertPrep.Features.Practice;

public partial class PracticeViewModel : ViewModelBase
{
    private readonly PracticeRun _run;
    private readonly PracticeSessionService _service;
    private readonly Func<PracticeResult, Task> _completed;
    private readonly Func<int, Task> _exit;
    private int _questionIndex;
    private bool _changingExclusiveChoice;
    private bool _loadingQuestion;
    private Task _pendingDraftSave = Task.CompletedTask;

    public PracticeViewModel(
        PracticeRun run,
        PracticeSessionService service,
        Func<PracticeResult, Task> completed,
        Func<int, Task> exit)
    {
        _run = run;
        _service = service;
        _completed = completed;
        _exit = exit;
        _questionIndex = Math.Clamp(run.CurrentQuestionIndex, 0, run.Questions.Count - 1);
        LoadQuestion();
    }

    public ObservableCollection<ChoiceOptionViewModel> Choices { get; } = [];
    public string ExamCode => _run.ExamCode;
    public string SubmitButtonText => _run.Mode == PracticeMode.Study ? "Check answer" : "Submit answer";
    public string QuestionPosition => $"Question {_questionIndex + 1} of {_run.Questions.Count}";
    public double ProgressValue => (_questionIndex + 1) * 100d / _run.Questions.Count;
    public string NextButtonText => _questionIndex == _run.Questions.Count - 1 ? "Finish session" : "Next question";
    public bool CanSubmit =>
        !IsBusy &&
        !ShowFeedback &&
        Choices.Count(choice => choice.IsSelected) == _run.Questions[_questionIndex].RequiredAnswerCount;
    public bool CanContinue => !IsBusy && ShowFeedback;

    public Task FlushDraftAsync() => _pendingDraftSave;

    [ObservableProperty]
    private string objectiveName = string.Empty;

    [ObservableProperty]
    private string questionText = string.Empty;

    [ObservableProperty]
    private string answerInstruction = string.Empty;

    [ObservableProperty]
    private bool showFeedback;

    [ObservableProperty]
    private bool feedbackIsCorrect;

    [ObservableProperty]
    private string feedbackHeading = string.Empty;

    [ObservableProperty]
    private string explanation = string.Empty;

    [ObservableProperty]
    private string sourceName = string.Empty;

    [ObservableProperty]
    private string sourceUrl = string.Empty;

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private async Task SubmitAsync()
    {
        await RunBusyAsync(async () =>
        {
            var question = _run.Questions[_questionIndex];
            var selectedIds = Choices.Where(choice => choice.IsSelected).Select(choice => choice.Id).ToList();
            await _pendingDraftSave;
            var feedback = await _service.SubmitAsync(question.SessionItemId, selectedIds);

            if (_run.Mode == PracticeMode.ExamSimulation)
            {
                await AdvanceOrCompleteAsync();
                return;
            }

            foreach (var choice in Choices)
            {
                choice.RevealFeedback(feedback.CorrectChoiceIds.Contains(choice.Id));
            }

            FeedbackIsCorrect = feedback.IsCorrect;
            FeedbackHeading = feedback.IsCorrect ? "Correct" : "Incorrect";
            Explanation = feedback.Explanation;
            SourceName = feedback.SourceName;
            SourceUrl = feedback.SourceUrl;
            ShowFeedback = true;
        });

        NotifyCommandState();
    }

    [RelayCommand(CanExecute = nameof(CanContinue))]
    private async Task NextAsync()
    {
        await RunBusyAsync(AdvanceOrCompleteAsync);
        NotifyCommandState();
    }

    [RelayCommand]
    private async Task ExitAsync() =>
        await RunBusyAsync(async () =>
        {
            await _pendingDraftSave;
            await _exit(_run.SessionId);
        });

    private async Task AdvanceOrCompleteAsync()
    {
        if (_questionIndex == _run.Questions.Count - 1)
        {
            var result = await _service.CompleteAsync(_run.SessionId);
            await _completed(result);
            return;
        }

        _questionIndex++;
        LoadQuestion();
    }

    private void LoadQuestion()
    {
        _loadingQuestion = true;
        var question = _run.Questions[_questionIndex];
        ObjectiveName = _run.Scope == PracticeSessionScope.MixedExam
            ? $"{question.ExamCode}  •  {question.ObjectiveName}"
            : question.ObjectiveName;
        QuestionText = question.Prompt;
        AnswerInstruction = question.Kind switch
        {
            QuestionKind.MultipleChoice => $"Select exactly {question.RequiredAnswerCount} answers.",
            QuestionKind.TrueFalse => "Choose whether the statement is true or false.",
            _ => "Select exactly 1 answer."
        };
        ShowFeedback = false;
        FeedbackHeading = string.Empty;
        Explanation = string.Empty;
        SourceName = string.Empty;
        SourceUrl = string.Empty;
        ErrorMessage = null;

        Choices.Clear();
        foreach (var choice in question.Choices)
        {
            Choices.Add(new ChoiceOptionViewModel(
                choice.SessionChoiceId,
                choice.Text,
                choice.IsSelected,
                question.Kind is QuestionKind.SingleChoice or QuestionKind.TrueFalse,
                OnChoiceChanged));
        }
        _loadingQuestion = false;

        OnPropertyChanged(nameof(QuestionPosition));
        OnPropertyChanged(nameof(ProgressValue));
        OnPropertyChanged(nameof(NextButtonText));
        NotifyCommandState();
    }

    private void OnChoiceChanged(ChoiceOptionViewModel changed)
    {
        if (_changingExclusiveChoice || _loadingQuestion)
        {
            return;
        }

        var question = _run.Questions[_questionIndex];
        if (question.Kind is (QuestionKind.SingleChoice or QuestionKind.TrueFalse) && changed.IsSelected)
        {
            _changingExclusiveChoice = true;
            foreach (var choice in Choices.Where(choice => !ReferenceEquals(choice, changed)))
            {
                choice.IsSelected = false;
            }
            _changingExclusiveChoice = false;
        }

        NotifyCommandState();
        var selectedIds = Choices
            .Where(choice => choice.IsSelected)
            .Select(choice => choice.Id)
            .ToList();
        _pendingDraftSave = PersistDraftAsync(
            _pendingDraftSave,
            question.SessionItemId,
            selectedIds);
    }

    private async Task PersistDraftAsync(
        Task previousSave,
        int sessionItemId,
        IReadOnlyCollection<int> selectedChoiceIds)
    {
        try
        {
            await previousSave;
            await _service.SaveDraftSelectionAsync(sessionItemId, selectedChoiceIds);
        }
        catch (Exception exception)
        {
            ErrorMessage = $"The draft answer could not be saved: {exception.Message}";
        }
    }

    private void NotifyCommandState()
    {
        OnPropertyChanged(nameof(CanSubmit));
        OnPropertyChanged(nameof(CanContinue));
        SubmitCommand.NotifyCanExecuteChanged();
        NextCommand.NotifyCanExecuteChanged();
    }
}

public partial class ChoiceOptionViewModel(
    int id,
    string text,
    bool selected,
    bool usesRadioIndicator,
    Action<ChoiceOptionViewModel> changed) : ObservableObject
{
    public int Id { get; } = id;
    public string Text { get; } = text;
    public bool UsesRadioIndicator { get; } = usesRadioIndicator;
    public bool UsesCheckboxIndicator => !UsesRadioIndicator;

    [ObservableProperty]
    private bool isSelected = selected;

    [ObservableProperty]
    private bool canChangeSelection = true;

    [ObservableProperty]
    private bool showCorrectIndicator;

    [ObservableProperty]
    private bool showIncorrectIndicator;

    partial void OnIsSelectedChanged(bool value) => changed(this);

    public void RevealFeedback(bool isCorrectAnswer)
    {
        CanChangeSelection = false;
        ShowCorrectIndicator = isCorrectAnswer;
        ShowIncorrectIndicator = IsSelected && !isCorrectAnswer;
    }
}
