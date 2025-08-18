// Converters/AnswerCorrectBrushConverter.cs
using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AvaloniaApplication3.Converters
{
    // Input: IsCorrect (bool)
    public sealed class AnswerCorrectBrushConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var isCorrect = value as bool? ?? false;
            return isCorrect ? Brushes.LightGreen : Brushes.IndianRed;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
