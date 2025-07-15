using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using AvaloniaApplication3.Models;
using AvaloniaApplication3.ViewModels;
using AvaloniaApplication3.Views;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
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
                }

                // Add sample quiz if none exist
                if (!db.Quizzes.Any())
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
                            new Answer { Text = "5", IsCorrect = false }
                        }
                    },
                    new Question
                    {
                        Text = "Select all fruits.",
                        Type = QuestionType.MultipleChoice,
                        Explanation = "Apple and Banana are fruits. Carrot is not.",
                        Answers = new List<Answer>
                        {
                            new Answer { Text = "Apple", IsCorrect = true },
                            new Answer { Text = "Banana", IsCorrect = true },
                            new Answer { Text = "Carrot", IsCorrect = false }
                        }
                    },
                    new Question
                    {
                        Text = "The earth is flat.",
                        Type = QuestionType.TrueFalse,
                        Explanation = "No, the earth is spherical.",
                        Answers = new List<Answer>
                        {
                            new Answer { Text = "True", IsCorrect = false },
                            new Answer { Text = "False", IsCorrect = true }
                        }
                    }
                }
                    };

                    db.Quizzes.Add(quiz);
                }

                db.SaveChanges(); // Apply all changes
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
