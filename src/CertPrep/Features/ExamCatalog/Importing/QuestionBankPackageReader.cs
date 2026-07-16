using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Data.Sqlite;

namespace CertPrep.Features.ExamCatalog.Importing;

public sealed class QuestionBankPackageReader
{
    private const string EmbeddedResourceName = "CertPrep.Content.default-question-bank.json";
    private const string EmbeddedExamResourcePrefix = "CertPrep.Content.Banks.";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<QuestionBankPackage> ReadEmbeddedAsync(
        CancellationToken cancellationToken = default)
    {
        var assembly = typeof(QuestionBankPackageReader).Assembly;
        await using var stream = assembly
            .GetManifestResourceStream(EmbeddedResourceName)
            ?? throw new InvalidOperationException($"Embedded question bank '{EmbeddedResourceName}' was not found.");

        var package = await JsonSerializer.DeserializeAsync<QuestionBankPackage>(stream, JsonOptions, cancellationToken)
            ?? throw new QuestionBankValidationException("The embedded question bank is empty.");

        var examResourceNames = assembly.GetManifestResourceNames()
            .Where(name => name.StartsWith(EmbeddedExamResourcePrefix, StringComparison.Ordinal) &&
                           name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToList();

        foreach (var resourceName in examResourceNames)
        {
            await using var examStream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"Embedded exam bank '{resourceName}' was not found.");
            var exam = await JsonSerializer.DeserializeAsync<QuestionBankExam>(
                examStream,
                JsonOptions,
                cancellationToken)
                ?? throw new QuestionBankValidationException($"Embedded exam bank '{resourceName}' is empty.");
            package.Exams.Add(exam);
        }

        return package;
    }

    public async Task<QuestionBankPackage> ReadSqliteAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("A SQLite file path is required.", nameof(path));
        }

        var fullPath = Path.GetFullPath(path);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("The SQLite question bank was not found.", fullPath);
        }

        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = fullPath,
            Mode = SqliteOpenMode.ReadOnly,
            Pooling = false
        }.ToString();

        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await ValidateSchemaAsync(connection, cancellationToken);

        var package = new QuestionBankPackage { SchemaVersion = 1 };
        var examsById = new Dictionary<long, QuestionBankExam>();
        var objectivesById = new Dictionary<long, (long ExamId, QuestionBankObjective Objective)>();
        var questionsById = new Dictionary<long, QuestionBankQuestion>();

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = """
                SELECT Id, Provider, Code, Title, Summary, ContentVersion
                FROM Exams
                WHERE IsArchived = 0
                ORDER BY Provider, Code;
                """;
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var exam = new QuestionBankExam
                {
                    Provider = reader.GetString(1),
                    Code = reader.GetString(2),
                    Title = reader.GetString(3),
                    Summary = reader.GetString(4),
                    ContentVersion = reader.GetString(5)
                };
                examsById.Add(reader.GetInt64(0), exam);
                package.Exams.Add(exam);
            }
        }

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = """
                SELECT Id, ExamId, ContentKey, Name, SortOrder
                FROM ExamObjectives
                ORDER BY ExamId, SortOrder;
                """;
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var examId = reader.GetInt64(1);
                if (!examsById.TryGetValue(examId, out var exam))
                {
                    continue;
                }

                var objective = new QuestionBankObjective
                {
                    ContentKey = reader.GetString(2),
                    Name = reader.GetString(3),
                    SortOrder = reader.GetInt32(4)
                };
                exam.Objectives.Add(objective);
                objectivesById.Add(reader.GetInt64(0), (examId, objective));
            }
        }

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = """
                SELECT Id, ExamId, ExamObjectiveId, ContentKey, Prompt, Kind, Difficulty,
                       Explanation, SourceName, SourceUrl, IsActive
                FROM Questions
                ORDER BY ExamId, Id;
                """;
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var examId = reader.GetInt64(1);
                var objectiveId = reader.GetInt64(2);
                if (!examsById.TryGetValue(examId, out var exam) ||
                    !objectivesById.TryGetValue(objectiveId, out var objective) ||
                    objective.ExamId != examId)
                {
                    throw new QuestionBankValidationException("A source question refers to a missing exam or objective.");
                }

                if (!Enum.TryParse<QuestionKind>(reader.GetString(5), true, out var kind) ||
                    !Enum.TryParse<QuestionDifficulty>(reader.GetString(6), true, out var difficulty))
                {
                    throw new QuestionBankValidationException("A source question has an unsupported kind or difficulty.");
                }

                var question = new QuestionBankQuestion
                {
                    ContentKey = reader.GetString(3),
                    ObjectiveKey = objective.Objective.ContentKey,
                    Prompt = reader.GetString(4),
                    Kind = kind,
                    Difficulty = difficulty,
                    Explanation = reader.GetString(7),
                    SourceName = reader.GetString(8),
                    SourceUrl = reader.GetString(9),
                    IsActive = reader.GetBoolean(10)
                };
                exam.Questions.Add(question);
                questionsById.Add(reader.GetInt64(0), question);
            }
        }

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = """
                SELECT QuestionId, Text, IsCorrect, SortOrder
                FROM AnswerChoices
                ORDER BY QuestionId, SortOrder;
                """;
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                if (questionsById.TryGetValue(reader.GetInt64(0), out var question))
                {
                    question.Choices.Add(new QuestionBankChoice
                    {
                        Text = reader.GetString(1),
                        IsCorrect = reader.GetBoolean(2),
                        SortOrder = reader.GetInt32(3)
                    });
                }
            }
        }

        return package;
    }

    private static async Task ValidateSchemaAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        var requiredSchema = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["Exams"] = ["Id", "Provider", "Code", "Title", "Summary", "ContentVersion", "IsArchived"],
            ["ExamObjectives"] = ["Id", "ExamId", "ContentKey", "Name", "SortOrder"],
            ["Questions"] = ["Id", "ExamId", "ExamObjectiveId", "ContentKey", "Prompt", "Kind", "Difficulty", "Explanation", "SourceName", "SourceUrl", "IsActive"],
            ["AnswerChoices"] = ["QuestionId", "Text", "IsCorrect", "SortOrder"]
        };

        foreach (var (table, requiredColumns) in requiredSchema)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = $"PRAGMA table_info('{table}');";
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            while (await reader.ReadAsync(cancellationToken))
            {
                columns.Add(reader.GetString(1));
            }

            var missing = requiredColumns.Where(column => !columns.Contains(column)).ToList();
            if (missing.Count > 0)
            {
                throw new QuestionBankValidationException(
                    $"'{Path.GetFileName(connection.DataSource)}' is not a compatible CertPrep question bank. " +
                    $"Table {table} is missing: {string.Join(", ", missing)}.");
            }
        }
    }
}
