// Converters/SelectedAnswerOutlineConverter.cs
using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AvaloniaApplication3.Converters
{
    // inputs: [answerIsSelected(bool), answerIsCorrect(bool)]
    public sealed class SelectedAnswerOutlineConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count < 2) return Brushes.Transparent;

            var selected = values[0] as bool? ?? false;
            var correct = values[1] as bool? ?? false;

            if (!selected) return Brushes.Transparent;
            return correct ? Brushes.LightGreen : Brushes.IndianRed;
        }
    }
}
