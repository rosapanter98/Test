using CertPrep.Features.ExamCatalog;
using CertPrep.Features.Rewards;

namespace CertPrep.Features.Practice;

public sealed record PracticeRun(
    int SessionId,
    string ExamCode,
    string ExamTitle,
    PracticeSessionScope Scope,
    PracticeMode Mode,
    PracticeSessionPurpose Purpose,
    int CurrentQuestionIndex,
    IReadOnlyList<PracticeQuestion> Questions);

public sealed record PracticeQuestion(
    int SessionItemId,
    int SourceQuestionId,
    string ExamCode,
    string ExamTitle,
    string ObjectiveName,
    string Prompt,
    QuestionKind Kind,
    int RequiredAnswerCount,
    QuestionDifficulty Difficulty,
    IReadOnlyList<PracticeChoice> Choices);

public sealed record PracticeChoice(int SessionChoiceId, string Text, bool IsSelected);

public sealed record ActiveSessionSummary(
    int SessionId,
    string ExamCode,
    string ExamTitle,
    PracticeMode Mode,
    int AnsweredQuestions,
    int TotalQuestions,
    DateTimeOffset StartedAt);

public sealed record PracticeResume(PracticeRun? Run, PracticeResult? CompletedResult);

public sealed record QuestionPracticeHistory(
    int SourceQuestionId,
    int Attempts,
    int IncorrectAttempts,
    bool LastAttemptWasIncorrect);

public sealed record SubmissionCandidate(
    int SessionItemId,
    QuestionKind Kind,
    bool IsSubmitted,
    string Explanation,
    string SourceName,
    string SourceUrl,
    IReadOnlySet<int> AvailableChoiceIds,
    IReadOnlySet<int> CorrectChoiceIds);

public sealed record SubmissionFeedback(
    bool IsCorrect,
    string Explanation,
    string SourceName,
    string SourceUrl,
    IReadOnlySet<int> CorrectChoiceIds);

public sealed record ObjectiveResult(
    string ExamCode,
    string ObjectiveContentKey,
    string Name,
    int CorrectAnswers,
    int TotalQuestions,
    int ScorePercent);

public sealed record MissedQuestionReview(
    int SourceQuestionId,
    string ExamCode,
    string ObjectiveName,
    string Prompt,
    IReadOnlyList<string> SelectedAnswers,
    IReadOnlyList<string> CorrectAnswers,
    string Explanation,
    string SourceName,
    string SourceUrl);

public sealed record PracticeResult(
    int SessionId,
    string ExamCode,
    string ExamTitle,
    PracticeMode Mode,
    PracticeSessionPurpose Purpose,
    int CorrectAnswers,
    int TotalQuestions,
    int ScorePercent,
    TimeSpan Duration,
    IReadOnlyList<ObjectiveResult> Objectives,
    IReadOnlyList<MissedQuestionReview> MissedQuestions,
    RewardOutcome Rewards);
