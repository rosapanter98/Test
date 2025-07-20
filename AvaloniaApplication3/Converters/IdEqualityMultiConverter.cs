using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace AvaloniaApplication3.Converters
{
    public class IdEqualityMultiConverter : IMultiValueConverter
    {
        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count < 2 || values[0] is not int selectedId || values[1] is not int currentId)
                return false;

            return selectedId == currentId;
        }

        public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
        {
            if (value is bool isChecked && isChecked)
                return new object[] { BindingOperations.DoNothing, BindingOperations.DoNothing };

            return new object[] { BindingOperations.DoNothing, BindingOperations.DoNothing };
        }
    }
}
