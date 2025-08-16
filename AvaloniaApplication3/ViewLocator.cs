using Avalonia.Controls;
using Avalonia.Controls.Templates;
using AvaloniaApplication3.ViewModels;
using System;

namespace AvaloniaApplication3
{
    public class ViewLocator : IDataTemplate
    {
        public Control? Build(object? param)
        {
            if (param is null) return null;

            var viewModelType = param.GetType();
            var viewTypeName = viewModelType.FullName?
                .Replace("ViewModels", "Views")
                .Replace("ViewModel", "View");

            if (viewTypeName == null)
                return new TextBlock { Text = "Invalid ViewModel" };

            var viewType = Type.GetType(viewTypeName);

            if (viewType != null)
                return (Control)Activator.CreateInstance(viewType)!;

            return new TextBlock { Text = $"Not Found: {viewTypeName}" };
        }

        public bool Match(object? data)
        {
            return data is ViewModelBase;
        }
    }
}
