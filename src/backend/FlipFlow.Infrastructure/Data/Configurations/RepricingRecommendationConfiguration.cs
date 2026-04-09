using FlipFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlipFlow.Infrastructure.Data.Configurations;

public sealed class RepricingRecommendationConfiguration : IEntityTypeConfiguration<RepricingRecommendation>
{
    public void Configure(EntityTypeBuilder<RepricingRecommendation> builder)
    {
        builder.ToTable("repricing_recommendations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PreviousPrice)
            .HasPrecision(10, 2);

        builder.Property(x => x.RecommendedPrice)
            .HasPrecision(10, 2);

        builder.Property(x => x.Reason)
            .HasMaxLength(500)
            .IsRequired();
    }
}
