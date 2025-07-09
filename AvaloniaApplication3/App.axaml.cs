using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using AvaloniaApplication3.Models;
using AvaloniaApplication3.ViewModels;
using AvaloniaApplication3.Views;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AvaloniaApplication3
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            using (var db = new AppDbContext())
            {
#if DEBUG
                // Delete database if it exists to avoid schema mismatch
                if (System.IO.File.Exists("app.db"))
                    System.IO.File.Delete("app.db");

                db.Database.EnsureCreated(); // Fast DB init for dev
#else
        db.Database.Migrate(); // Apply migrations in production
#endif

                // Add admin user if it doesn't exist
                if (!db.Users.Any(u => u.Username == "admin"))
                {
                    db.Users.Add(new User
                    {
                        Username = "admin",
                        DisplayName = "Administrator",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123")
                    });
                    db.SaveChanges();
                }
            }

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                DisableAvaloniaDataAnnotationValidation();

                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }



        private void DisableAvaloniaDataAnnotationValidation()
        {
            var dataValidationPluginsToRemove =
                BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

            foreach (var plugin in dataValidationPluginsToRemove)
            {
                BindingPlugins.DataValidators.Remove(plugin);
            }
        }
    }
}
