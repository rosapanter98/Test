// Converters/BoolToResultBrushConverter.cs
using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AvaloniaApplication3.Converters
{
    public sealed class BoolToResultBrushConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is bool b ? (b ? Brushes.LightGreen : Brushes.IndianRed) : Brushes.Transparent;

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
