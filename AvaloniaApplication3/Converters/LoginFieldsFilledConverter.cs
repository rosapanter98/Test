using Avalonia.Data.Converters;
using AvaloniaApplication3.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaApplication3.Converters
{
    public class LoginFieldsFilledConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is LoginViewModel vm)
                return !string.IsNullOrWhiteSpace(vm.UsernameInput) && !string.IsNullOrWhiteSpace(vm.PasswordInput);
            return false;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
