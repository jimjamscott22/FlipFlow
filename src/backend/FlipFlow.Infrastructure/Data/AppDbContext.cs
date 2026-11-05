using FlipFlow.Domain.Common;
using FlipFlow.Domain.Entities;
using FlipFlow.Infrastructure.Data.Converters;
using Microsoft.EntityFrameworkCore;

namespace FlipFlow.Infrastructure.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Item> Items => Set<Item>();

    public DbSet<ItemPhoto> ItemPhotos => Set<ItemPhoto>();

    public DbSet<ListingDraft> ListingDrafts => Set<ListingDraft>();

    public DbSet<MarketplaceListing> MarketplaceListings => Set<MarketplaceListing>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // SQLite has no native decimal or DateTimeOffset, and the EF provider cannot
        // order or aggregate them reliably. Money is stored as integer cents and
        // timestamps as UTC ticks, once, here, so every entity gets it automatically
        // and SUM / ORDER BY / comparisons translate to SQL.
        configurationBuilder.Properties<decimal>().HaveConversion<DecimalToCentsConverter>();
        configurationBuilder.Properties<decimal?>().HaveConversion<NullableDecimalToCentsConverter>();
        configurationBuilder.Properties<DateTimeOffset>().HaveConversion<DateTimeOffsetToTicksConverter>();
        configurationBuilder.Properties<DateTimeOffset?>().HaveConversion<NullableDateTimeOffsetToTicksConverter>();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries<AuditableEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = DateTimeOffset.UtcNow;
            }

            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = DateTimeOffset.UtcNow;
            }
        }
    }
}
