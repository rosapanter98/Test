using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace AvaloniaApplication3.Converters
{
    public class IdEqualityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value?.ToString() == parameter?.ToString();

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value is true ? int.Parse(parameter!.ToString()!) : BindingOperations.DoNothing;
    }

}