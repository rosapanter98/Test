using AvaloniaApplication3.Models;
using AvaloniaApplication3.Utility;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace AvaloniaApplication3
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Quiz> Quizzes => Set<Quiz>();
        public DbSet<Question> Questions => Set<Question>();
        public DbSet<Answer> Answers => Set<Answer>();
        public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
        public DbSet<QuizAttemptItem> QuizAttemptItems => Set<QuizAttemptItem>();
        public DbSet<QuizAttemptItemAnswer> QuizAttemptItemAnswers => Set<QuizAttemptItemAnswer>();

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public AppDbContext() { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(DbConfig.GetDbPath())!);
                // Use SQLite for simplicity; adjust as needed for your environment
                optionsBuilder.UseSqlite(DbConfig.GetConnectionString());
            }
            Console.WriteLine($"Using database at: {DbConfig.GetDbPath()}");
        }

        protected override void OnModelCreating(ModelBuilder b)
        {
            // --- Quiz/Question/Answer (basic) ---
            b.Entity<Quiz>(e =>
            {
                e.Property(x => x.Title).IsRequired().HasMaxLength(200);
                e.HasMany(x => x.Questions)
                 .WithOne(x => x.Quiz)
                 .HasForeignKey(x => x.QuizId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            b.Entity<Question>(e =>
            {
                e.Property(x => x.Text).IsRequired();
                e.HasMany(x => x.Answers)
                 .WithOne(x => x.Question)
                 .HasForeignKey(x => x.QuestionId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            b.Entity<User>(e =>
            {
                e.Property(u => u.Username).IsRequired().HasMaxLength(100);
                e.HasIndex(u => u.Username).IsUnique();
                e.Property(u => u.Username).UseCollation("NOCASE");
            });

            b.Entity<Answer>(e =>
            {
                e.Property(x => x.Text).IsRequired();
            });

            // --- Attempts snapshot model ---
            b.Entity<QuizAttempt>(e =>
            {
                e.HasIndex(x => new { x.UserId, x.StartedAt });
                e.Property(x => x.QuizId).IsRequired();
                e.Property(x => x.UserId).IsRequired();

                e.HasMany(x => x.Items)
                 .WithOne(x => x.QuizAttempt)
                 .HasForeignKey(x => x.QuizAttemptId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            b.Entity<QuizAttemptItem>(e =>
            {
                e.Property(x => x.QuestionText).IsRequired();
                e.Property(x => x.OrderIndex).IsRequired();

                e.HasMany(x => x.Answers)
                 .WithOne(x => x.QuizAttemptItem)
                 .HasForeignKey(x => x.QuizAttemptItemId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            b.Entity<QuizAttemptItemAnswer>(e =>
            {
                e.Property(x => x.Text).IsRequired();
            });

            b.Entity<QuizAttempt>(e =>
            {
                e.Property(x => x.Status).IsRequired();
                e.Property(x => x.CurrentIndex).HasDefaultValue(0);
                e.HasIndex(x => new { x.UserId, x.QuizId, x.Status }); // quick lookup for resumes
            });

        }
    }
}
