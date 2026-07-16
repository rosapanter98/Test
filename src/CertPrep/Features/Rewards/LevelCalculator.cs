namespace CertPrep.Features.Rewards;

public sealed class LevelCalculator
{
    public LevelProgress Calculate(int totalXp)
    {
        var remaining = Math.Max(0, totalXp);
        var level = 1;
        var required = RequiredForNextLevel(level);
        while (remaining >= required)
        {
            remaining -= required;
            level++;
            required = RequiredForNextLevel(level);
        }

        return new LevelProgress(
            level,
            Math.Max(0, totalXp),
            remaining,
            required,
            (int)Math.Round(remaining * 100d / required, MidpointRounding.AwayFromZero));
    }

    private static int RequiredForNextLevel(int level) => 200 + ((level - 1) * 50);
}
