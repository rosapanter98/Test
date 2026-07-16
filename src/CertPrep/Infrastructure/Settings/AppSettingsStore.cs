using System.Text.Json;
using CertPrep.Shared;

namespace CertPrep.Infrastructure.Settings;

public sealed record AppSettings(AppTheme Theme, bool RoastModeEnabled);

public sealed class AppSettingsStore(string path)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly object _gate = new();

    public AppSettings Load()
    {
        lock (_gate)
        {
            return LoadCore();
        }
    }

    public void SaveTheme(AppTheme theme)
    {
        lock (_gate)
        {
            SaveCore(LoadCore() with { Theme = theme });
        }
    }

    public void SaveRoastMode(bool enabled)
    {
        lock (_gate)
        {
            SaveCore(LoadCore() with { RoastModeEnabled = enabled });
        }
    }

    private AppSettings LoadCore()
    {
        if (!File.Exists(path))
        {
            return Defaults();
        }

        try
        {
            var persisted = JsonSerializer.Deserialize<PersistedSettings>(File.ReadAllText(path));
            return new AppSettings(
                persisted is not null && Enum.IsDefined(persisted.Theme)
                    ? persisted.Theme
                    : AppTheme.System,
                persisted?.RoastModeEnabled ?? true);
        }
        catch (JsonException)
        {
            return Defaults();
        }
        catch (IOException)
        {
            return Defaults();
        }
    }

    private void SaveCore(AppSettings settings)
    {
        var directory = Path.GetDirectoryName(path)
            ?? throw new InvalidOperationException("The application settings path has no directory.");
        Directory.CreateDirectory(directory);

        var temporaryPath = $"{path}.tmp";
        File.WriteAllText(
            temporaryPath,
            JsonSerializer.Serialize(
                new PersistedSettings(settings.Theme, settings.RoastModeEnabled),
                JsonOptions));
        File.Move(temporaryPath, path, true);
    }

    private static AppSettings Defaults() => new(AppTheme.System, true);

    private sealed record PersistedSettings(AppTheme Theme, bool? RoastModeEnabled);
}
