using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FlipFlow.Infrastructure.Data;

/// <summary>Used only by <c>dotnet ef</c> at design time.</summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlite("Data Source=design-time.db");

        return new AppDbContext(optionsBuilder.Options);
    }
}
