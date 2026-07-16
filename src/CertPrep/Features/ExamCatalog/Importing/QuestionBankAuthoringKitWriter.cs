using System.IO.Compression;

namespace CertPrep.Features.ExamCatalog.Importing;

public sealed class QuestionBankAuthoringKitWriter
{
    private const string ResourcePrefix = "CertPrep.Content.Authoring.";
    private static readonly (string ResourceName, string EntryName)[] Files =
    [
        ("README.md", "README.md"),
        ("question-bank.schema.json", "question-bank.schema.json"),
        ("question-bank-template.json", "question-bank-template.json")
    ];

    public async Task WriteAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("An authoring-kit path is required.", nameof(path));
        }

        var fullPath = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(fullPath)
            ?? throw new IOException("The authoring-kit folder could not be determined.");
        Directory.CreateDirectory(directory);

        var temporaryPath = Path.Combine(
            directory,
            $".{Path.GetFileName(fullPath)}.{Guid.NewGuid():N}.tmp");

        try
        {
            await using (var output = new FileStream(
                temporaryPath,
                FileMode.CreateNew,
                FileAccess.ReadWrite,
                FileShare.None,
                81920,
                FileOptions.Asynchronous))
            using (var archive = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: false))
            {
                var assembly = typeof(QuestionBankAuthoringKitWriter).Assembly;
                foreach (var (resourceName, entryName) in Files)
                {
                    await using var input = assembly.GetManifestResourceStream(ResourcePrefix + resourceName)
                        ?? throw new InvalidOperationException(
                            $"Authoring-kit resource '{resourceName}' was not found.");
                    await using var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal).Open();
                    await input.CopyToAsync(entry, cancellationToken);
                }
            }

            File.Move(temporaryPath, fullPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }
}
