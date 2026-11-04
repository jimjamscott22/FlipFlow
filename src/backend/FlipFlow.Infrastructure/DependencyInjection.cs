using FlipFlow.Application.Abstractions.Common;
using FlipFlow.Application.Abstractions.Content;
using FlipFlow.Application.Abstractions.Items;
using FlipFlow.Application.Abstractions.Pricing;
using FlipFlow.Infrastructure.Data;
using FlipFlow.Infrastructure.Data.Seed;
using FlipFlow.Infrastructure.Files;
using FlipFlow.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FlipFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = ResolveConnectionString(configuration);

        services.Configure<LocalFileStorageOptions>(configuration.GetSection(LocalFileStorageOptions.SectionName));
        services.Configure<DemoDataOptions>(configuration.GetSection(DemoDataOptions.SectionName));

        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IItemService, ItemService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IListingContentGenerator, MockListingContentGenerator>();
        services.AddScoped<IPricingSuggestionService, RuleBasedPricingSuggestionService>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<ApplicationDbInitializer>();

        return services;
    }

    /// <summary>
    /// A relative SQLite path (the default, <c>data/flipflow.db</c>) is anchored to the
    /// application content root, not the process working directory, so the database is
    /// found no matter where the app is started from. The folder is created if missing.
    /// </summary>
    private static string ResolveConnectionString(IConfiguration configuration)
    {
        var raw = configuration.GetConnectionString("DefaultConnection") ?? "Data Source=data/flipflow.db";
        var builder = new SqliteConnectionStringBuilder(raw);

        if (!Path.IsPathRooted(builder.DataSource) && builder.DataSource != ":memory:")
        {
            var contentRoot = configuration[HostDefaults.ContentRootKey] ?? Directory.GetCurrentDirectory();
            builder.DataSource = Path.GetFullPath(Path.Combine(contentRoot, builder.DataSource));
        }

        if (builder.DataSource != ":memory:")
        {
            var directory = Path.GetDirectoryName(builder.DataSource);

            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        return builder.ToString();
    }
}
