// Converters/ElapsedConverter.cs
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace AvaloniaApplication3.Converters
{
    public class ElapsedConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count < 2) return null;
            if (values[0] is not DateTime started) return null;
            var completed = values[1] as DateTime?;
            var end = completed ?? DateTime.UtcNow;
            var ts = end - started;
            if (ts < TimeSpan.Zero) ts = TimeSpan.Zero;
            return ts.TotalHours >= 1
                ? $"{(int)ts.TotalHours}h {ts.Minutes}m {ts.Seconds}s"
                : $"{ts.Minutes}m {ts.Seconds}s";
        }
    }
}
