using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CertPrep.Infrastructure.Persistence;

public sealed class StudyDbContextFactory : IDesignTimeDbContextFactory<StudyDbContext>
{
    public StudyDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<StudyDbContext>()
            .UseSqlite($"Data Source={AppPaths.GetDatabasePath()}")
            .Options;

        return new StudyDbContext(options);
    }
}
