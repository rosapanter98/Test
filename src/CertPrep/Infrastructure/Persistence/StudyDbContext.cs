using CertPrep.Features.ExamCatalog;
using CertPrep.Features.Practice;
using CertPrep.Features.Rewards;
using Microsoft.EntityFrameworkCore;

namespace CertPrep.Infrastructure.Persistence;

public sealed class StudyDbContext(DbContextOptions<StudyDbContext> options) : DbContext(options)
{
    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<ExamObjective> ExamObjectives => Set<ExamObjective>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<AnswerChoice> AnswerChoices => Set<AnswerChoice>();
    public DbSet<PracticeSession> PracticeSessions => Set<PracticeSession>();
    public DbSet<PracticeSessionItem> PracticeSessionItems => Set<PracticeSessionItem>();
    public DbSet<PracticeSessionChoice> PracticeSessionChoices => Set<PracticeSessionChoice>();
    public DbSet<RewardLedgerEntry> RewardLedgerEntries => Set<RewardLedgerEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Exam>(entity =>
        {
            entity.Property(x => x.Provider).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Code).HasMaxLength(30).IsRequired().UseCollation("NOCASE");
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Summary).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.ContentVersion).HasMaxLength(40).IsRequired();
            entity.HasIndex(x => new { x.Provider, x.Code }).IsUnique();
            entity.HasMany(x => x.Objectives).WithOne(x => x.Exam).HasForeignKey(x => x.ExamId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.Questions).WithOne(x => x.Exam).HasForeignKey(x => x.ExamId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ExamObjective>(entity =>
        {
            entity.Property(x => x.ContentKey).HasMaxLength(100).IsRequired().UseCollation("NOCASE");
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.HasIndex(x => new { x.ExamId, x.ContentKey }).IsUnique();
            entity.HasIndex(x => new { x.ExamId, x.SortOrder });
            entity.HasMany(x => x.Questions).WithOne(x => x.Objective).HasForeignKey(x => x.ExamObjectiveId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.Property(x => x.ContentKey).HasMaxLength(100).IsRequired().UseCollation("NOCASE");
            entity.Property(x => x.Prompt).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.Kind).HasConversion<string>().HasMaxLength(30);
            entity.Property(x => x.Difficulty).HasConversion<string>().HasMaxLength(30);
            entity.Property(x => x.Explanation).HasMaxLength(4000).IsRequired();
            entity.Property(x => x.SourceName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.SourceUrl).HasMaxLength(1000).IsRequired();
            entity.HasIndex(x => new { x.ExamId, x.ContentKey }).IsUnique();
            entity.HasMany(x => x.Choices).WithOne(x => x.Question).HasForeignKey(x => x.QuestionId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AnswerChoice>(entity =>
        {
            entity.Property(x => x.Text).HasMaxLength(1000).IsRequired();
            entity.HasIndex(x => new { x.QuestionId, x.SortOrder }).IsUnique();
        });

        modelBuilder.Entity<PracticeSession>(entity =>
        {
            entity.Property(x => x.ExamCodeSnapshot).HasMaxLength(30).IsRequired();
            entity.Property(x => x.ExamTitleSnapshot).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Scope).HasConversion<string>().HasMaxLength(30);
            entity.Property(x => x.Mode).HasConversion<string>().HasMaxLength(30);
            entity.Property(x => x.Purpose).HasConversion<string>().HasMaxLength(30);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
            entity.HasOne(x => x.Exam).WithMany().HasForeignKey(x => x.ExamId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(x => x.Items).WithOne(x => x.PracticeSession).HasForeignKey(x => x.PracticeSessionId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => x.StartedAt);
        });

        modelBuilder.Entity<PracticeSessionItem>(entity =>
        {
            entity.Property(x => x.ExamCodeSnapshot).HasMaxLength(30).IsRequired();
            entity.Property(x => x.ExamTitleSnapshot).HasMaxLength(200).IsRequired();
            entity.Property(x => x.ObjectiveContentKeySnapshot).HasMaxLength(100).IsRequired().UseCollation("NOCASE");
            entity.Property(x => x.ObjectiveName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Prompt).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.Kind).HasConversion<string>().HasMaxLength(30);
            entity.Property(x => x.Difficulty).HasConversion<string>().HasMaxLength(30);
            entity.Property(x => x.Explanation).HasMaxLength(4000).IsRequired();
            entity.Property(x => x.SourceName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.SourceUrl).HasMaxLength(1000).IsRequired();
            entity.HasOne(x => x.SourceExam).WithMany().HasForeignKey(x => x.SourceExamId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.PracticeSessionId, x.OrderIndex }).IsUnique();
            entity.HasMany(x => x.Choices).WithOne(x => x.PracticeSessionItem).HasForeignKey(x => x.PracticeSessionItemId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PracticeSessionChoice>(entity =>
        {
            entity.Property(x => x.Text).HasMaxLength(1000).IsRequired();
            entity.HasIndex(x => new { x.PracticeSessionItemId, x.SortOrder }).IsUnique();
        });

        modelBuilder.Entity<RewardLedgerEntry>(entity =>
        {
            entity.Property(x => x.ExamCode).HasMaxLength(30).IsRequired();
            entity.Property(x => x.ObjectiveContentKey).HasMaxLength(100).UseCollation("NOCASE");
            entity.Property(x => x.Kind).HasConversion<string>().HasMaxLength(30);
            entity.Property(x => x.IdempotencyKey).HasMaxLength(240).IsRequired().UseCollation("NOCASE");
            entity.Property(x => x.Description).HasMaxLength(240).IsRequired();
            entity.HasIndex(x => x.IdempotencyKey).IsUnique();
            entity.HasIndex(x => new { x.PracticeSessionId, x.RuleVersion });
            entity.HasOne(x => x.PracticeSession)
                .WithMany()
                .HasForeignKey(x => x.PracticeSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
