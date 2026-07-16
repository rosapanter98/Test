using CertPrep.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CertPrep.Features.ExamCatalog.Importing;

public sealed class QuestionBankImportService(
    IDbContextFactory<StudyDbContext> contextFactory,
    QuestionBankPackageReader packageReader,
    QuestionBankMerger merger)
{
    public async Task<QuestionBankMergeResult> ImportSqliteAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        var package = await packageReader.ReadSqliteAsync(path, cancellationToken);
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await merger.MergeAsync(context, package, cancellationToken);
    }
}
