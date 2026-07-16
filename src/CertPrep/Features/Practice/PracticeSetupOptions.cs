namespace CertPrep.Features.Practice;

public sealed record QuestionCountOption(int Count, string Label);
public sealed record PracticeModeOption(PracticeMode Mode, string Name, string Description);

public static class PracticeSetupOptions
{
    public static IReadOnlyList<QuestionCountOption> BuildQuestionCounts(int available, int minimum = 1)
    {
        minimum = Math.Max(1, minimum);
        var counts = new[] { 5, 10, 20, 40 }
            .Where(count => count >= minimum && count <= available)
            .Append(available)
            .Where(count => count >= minimum)
            .Distinct()
            .OrderBy(count => count)
            .ToList();

        return counts.Select(count => new QuestionCountOption(
            count,
            count == available ? $"All {count} questions" : $"{count} questions")).ToList();
    }

    public static IReadOnlyList<PracticeModeOption> BuildModes() =>
    [
        new(PracticeMode.Study, "Study mode", "Check each answer and see the explanation immediately."),
        new(PracticeMode.ExamSimulation, "Exam simulation", "No answer feedback until the session is complete.")
    ];
}
