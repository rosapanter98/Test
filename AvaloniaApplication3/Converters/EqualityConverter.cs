using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace AvaloniaApplication3.Converters
{
    public class EqualityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return Equals(value, parameter);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b && b)
                return parameter!;
            return BindingOperations.DoNothing;
        }
    }
}
