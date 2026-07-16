using CertPrep.Features.ExamCatalog.Importing;

namespace CertPrep.Infrastructure.Persistence;

public sealed class DatabaseSeeder(
    QuestionBankPackageReader packageReader,
    QuestionBankMerger merger)
{
    public async Task SeedAsync(
        StudyDbContext context,
        CancellationToken cancellationToken = default)
    {
        var package = await packageReader.ReadEmbeddedAsync(cancellationToken);
        await merger.MergeAsync(context, package, cancellationToken);
    }
}
