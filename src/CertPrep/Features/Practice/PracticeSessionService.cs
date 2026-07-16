using CertPrep.Features.ExamCatalog;
using CertPrep.Features.Rewards;
using CertPrep.Features.Progress;

namespace CertPrep.Features.Practice;

public sealed class PracticeSessionService(
    PracticeRepository repository,
    RewardService rewardService,
    MasteryService masteryService,
    TimeProvider timeProvider,
    Random random)
{
    public async Task<ActiveSessionSummary?> GetActiveSessionAsync(
        CancellationToken cancellationToken = default)
    {
        var session = await repository.GetActiveSessionAsync(cancellationToken);
        return session is null
            ? null
            : new ActiveSessionSummary(
                session.Id,
                session.ExamCodeSnapshot,
                session.ExamTitleSnapshot,
                session.Mode,
                session.Items.Count(item => item.SubmittedAt is not null),
                session.Items.Count,
                session.StartedAt);
    }

    public async Task<PracticeResume> ResumeAsync(
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        var session = await repository.GetSessionAsync(sessionId, cancellationToken)
            ?? throw new InvalidOperationException("The saved session no longer exists.");
        if (session.Status != PracticeSessionStatus.InProgress)
        {
            throw new InvalidOperationException("Only an in-progress session can be resumed.");
        }

        if (session.Items.All(item => item.SubmittedAt is not null))
        {
            return new PracticeResume(null, await CompleteAsync(sessionId, cancellationToken));
        }

        return new PracticeResume(MapRun(session), null);
    }

    public Task<PracticeRun> StartAsync(
        int examId,
        PracticeMode mode,
        int questionCount,
        CancellationToken cancellationToken = default) =>
        StartAsync([examId], mode, questionCount, cancellationToken);

    public async Task<PracticeRun> StartAsync(
        IReadOnlyCollection<int> examIds,
        PracticeMode mode,
        int questionCount,
        CancellationToken cancellationToken = default)
    {
        var requestedExamIds = examIds.Distinct().ToList();
        if (requestedExamIds.Count == 0)
        {
            throw new InvalidOperationException("Select at least one exam.");
        }

        if (questionCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(questionCount), "Choose at least one question.");
        }

        var exams = await repository.LoadExamsWithQuestionsAsync(requestedExamIds, cancellationToken);
        if (exams.Count != requestedExamIds.Count)
        {
            throw new InvalidOperationException("One or more selected exams no longer exist.");
        }

        var emptyExam = exams.FirstOrDefault(exam => exam.Questions.Count == 0);
        if (emptyExam is not null)
        {
            throw new InvalidOperationException($"{emptyExam.Code} does not have any active questions.");
        }

        if (exams.Count > 1 && questionCount < exams.Count)
        {
            throw new InvalidOperationException("Choose a session length that includes at least one question per exam.");
        }

        var history = await repository.GetQuestionHistoryAsync(requestedExamIds, cancellationToken);
        var selectedQuestions = SelectBalancedQuestions(exams, questionCount, history);
        return await CreateSessionAsync(
            exams,
            selectedQuestions,
            mode,
            PracticeSessionPurpose.Standard,
            cancellationToken);
    }

    public async Task<PracticeRun> StartReviewAsync(
        IReadOnlyCollection<int> sourceQuestionIds,
        CancellationToken cancellationToken = default)
    {
        var requestedIds = sourceQuestionIds.Distinct().ToList();
        if (requestedIds.Count == 0)
        {
            throw new InvalidOperationException("There are no missed questions to retry.");
        }

        var questions = await repository.LoadQuestionsAsync(requestedIds, cancellationToken);
        if (questions.Count != requestedIds.Count)
        {
            throw new InvalidOperationException("One or more missed questions are no longer available.");
        }

        var exams = questions
            .Select(question => question.Exam)
            .DistinctBy(exam => exam.Id)
            .OrderBy(exam => exam.Code)
            .ToList();

        return await CreateSessionAsync(
            exams,
            Shuffle(questions),
            PracticeMode.Study,
            PracticeSessionPurpose.Review,
            cancellationToken);
    }

    public async Task<PracticeRun> StartBossAsync(
        int examId,
        CancellationToken cancellationToken = default)
    {
        var readiness = (await masteryService.GetExamReadinessAsync(cancellationToken))
            .SingleOrDefault(exam => exam.ExamId == examId)
            ?? throw new InvalidOperationException("The exam no longer exists.");
        if (!readiness.BossUnlocked)
        {
            throw new InvalidOperationException("Reach Reliable in every objective before starting the Boss Exam.");
        }

        var exams = await repository.LoadExamsWithQuestionsAsync([examId], cancellationToken);
        var exam = exams.Single();
        var history = await repository.GetQuestionHistoryAsync([examId], cancellationToken);
        var selectedQuestions = SelectBossQuestions(exam, Math.Min(40, exam.Questions.Count), history);
        return await CreateSessionAsync(
            exams,
            selectedQuestions,
            PracticeMode.ExamSimulation,
            PracticeSessionPurpose.Boss,
            cancellationToken);
    }

    private async Task<PracticeRun> CreateSessionAsync(
        IReadOnlyList<Exam> exams,
        IReadOnlyList<Question> selectedQuestions,
        PracticeMode mode,
        PracticeSessionPurpose purpose,
        CancellationToken cancellationToken)
    {
        var isMixed = exams.Count > 1;
        var examCodes = exams.Select(exam => exam.Code).OrderBy(code => code).ToList();
        var mixedTitle = string.Join(" + ", examCodes);
        if (mixedTitle.Length > 200)
        {
            mixedTitle = $"{exams.Count} exams mixed practice";
        }

        var session = new PracticeSession
        {
            ExamId = isMixed ? null : exams[0].Id,
            ExamCodeSnapshot = isMixed ? "MIXED" : exams[0].Code,
            ExamTitleSnapshot = isMixed ? mixedTitle : exams[0].Title,
            Scope = isMixed ? PracticeSessionScope.MixedExam : PracticeSessionScope.SingleExam,
            Mode = mode,
            Purpose = purpose,
            StartedAt = timeProvider.GetUtcNow(),
            Items = selectedQuestions.Select((question, questionIndex) => new PracticeSessionItem
            {
                SourceQuestionId = question.Id,
                SourceExamId = question.ExamId,
                ExamCodeSnapshot = question.Exam.Code,
                ExamTitleSnapshot = question.Exam.Title,
                OrderIndex = questionIndex,
                ObjectiveContentKeySnapshot = question.Objective.ContentKey,
                ObjectiveName = question.Objective.Name,
                Prompt = question.Prompt,
                Kind = question.Kind,
                Difficulty = question.Difficulty,
                Explanation = question.Explanation,
                SourceName = question.SourceName,
                SourceUrl = question.SourceUrl,
                Choices = Shuffle(question.Choices).Select((choice, choiceIndex) => new PracticeSessionChoice
                {
                    SourceAnswerChoiceId = choice.Id,
                    Text = choice.Text,
                    IsCorrect = choice.IsCorrect,
                    SortOrder = choiceIndex
                }).ToList()
            }).ToList()
        };

        await repository.SaveNewSessionAsync(session, cancellationToken);
        return MapRun(session);
    }

    public async Task<SubmissionFeedback> SubmitAsync(
        int sessionItemId,
        IReadOnlyCollection<int> selectedChoiceIds,
        CancellationToken cancellationToken = default)
    {
        var candidate = await repository.GetSubmissionCandidateAsync(sessionItemId, cancellationToken)
            ?? throw new InvalidOperationException("The practice question no longer exists.");

        if (candidate.IsSubmitted)
        {
            throw new InvalidOperationException("This question has already been submitted.");
        }

        var selected = selectedChoiceIds.ToHashSet();
        if (selected.Count == 0)
        {
            throw new InvalidOperationException("Select an answer before submitting.");
        }

        if (!selected.IsSubsetOf(candidate.AvailableChoiceIds))
        {
            throw new InvalidOperationException("The selection contains an answer that is not part of this question.");
        }

        if (selected.Count != candidate.CorrectChoiceIds.Count)
        {
            var answerLabel = candidate.CorrectChoiceIds.Count == 1 ? "answer" : "answers";
            throw new InvalidOperationException(
                $"Select exactly {candidate.CorrectChoiceIds.Count} {answerLabel} for this question.");
        }

        var isCorrect = selected.SetEquals(candidate.CorrectChoiceIds);
        await repository.SaveSubmissionAsync(
            sessionItemId,
            selected,
            isCorrect,
            timeProvider.GetUtcNow(),
            cancellationToken);

        return new SubmissionFeedback(
            isCorrect,
            candidate.Explanation,
            candidate.SourceName,
            candidate.SourceUrl,
            candidate.CorrectChoiceIds);
    }

    public Task SaveDraftSelectionAsync(
        int sessionItemId,
        IReadOnlyCollection<int> selectedChoiceIds,
        CancellationToken cancellationToken = default) =>
        repository.SaveDraftSelectionAsync(
            sessionItemId,
            selectedChoiceIds.ToHashSet(),
            cancellationToken);

    public async Task<PracticeResult> CompleteAsync(
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        await repository.MarkCompletedAsync(sessionId, timeProvider.GetUtcNow(), cancellationToken);
        var session = await repository.GetSessionAsync(sessionId, cancellationToken)
            ?? throw new InvalidOperationException("The completed session could not be loaded.");

        var correct = session.Items.Count(item => item.IsCorrect == true);
        var total = session.Items.Count;
        var objectives = session.Items
            .GroupBy(item => new
            {
                item.ExamCodeSnapshot,
                item.ObjectiveContentKeySnapshot,
                item.ObjectiveName
            })
            .Select(group =>
            {
                var objectiveCorrect = group.Count(item => item.IsCorrect == true);
                var objectiveTotal = group.Count();
                return new ObjectiveResult(
                    group.Key.ExamCodeSnapshot,
                    group.Key.ObjectiveContentKeySnapshot,
                    group.Key.ObjectiveName,
                    objectiveCorrect,
                    objectiveTotal,
                    Percentage(objectiveCorrect, objectiveTotal));
            })
            .OrderBy(result => result.ExamCode)
            .ThenBy(result => result.Name)
            .ToList();
        var missedQuestions = session.Items
            .Where(item => item.IsCorrect != true)
            .OrderBy(item => item.OrderIndex)
            .Select(item => new MissedQuestionReview(
                item.SourceQuestionId,
                item.ExamCodeSnapshot,
                item.ObjectiveName,
                item.Prompt,
                item.Choices.Where(choice => choice.IsSelected).OrderBy(choice => choice.SortOrder).Select(choice => choice.Text).ToList(),
                item.Choices.Where(choice => choice.IsCorrect).OrderBy(choice => choice.SortOrder).Select(choice => choice.Text).ToList(),
                item.Explanation,
                item.SourceName,
                item.SourceUrl))
            .ToList();
        var rewards = await rewardService.AwardCompletedSessionAsync(session.Id, cancellationToken);

        return new PracticeResult(
            session.Id,
            session.ExamCodeSnapshot,
            session.ExamTitleSnapshot,
            session.Mode,
            session.Purpose,
            correct,
            total,
            Percentage(correct, total),
            session.CompletedAt!.Value - session.StartedAt,
            objectives,
            missedQuestions,
            rewards);
    }

    public Task AbandonAsync(int sessionId, CancellationToken cancellationToken = default) =>
        repository.MarkAbandonedAsync(sessionId, cancellationToken);

    private PracticeRun MapRun(PracticeSession session) =>
        new(
            session.Id,
            session.ExamCodeSnapshot,
            session.ExamTitleSnapshot,
            session.Scope,
            session.Mode,
            session.Purpose,
            session.Items
                .Where(item => item.SubmittedAt is null)
                .Select(item => item.OrderIndex)
                .DefaultIfEmpty(session.Items.Count)
                .Min(),
            session.Items.OrderBy(item => item.OrderIndex).Select(item => new PracticeQuestion(
                item.Id,
                item.SourceQuestionId,
                item.ExamCodeSnapshot,
                item.ExamTitleSnapshot,
                item.ObjectiveName,
                item.Prompt,
                item.Kind,
                item.Choices.Count(choice => choice.IsCorrect),
                item.Difficulty,
                item.Choices.OrderBy(choice => choice.SortOrder)
                    .Select(choice => new PracticeChoice(choice.Id, choice.Text, choice.IsSelected))
                    .ToList())).ToList());

    private List<Question> SelectBalancedQuestions(
        IReadOnlyList<Exam> exams,
        int requestedCount,
        IReadOnlyDictionary<int, QuestionPracticeHistory> history)
    {
        var availableCount = exams.Sum(exam => exam.Questions.Count);
        var targetCount = Math.Min(requestedCount, availableCount);
        var queues = exams
            .Where(exam => exam.Questions.Count > 0)
            .Select(exam => new Queue<Question>(WeightedShuffle(exam.Questions, history)))
            .ToList();
        var selected = new List<Question>(targetCount);

        while (selected.Count < targetCount)
        {
            foreach (var queue in Shuffle(queues.Where(queue => queue.Count > 0)))
            {
                selected.Add(queue.Dequeue());
                if (selected.Count == targetCount)
                {
                    break;
                }
            }
        }

        return Shuffle(selected);
    }

    private List<Question> SelectBossQuestions(
        Exam exam,
        int requestedCount,
        IReadOnlyDictionary<int, QuestionPracticeHistory> history)
    {
        var queues = exam.Questions
            .GroupBy(question => question.Objective.ContentKey, StringComparer.OrdinalIgnoreCase)
            .Select(group => new Queue<Question>(WeightedShuffle(group, history)))
            .ToList();
        var selected = new List<Question>(requestedCount);
        while (selected.Count < requestedCount)
        {
            foreach (var queue in Shuffle(queues.Where(queue => queue.Count > 0)))
            {
                selected.Add(queue.Dequeue());
                if (selected.Count == requestedCount)
                {
                    break;
                }
            }
        }

        return Shuffle(selected);
    }

    private List<Question> WeightedShuffle(
        IEnumerable<Question> questions,
        IReadOnlyDictionary<int, QuestionPracticeHistory> history) =>
        questions
            .Select(question =>
            {
                var weight = 1d;
                if (!history.TryGetValue(question.Id, out var performance))
                {
                    weight += 0.5;
                }
                else
                {
                    weight += performance.IncorrectAttempts * 3d / Math.Max(1, performance.Attempts);
                    if (performance.LastAttemptWasIncorrect)
                    {
                        weight += 4;
                    }
                }

                var sample = Math.Max(double.Epsilon, random.NextDouble());
                return (Question: question, SortKey: -Math.Log(sample) / weight);
            })
            .OrderBy(item => item.SortKey)
            .Select(item => item.Question)
            .ToList();

    private List<T> Shuffle<T>(IEnumerable<T> source)
    {
        var items = source.ToList();
        for (var index = items.Count - 1; index > 0; index--)
        {
            var swapIndex = random.Next(index + 1);
            (items[index], items[swapIndex]) = (items[swapIndex], items[index]);
        }

        return items;
    }

    private static int Percentage(int correct, int total) =>
        total == 0
            ? 0
            : (int)Math.Round(correct * 100d / total, MidpointRounding.AwayFromZero);
}
