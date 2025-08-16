using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using AvaloniaApplication3.Models;
using AvaloniaApplication3.Repositories;
using AvaloniaApplication3.Services;
using AvaloniaApplication3.ViewModels;
using AvaloniaApplication3.Views;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
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

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                DisableAvaloniaDataAnnotationValidation();

                // --- SQLite location (per-user) ---
                var dbPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "quiz.db");

                // --- Build EF options once; reuse everywhere ---
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlite($"Data Source={dbPath}")
                    .Options;

                // Ensure DB + apply migrations
                using (var ctx = new AppDbContext(options))
                {
                    ctx.Database.Migrate();
                }

            
                using (var ctx = new AppDbContext(options))
                {
                    ctx.Database.Migrate();

                    if (!ctx.Users.Any(u => u.Username == "admin"))
                    {
                        var admin = new User
                        {
                            Username = "a",
                            DisplayName = "Administrator",
                            PasswordHash = BCrypt.Net.BCrypt.HashPassword("a"),
                            Role = UserRole.Admin,
                            CreatedAt = DateTime.UtcNow
                        };
                        ctx.Users.Add(admin);
                        ctx.SaveChanges();
                    }
                }


                // --- Manual composition root ---
                var session = new SessionService();

                // Repositories share the same options
                var userRepo = new EfUserRepository(new AppDbContext(options));
                var loginSvc = new LoginService(userRepo);

                var quizRepo = new EfQuizRepository(new AppDbContext(options));
                var quizSvc = new QuizService(quizRepo);

                var attemptsRepo = new EfQuizAttemptRepository(new AppDbContext(options));
                var attemptsSvc = new QuizAttemptService(attemptsRepo);

                var mainVm = new MainWindowViewModel(session, loginSvc, quizSvc, attemptsSvc);

                desktop.MainWindow = new MainWindow
                {
                    DataContext = mainVm
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

        private static void DisableAvaloniaDataAnnotationValidation()
        {
            foreach (var plugin in BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToList())
                BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}
