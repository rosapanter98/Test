using Avalonia;
using Avalonia.Styling;
using CertPrep.Infrastructure.Settings;

namespace CertPrep.Shared;

public enum AppTheme
{
    System,
    Light,
    Dark
}

public sealed class AppearanceService(AppSettingsStore settingsStore)
{
    public AppTheme CurrentTheme { get; private set; } = AppTheme.System;

    public void Initialize()
    {
        CurrentTheme = settingsStore.Load().Theme;
        Apply(CurrentTheme);
    }

    public void SetTheme(AppTheme theme)
    {
        if (!Enum.IsDefined(theme))
        {
            throw new ArgumentOutOfRangeException(nameof(theme));
        }

        CurrentTheme = theme;
        Apply(theme);
        settingsStore.SaveTheme(theme);
    }

    private static void Apply(AppTheme theme)
    {
        if (Application.Current is not { } application)
        {
            return;
        }

        application.RequestedThemeVariant = theme switch
        {
            AppTheme.Light => ThemeVariant.Light,
            AppTheme.Dark => ThemeVariant.Dark,
            _ => ThemeVariant.Default
        };
    }
}
