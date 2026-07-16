using CertPrep.Features.Rewards;
using CertPrep.Infrastructure.Settings;
using Xunit;

namespace CertPrep.Tests.Rewards;

public sealed class RankPresentationTests
{
    [Fact]
    public void Roast_mode_is_the_persisted_default_and_clean_mode_removes_the_insult()
    {
        var directory = Path.Combine(Path.GetTempPath(), "CertPrep.Tests", Guid.NewGuid().ToString("N"));
        var path = Path.Combine(directory, "settings.json");

        try
        {
            var first = new RankPresentationService(new AppSettingsStore(path));
            first.Initialize();
            Assert.True(first.IsRoastModeEnabled);
            Assert.Equal("Certified Fucking Noob", first.GetRank(new LevelProgress(1, 20, 20, 200, 10)).Title);
            Assert.Equal("Disgustingly competent.", first.GetReaction(90).Caption);

            first.SetRoastMode(false);
            var second = new RankPresentationService(new AppSettingsStore(path));
            second.Initialize();
            Assert.False(second.IsRoastModeEnabled);
            Assert.Equal("Rookie", second.GetRank(new LevelProgress(1, 20, 20, 200, 10)).Title);
            Assert.Equal("Strong result. Keep the readiness evidence fresh.", second.GetReaction(90).Caption);
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
