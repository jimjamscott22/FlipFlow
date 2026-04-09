using FlipFlow.Domain.Common;
using FlipFlow.Domain.Enums;

namespace FlipFlow.Domain.Entities;

public sealed class MarketplaceListing : AuditableEntity
{
    public Guid ItemId { get; set; }

    public MarketplacePlatform Platform { get; set; }

    public MarketplaceListingStatus Status { get; set; } = MarketplaceListingStatus.Draft;

    public decimal ListedPrice { get; set; }

    public string? ExternalListingId { get; set; }

    public string? ListingUrl { get; set; }

    public DateTimeOffset? PublishedAtUtc { get; set; }

    public DateTimeOffset? SoldAtUtc { get; set; }

    public Item? Item { get; set; }
}
