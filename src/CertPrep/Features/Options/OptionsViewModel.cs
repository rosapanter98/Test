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
    private string? importStatus;

    partial void OnSelectedThemeChanged(AppTheme value) => appearanceService.SetTheme(value);

    public async Task ImportQuestionBankAsync(string path)
    {
        try
        {
            ImportStatus = "Validating question bank…";
            var result = await questionBankImportService.ImportSqliteAsync(path);
            ImportStatus = result.Summary;
        }
        catch (Exception exception) when (exception is
            QuestionBankValidationException or
            FileNotFoundException or
            IOException or
            Microsoft.Data.Sqlite.SqliteException)
        {
            ImportStatus = $"Import failed: {exception.Message}";
        }
    }
}
