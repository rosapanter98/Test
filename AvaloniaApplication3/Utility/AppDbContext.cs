using AvaloniaApplication3.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace AvaloniaApplication3.Utility
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
                optionsBuilder.UseSqlite(DbConfig.GetConnectionString());
            }
            Console.WriteLine($"Using database at: {DbConfig.GetDbPath()}");
        }

        protected override void OnModelCreating(ModelBuilder b)
        {
            // --- User ---
            b.Entity<User>(e =>
            {
                e.Property(u => u.Username).IsRequired().HasMaxLength(100).UseCollation("NOCASE");
                e.HasIndex(u => u.Username).IsUnique();
            });

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

            b.Entity<Answer>(e =>
            {
                e.Property(x => x.Text).IsRequired();
            });

            // --- Attempts snapshot model ---
            b.Entity<QuizAttempt>(e =>
            {
                // Store DateTimeOffset as UTC DateTime in SQLite (enables ORDER BY, etc.)
                e.Property(x => x.StartedAt)
                 .HasConversion(
                     v => v.UtcDateTime,
                     v => new DateTimeOffset(DateTime.SpecifyKind(v, DateTimeKind.Utc)));

                e.Property(x => x.CompletedAt)
                 .HasConversion(
                     v => v.HasValue ? v.Value.UtcDateTime : (DateTime?)null,
                     v => v.HasValue
                          ? new DateTimeOffset(DateTime.SpecifyKind(v.Value, DateTimeKind.Utc))
                          : null);

                e.Property(x => x.QuizId).IsRequired();
                e.Property(x => x.UserId).IsRequired();
                e.Property(x => x.Status).IsRequired();
                e.Property(x => x.CurrentIndex).HasDefaultValue(0);

                e.HasIndex(x => new { x.UserId, x.StartedAt });
                e.HasIndex(x => new { x.UserId, x.QuizId, x.Status }); // quick lookup for resumes

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
        }
    }
}
