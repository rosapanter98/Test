// Converters/AnswerMarkConverter.cs
using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AvaloniaApplication3.Converters
{
    // inputs: [answerIsSelected(bool), answerIsCorrect(bool)]
    public sealed class AnswerMarkConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count < 2) return null;
            var selected = values[0] as bool? ?? false;
            var correct = values[1] as bool? ?? false;

            if (!selected) return null;

            // ASCII-safe marks to avoid encoding issues
            return correct ? "[OK]" : "[X]";
        }
    }
}