using CertPrep.Infrastructure.Settings;
using CertPrep.Shared;
using Xunit;

namespace CertPrep.Tests.Settings;

public sealed class AppearanceSettingsTests
{
    [Fact]
    public void System_is_default_and_an_explicit_choice_is_persisted()
    {
        var directory = Path.Combine(Path.GetTempPath(), "CertPrep.Tests", Guid.NewGuid().ToString("N"));
        var path = Path.Combine(directory, "settings.json");

        try
        {
            var first = new AppearanceService(new AppSettingsStore(path));
            first.Initialize();
            Assert.Equal(AppTheme.System, first.CurrentTheme);

            first.SetTheme(AppTheme.Dark);
            var second = new AppearanceService(new AppSettingsStore(path));
            second.Initialize();
            Assert.Equal(AppTheme.Dark, second.CurrentTheme);
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }
    }
}
