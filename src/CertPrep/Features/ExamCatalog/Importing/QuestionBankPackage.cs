using System.Text.Json.Serialization;

namespace CertPrep.Features.ExamCatalog.Importing;

public sealed class QuestionBankPackage
{
    [JsonPropertyName("$schema")]
    public string? Schema { get; init; }

    public int SchemaVersion { get; init; }
    public List<QuestionBankExam> Exams { get; init; } = [];
}

public sealed class QuestionBankExam
{
    public string Provider { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string ContentVersion { get; init; } = string.Empty;
    public List<QuestionBankObjective> Objectives { get; init; } = [];
    public List<QuestionBankQuestion> Questions { get; init; } = [];
}

public sealed class QuestionBankObjective
{
    public string ContentKey { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int SortOrder { get; init; }
}

public sealed class QuestionBankQuestion
{
    public string ContentKey { get; init; } = string.Empty;
    public string ObjectiveKey { get; init; } = string.Empty;
    public string Prompt { get; init; } = string.Empty;
    public QuestionKind Kind { get; init; }
    public QuestionDifficulty Difficulty { get; init; }
    public string Explanation { get; init; } = string.Empty;
    public string SourceName { get; init; } = string.Empty;
    public string SourceUrl { get; init; } = string.Empty;
    public bool IsActive { get; init; } = true;
    public List<QuestionBankChoice> Choices { get; init; } = [];
}

public sealed class QuestionBankChoice
{
    public string Text { get; init; } = string.Empty;
    public bool IsCorrect { get; init; }
    public int SortOrder { get; init; }
}

public sealed record QuestionBankMergeResult(
    int ExamsAdded,
    int ExamsUpdated,
    int QuestionsAdded,
    int QuestionsUpdated)
{
    public string Summary =>
        $"Catalog merged: {ExamsAdded} exams added, {ExamsUpdated} updated; " +
        $"{QuestionsAdded} questions added, {QuestionsUpdated} updated.";
}

public sealed class QuestionBankValidationException(string message) : Exception(message);
