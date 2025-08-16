using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaApplication3.Helpers
{
    public interface ISaveable
    {
        IAsyncRelayCommand? SaveCommand { get; }
    }

}
