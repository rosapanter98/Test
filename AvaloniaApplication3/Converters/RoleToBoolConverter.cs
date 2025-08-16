using System;
using System.Globalization;
using Avalonia.Data.Converters;
using AvaloniaApplication3.Models;

namespace AvaloniaApplication3.Converters
{
    public class RoleToBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is UserRole role && (role == UserRole.Admin || role == UserRole.Moderator);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
