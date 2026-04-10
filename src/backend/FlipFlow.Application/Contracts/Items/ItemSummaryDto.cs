using FlipFlow.Domain.Enums;

namespace FlipFlow.Application.Contracts.Items;

public sealed class ItemSummaryDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Brand { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public ItemCondition Condition { get; set; }

    public decimal AskingPrice { get; set; }

    public ItemStatus Status { get; set; }

    public int PhotoCount { get; set; }

    public string? PrimaryPhotoRelativePath { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}
