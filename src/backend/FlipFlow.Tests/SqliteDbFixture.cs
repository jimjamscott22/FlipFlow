using FlipFlow.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FlipFlow.Tests;

/// <summary>
/// A real SQLite database held in memory for the life of the fixture. The connection must stay
/// open or the in-memory database disappears. The schema is created from the actual migrations,
/// so the tests also prove the migrations apply.
/// </summary>
public sealed class SqliteDbFixture : IDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteDbFixture()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        using var context = CreateContext();
        context.Database.Migrate();
    }

    /// <summary>Each call returns a fresh context, so reads do not come from the change tracker.</summary>
    public AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        return new AppDbContext(options);
    }

    public void Dispose() => _connection.Dispose();
}
