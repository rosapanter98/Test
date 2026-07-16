namespace CertPrep.Features.Progress;

public sealed class MasteryService(
    ProgressRepository repository,
    MasteryCalculator calculator)
{
    public async Task<IReadOnlyList<ExamReadiness>> GetExamReadinessAsync(
        CancellationToken cancellationToken = default)
    {
        var inputs = await repository.GetMasteryInputsAsync(cancellationToken);
        return inputs
            .GroupBy(input => input.ExamId)
            .Select(group => calculator.CalculateExam(group.ToList()))
            .OrderBy(exam => exam.ExamCode)
            .ToList();
    }
}
