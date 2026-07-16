namespace CertPrep.Infrastructure.Persistence;

public static class AppPaths
{
    public static string GetDatabasePath() => Path.Combine(GetApplicationDirectory(), "certprep.db");

    public static string GetSettingsPath() => Path.Combine(GetApplicationDirectory(), "settings.json");

    public static string GetBackupsDirectory() => Path.Combine(GetApplicationDirectory(), "backups");

    private static string GetApplicationDirectory()
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CertPrep"
#if DEBUG
            , "Dev"
#endif
        );

        Directory.CreateDirectory(directory);
        return directory;
    }
}
