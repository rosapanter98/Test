using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Linq;
using System.Reflection;

namespace AvaloniaApplication3.ViewModels
{
    public class ViewModelBase : ObservableObject
    {
        protected void NotifyAllCanExecuteChanged()
        {
            var commandProperties = this.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => typeof(IRelayCommand).IsAssignableFrom(p.PropertyType));

            foreach (var prop in commandProperties)
            {
                if (prop.GetValue(this) is IRelayCommand command)
                {
                    command.NotifyCanExecuteChanged();
                }
            }
        }
    }
}