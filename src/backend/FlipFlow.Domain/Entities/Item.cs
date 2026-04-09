using FlipFlow.Domain.Common;
using FlipFlow.Domain.Enums;

namespace FlipFlow.Domain.Entities;

public sealed class Item : AuditableEntity
{
    public Guid OwnerUserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Brand { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public string Category { get; set; } = "Electronics";

    public ItemCondition Condition { get; set; } = ItemCondition.Good;

    public string Description { get; set; } = string.Empty;

    public decimal AskingPrice { get; set; }

    public ItemStatus Status { get; set; } = ItemStatus.Draft;

    public List<ItemPhoto> Photos { get; set; } = [];

    public List<ListingDraft> ListingDrafts { get; set; } = [];

    public List<MarketplaceListing> MarketplaceListings { get; set; } = [];

    public List<RepricingRecommendation> RepricingRecommendations { get; set; } = [];
}
