using CertPrep.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CertPrep.Features.ExamCatalog.Importing;

public sealed class QuestionBankMerger
{
    public async Task<QuestionBankMergeResult> MergeAsync(
        StudyDbContext context,
        QuestionBankPackage package,
        CancellationToken cancellationToken = default)
    {
        Validate(package);

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var exams = await context.Exams
            .Include(exam => exam.Objectives)
            .Include(exam => exam.Questions)
            .ThenInclude(question => question.Choices)
            .ToListAsync(cancellationToken);

        var examsAdded = 0;
        var examsUpdated = 0;
        var questionsAdded = 0;
        var questionsUpdated = 0;

        foreach (var incomingExam in package.Exams)
        {
            var exam = exams.FirstOrDefault(existing =>
                string.Equals(existing.Provider, incomingExam.Provider, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(existing.Code, incomingExam.Code, StringComparison.OrdinalIgnoreCase));
            var examIsNew = exam is null;
            exam ??= new Exam();

            var examChanged = SetExamValues(exam, incomingExam);
            if (examIsNew)
            {
                context.Exams.Add(exam);
                exams.Add(exam);
                examsAdded++;
            }

            var objectives = new Dictionary<string, ExamObjective>(StringComparer.OrdinalIgnoreCase);
            foreach (var incomingObjective in incomingExam.Objectives.OrderBy(item => item.SortOrder))
            {
                var objective = exam.Objectives.FirstOrDefault(existing =>
                        string.Equals(existing.ContentKey, incomingObjective.ContentKey, StringComparison.OrdinalIgnoreCase))
                    ?? exam.Objectives.FirstOrDefault(existing =>
                        existing.ContentKey.StartsWith("legacy-objective-", StringComparison.Ordinal) &&
                        string.Equals(existing.Name, incomingObjective.Name, StringComparison.OrdinalIgnoreCase));

                if (objective is null)
                {
                    objective = new ExamObjective { Exam = exam };
                    exam.Objectives.Add(objective);
                    examChanged = true;
                }

                examChanged |= SetObjectiveValues(objective, incomingObjective);
                objectives.Add(incomingObjective.ContentKey, objective);
            }

            foreach (var incomingQuestion in incomingExam.Questions)
            {
                var question = exam.Questions.FirstOrDefault(existing =>
                        string.Equals(existing.ContentKey, incomingQuestion.ContentKey, StringComparison.OrdinalIgnoreCase))
                    ?? exam.Questions.FirstOrDefault(existing =>
                        existing.ContentKey.StartsWith("legacy-question-", StringComparison.Ordinal) &&
                        string.Equals(existing.Prompt, incomingQuestion.Prompt, StringComparison.Ordinal));
                var questionIsNew = question is null;
                question ??= new Question { Exam = exam };

                var questionChanged = SetQuestionValues(
                    question,
                    incomingQuestion,
                    objectives[incomingQuestion.ObjectiveKey]);
                questionChanged |= SetChoices(context, question, incomingQuestion.Choices);

                if (questionIsNew)
                {
                    exam.Questions.Add(question);
                    questionsAdded++;
                }
                else if (questionChanged)
                {
                    questionsUpdated++;
                }

                examChanged |= questionChanged;
            }

            if (!examIsNew && examChanged)
            {
                examsUpdated++;
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new QuestionBankMergeResult(examsAdded, examsUpdated, questionsAdded, questionsUpdated);
    }

    public static void Validate(QuestionBankPackage package)
    {
        Require(package.SchemaVersion == 1, $"Question bank schema {package.SchemaVersion} is not supported.");
        Require(package.Exams.Count > 0, "The question bank contains no exams.");

        var examKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var exam in package.Exams)
        {
            RequireText(exam.Provider, "Exam provider");
            RequireText(exam.Code, "Exam code");
            RequireText(exam.Title, $"{exam.Code} title");
            RequireText(exam.Summary, $"{exam.Code} summary");
            RequireText(exam.ContentVersion, $"{exam.Code} content version");
            Require(examKeys.Add($"{exam.Provider}|{exam.Code}"), $"Exam {exam.Provider} {exam.Code} is duplicated.");
            Require(exam.Objectives.Count > 0, $"Exam {exam.Code} has no objectives.");
            Require(exam.Questions.Count > 0, $"Exam {exam.Code} has no questions.");

            var objectiveKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var objectiveOrders = new HashSet<int>();
            foreach (var objective in exam.Objectives)
            {
                RequireKey(objective.ContentKey, $"{exam.Code} objective");
                RequireText(objective.Name, $"{exam.Code} objective name");
                Require(objectiveKeys.Add(objective.ContentKey), $"Objective key '{objective.ContentKey}' is duplicated in {exam.Code}.");
                Require(objectiveOrders.Add(objective.SortOrder), $"Objective sort order {objective.SortOrder} is duplicated in {exam.Code}.");
            }

            var questionKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var question in exam.Questions)
            {
                RequireKey(question.ContentKey, $"{exam.Code} question");
                Require(questionKeys.Add(question.ContentKey), $"Question key '{question.ContentKey}' is duplicated in {exam.Code}.");
                Require(objectiveKeys.Contains(question.ObjectiveKey), $"Question '{question.ContentKey}' refers to unknown objective '{question.ObjectiveKey}'.");
                RequireText(question.Prompt, $"Question '{question.ContentKey}' prompt");
                RequireText(question.Explanation, $"Question '{question.ContentKey}' explanation");
                RequireText(question.SourceName, $"Question '{question.ContentKey}' source name");
                Require(Uri.TryCreate(question.SourceUrl, UriKind.Absolute, out _), $"Question '{question.ContentKey}' has an invalid source URL.");
                Require(question.Choices.Count >= 2, $"Question '{question.ContentKey}' needs at least two choices.");
                Require(question.Choices.Any(choice => choice.IsCorrect), $"Question '{question.ContentKey}' has no correct choice.");
                Require(question.Choices.Any(choice => !choice.IsCorrect), $"Question '{question.ContentKey}' has no incorrect choice.");
                Require(question.Kind != QuestionKind.SingleChoice || question.Choices.Count(choice => choice.IsCorrect) == 1,
                    $"Single-choice question '{question.ContentKey}' must have exactly one correct choice.");
                Require(question.Kind != QuestionKind.MultipleChoice || question.Choices.Count(choice => choice.IsCorrect) >= 2,
                    $"Multiple-choice question '{question.ContentKey}' must have at least two correct choices.");

                var choiceOrders = new HashSet<int>();
                foreach (var choice in question.Choices)
                {
                    RequireText(choice.Text, $"Question '{question.ContentKey}' choice");
                    Require(choiceOrders.Add(choice.SortOrder), $"Question '{question.ContentKey}' has duplicate choice sort order {choice.SortOrder}.");
                }
            }
        }
    }

    private static bool SetExamValues(Exam target, QuestionBankExam source)
    {
        var changed = target.Provider != source.Provider || target.Code != source.Code ||
            target.Title != source.Title || target.Summary != source.Summary ||
            target.ContentVersion != source.ContentVersion || target.IsArchived;
        target.Provider = source.Provider;
        target.Code = source.Code;
        target.Title = source.Title;
        target.Summary = source.Summary;
        target.ContentVersion = source.ContentVersion;
        target.IsArchived = false;
        return changed;
    }

    private static bool SetObjectiveValues(ExamObjective target, QuestionBankObjective source)
    {
        var changed = target.ContentKey != source.ContentKey || target.Name != source.Name || target.SortOrder != source.SortOrder;
        target.ContentKey = source.ContentKey;
        target.Name = source.Name;
        target.SortOrder = source.SortOrder;
        return changed;
    }

    private static bool SetQuestionValues(
        Question target,
        QuestionBankQuestion source,
        ExamObjective objective)
    {
        var changed = target.ContentKey != source.ContentKey || target.Objective != objective ||
            target.Prompt != source.Prompt || target.Kind != source.Kind ||
            target.Difficulty != source.Difficulty || target.Explanation != source.Explanation ||
            target.SourceName != source.SourceName || target.SourceUrl != source.SourceUrl ||
            target.IsActive != source.IsActive;
        target.ContentKey = source.ContentKey;
        target.Objective = objective;
        target.Prompt = source.Prompt;
        target.Kind = source.Kind;
        target.Difficulty = source.Difficulty;
        target.Explanation = source.Explanation;
        target.SourceName = source.SourceName;
        target.SourceUrl = source.SourceUrl;
        target.IsActive = source.IsActive;
        return changed;
    }

    private static bool SetChoices(
        StudyDbContext context,
        Question question,
        IReadOnlyList<QuestionBankChoice> sourceChoices)
    {
        var current = question.Choices.OrderBy(choice => choice.SortOrder).ToList();
        var incoming = sourceChoices.OrderBy(choice => choice.SortOrder).ToList();
        var changed = current.Count != incoming.Count || current.Zip(incoming).Any(pair =>
            pair.First.Text != pair.Second.Text ||
            pair.First.IsCorrect != pair.Second.IsCorrect ||
            pair.First.SortOrder != pair.Second.SortOrder);
        if (!changed)
        {
            return false;
        }

        context.AnswerChoices.RemoveRange(question.Choices);
        question.Choices.Clear();
        question.Choices.AddRange(incoming.Select(choice => new AnswerChoice
        {
            Text = choice.Text,
            IsCorrect = choice.IsCorrect,
            SortOrder = choice.SortOrder
        }));
        return true;
    }

    private static void RequireKey(string value, string field)
    {
        RequireText(value, field);
        Require(value.Length <= 100, $"{field} key exceeds 100 characters.");
        Require(value.All(character => char.IsAsciiLetterOrDigit(character) || character is '-' or '_'),
            $"{field} key '{value}' may only contain letters, digits, '-' and '_'.");
    }

    private static void RequireText(string value, string field) =>
        Require(!string.IsNullOrWhiteSpace(value), $"{field} is required.");

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new QuestionBankValidationException(message);
        }
    }
}
