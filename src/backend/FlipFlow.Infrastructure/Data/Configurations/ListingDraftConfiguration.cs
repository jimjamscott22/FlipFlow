using FlipFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlipFlow.Infrastructure.Data.Configurations;

public sealed class ListingDraftConfiguration : IEntityTypeConfiguration<ListingDraft>
{
    public void Configure(EntityTypeBuilder<ListingDraft> builder)
    {
        builder.ToTable("listing_drafts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(5000)
            .IsRequired();
    }
}
