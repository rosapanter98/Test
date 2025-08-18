// Converters/AnswerOutlineConverter.cs
using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AvaloniaApplication3.Converters
{
    // inputs: [itemIsCorrect(bool), answerIsCorrect(bool), answerIsSelected(bool)]
    public class AnswerOutlineConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count < 3) return Brushes.Transparent;

            var itemIsCorrect = values[0] as bool? ?? false;
            var answerIsCorrect = values[1] as bool? ?? false;
            var answerIsSelected = values[2] as bool? ?? false;

            // Correct item: highlight only selected answers (they are the correct set)
            if (itemIsCorrect)
                return answerIsSelected ? Brushes.LightGreen : Brushes.Transparent;

            // Incorrect item:
            if (answerIsCorrect) return Brushes.LightGreen;                // show correct
            if (answerIsSelected && !answerIsCorrect) return Brushes.IndianRed; // show wrong selection
            return Brushes.Transparent;
        }
    }
}
