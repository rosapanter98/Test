using CertPrep.Features.ExamCatalog.Importing;
using CertPrep.Shared;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CertPrep.Features.Options;

public partial class OptionsViewModel(
    AppearanceService appearanceService,
    QuestionBankImportService questionBankImportService) : ViewModelBase
{
    public IReadOnlyList<AppTheme> ThemeOptions { get; } = Enum.GetValues<AppTheme>();

    [ObservableProperty]
    private AppTheme selectedTheme = appearanceService.CurrentTheme;

    [ObservableProperty]
    private string? questionBankStatus;

    partial void OnSelectedThemeChanged(AppTheme value) => appearanceService.SetTheme(value);

    public async Task ImportQuestionBankAsync(string path)
    {
        try
        {
            QuestionBankStatus = "Validating question bank…";
            var result = await questionBankImportService.ImportAsync(path);
            QuestionBankStatus = result.Summary;
        }
        catch (Exception exception) when (exception is
            QuestionBankValidationException or
            FileNotFoundException or
            IOException or
            Microsoft.Data.Sqlite.SqliteException)
        {
            QuestionBankStatus = $"Import failed: {exception.Message}";
        }
    }

    public async Task SaveAuthoringKitAsync(string path)
    {
        try
        {
            await questionBankImportService.SaveAuthoringKitAsync(path);
            QuestionBankStatus = $"Authoring kit saved to {Path.GetFileName(path)}.";
        }
        catch (Exception exception) when (exception is
            IOException or
            UnauthorizedAccessException or
            InvalidOperationException)
        {
            QuestionBankStatus = $"Could not save authoring kit: {exception.Message}";
        }
    }
}
