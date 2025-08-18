// Converters/SelectedMarkConverter.cs
using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AvaloniaApplication3.Converters
{
    // Input: IsSelected (bool) -> "X" or null
    public sealed class SelectedMarkConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => (value as bool? ?? false) ? "Your Answer" : null;

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
