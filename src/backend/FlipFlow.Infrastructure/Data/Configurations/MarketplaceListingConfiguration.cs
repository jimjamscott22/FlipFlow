using FlipFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlipFlow.Infrastructure.Data.Configurations;

public sealed class MarketplaceListingConfiguration : IEntityTypeConfiguration<MarketplaceListing>
{
    public void Configure(EntityTypeBuilder<MarketplaceListing> builder)
    {
        builder.ToTable("marketplace_listings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ListedPrice)
            .HasPrecision(10, 2);

        builder.Property(x => x.ExternalListingId)
            .HasMaxLength(200);

        builder.Property(x => x.ListingUrl)
            .HasMaxLength(500);

        builder.HasIndex(x => new { x.ItemId, x.Platform })
            .IsUnique();
    }
}
