using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Linq;
using System.Reflection;

namespace AvaloniaApplication3.Utility
{
    public class ViewModelBase : ObservableObject
    {
        public void NotifyAllCanExecuteChanged()
        {
            var relayCommands = GetType()
                .GetProperties()
                .Where(p => typeof(IRelayCommand).IsAssignableFrom(p.PropertyType))
                .Select(p => p.GetValue(this) as IRelayCommand)
                .Where(c => c != null);

            foreach (var command in relayCommands)
            {
                command!.NotifyCanExecuteChanged();
            }
        }
    }
}