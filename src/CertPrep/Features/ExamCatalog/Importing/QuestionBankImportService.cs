using CertPrep.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CertPrep.Features.ExamCatalog.Importing;

public sealed class QuestionBankImportService(
    IDbContextFactory<StudyDbContext> contextFactory,
    QuestionBankPackageReader packageReader,
    QuestionBankMerger merger,
    QuestionBankAuthoringKitWriter authoringKitWriter)
{
    public async Task<QuestionBankMergeResult> ImportAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        var package = await packageReader.ReadFileAsync(path, cancellationToken);
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await merger.MergeAsync(context, package, cancellationToken);
    }

    public Task SaveAuthoringKitAsync(
        string path,
        CancellationToken cancellationToken = default) =>
        authoringKitWriter.WriteAsync(path, cancellationToken);
}
