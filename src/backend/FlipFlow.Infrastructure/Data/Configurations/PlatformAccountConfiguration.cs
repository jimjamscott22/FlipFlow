using FlipFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlipFlow.Infrastructure.Data.Configurations;

public sealed class PlatformAccountConfiguration : IEntityTypeConfiguration<PlatformAccount>
{
    public void Configure(EntityTypeBuilder<PlatformAccount> builder)
    {
        builder.ToTable("platform_accounts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DisplayName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.ExternalAccountId)
            .HasMaxLength(200);

        builder.HasIndex(x => new { x.OwnerUserId, x.Platform })
            .IsUnique();
    }
}
