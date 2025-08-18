
using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AvaloniaApplication3.Converters
{
    public class BoolToResultTextConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is bool b ? (b ? "Correct!" : "Incorrect.") : null;

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
