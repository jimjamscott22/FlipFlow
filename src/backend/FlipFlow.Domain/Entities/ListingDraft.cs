using FlipFlow.Domain.Common;

namespace FlipFlow.Domain.Entities;

public sealed class ListingDraft : AuditableEntity
{
    public Guid ItemId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsAiGenerated { get; set; }

    public DateTimeOffset? GeneratedAtUtc { get; set; }

    public Item? Item { get; set; }
}
