using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using AvaloniaApplication3.Models;
using AvaloniaApplication3.Models.Enums;
using AvaloniaApplication3.Repositories;
using AvaloniaApplication3.Services;
using AvaloniaApplication3.Utility;
using AvaloniaApplication3.ViewModels;
using AvaloniaApplication3.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AvaloniaApplication3
{
    public partial class App : Application
    {
        private ServiceProvider? _services;
        private IServiceScope? _appScope; // persistent scope for app lifetime

        public override void Initialize() => AvaloniaXamlLoader.Load(this);

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                DisableAvaloniaDataAnnotationValidation();

                var sc = new ServiceCollection();

                // EF Core (scoped DbContext)
                sc.AddDbContext<AppDbContext>(o => o.UseSqlite(DbConfig.GetConnectionString()));

                // Repositories (scoped)
                sc.AddScoped<IUserRepository, EfUserRepository>();
                sc.AddScoped<IQuizRepository, EfQuizRepository>();
                sc.AddScoped<IQuizAttemptRepository, EfQuizAttemptRepository>();

                // Services (scoped except session)
                sc.AddScoped<IUserService, UserService>();
                sc.AddScoped<ILoginService, LoginService>();
                sc.AddScoped<IQuizService, QuizService>();
                sc.AddScoped<IQuizAttemptService, QuizAttemptService>();
                sc.AddScoped<IQuizImportService, QuizImportService>();
                sc.AddSingleton<SessionService>();

                _services = sc.BuildServiceProvider();

                // ---- Migrate + seed in a short-lived scope ----
                using (var scope = _services.CreateScope())
                {
                    var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    Directory.CreateDirectory(Path.GetDirectoryName(DbConfig.GetDbPath())!);
                    ctx.Database.Migrate();

                    if (!ctx.Users.Any(u => u.Username == "admin"))
                    {
                        ctx.Users.Add(new User
                        {
                            Username = "admin",
                            DisplayName = "Administrator",
                            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                            Role = UserRole.Admin,
                            CreatedAt = DateTime.UtcNow
                        });
                    }

                    if (!ctx.Quizzes.Any())
                    {
                        var quiz = new Quiz
                        {
                            Title = "Sample Quiz",
                            Description = "A test quiz with mixed questions",
                            Questions = new List<Question>
                            {
                                new Question
                                {
                                    Text = "What is 2 + 2?",
                                    Type = QuestionType.SingleChoice,
                                    Explanation = "2 + 2 is 4.",
                                    Answers = new List<Answer>
                                    {
                                        new Answer { Text = "3", IsCorrect = false },
                                        new Answer { Text = "4", IsCorrect = true },
                                        new Answer { Text = "5", IsCorrect = false },
                                    }
                                },
                                new Question
                                {
                                    Text = "Select all fruits.",
                                    Type = QuestionType.MultipleChoice,
                                    Explanation = "Apple and Banana are fruits. Carrot is not.",
                                    Answers = new List<Answer>
                                    {
                                        new Answer { Text = "Apple",  IsCorrect = true },
                                        new Answer { Text = "Banana", IsCorrect = true },
                                        new Answer { Text = "Carrot", IsCorrect = false },
                                    }
                                },
                                new Question
                                {
                                    Text = "The earth is flat.",
                                    Type = QuestionType.TrueFalse,
                                    Explanation = "No, the earth is spherical.",
                                    Answers = new List<Answer>
                                    {
                                        new Answer { Text = "True",  IsCorrect = false },
                                        new Answer { Text = "False", IsCorrect = true },
                                    }
                                }
                            }
                        };
                        ctx.Quizzes.Add(quiz);
                    }

                    ctx.SaveChanges();
                }

                // ---- Create a persistent scope for VMs/services used by the UI ----
                _appScope = _services.CreateScope(); // DO NOT dispose until app exit

                var sp = _appScope.ServiceProvider;
                var session = sp.GetRequiredService<SessionService>();
                var loginSvc = sp.GetRequiredService<ILoginService>();
                var quizSvc = sp.GetRequiredService<IQuizService>();
                var attemptsSvc = sp.GetRequiredService<IQuizAttemptService>();
                var userSvc = sp.GetRequiredService<IUserService>();
                var quizImportSvc = sp.GetRequiredService<IQuizImportService>();

                var mainVm = new MainWindowViewModel(session, loginSvc, quizSvc, attemptsSvc, quizImportSvc, userSvc);

                desktop.MainWindow = new MainWindow { DataContext = mainVm };
                desktop.Exit += (_, __) => DisposeScopes();
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void DisposeScopes()
        {
            _appScope?.Dispose();
            _appScope = null;
            if (_services is IDisposable d) d.Dispose();
            _services = null;
        }

        private static void DisableAvaloniaDataAnnotationValidation()
        {
            foreach (var plugin in BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToList())
                BindingPlugins.DataValidators.Remove(plugin);
        }

        public static T GetService<T>() where T : notnull =>
            ((App)Current!)._appScope!.ServiceProvider.GetRequiredService<T>();
    }
}