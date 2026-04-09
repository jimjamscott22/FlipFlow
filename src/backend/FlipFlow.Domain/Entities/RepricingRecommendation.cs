using FlipFlow.Domain.Common;

namespace FlipFlow.Domain.Entities;

public sealed class RepricingRecommendation : AuditableEntity
{
    public Guid ItemId { get; set; }

    public decimal PreviousPrice { get; set; }

    public decimal RecommendedPrice { get; set; }

    public string Reason { get; set; } = string.Empty;

    public DateTimeOffset EvaluatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public bool IsApplied { get; set; }

    public DateTimeOffset? AppliedAtUtc { get; set; }

    public Item? Item { get; set; }
}
