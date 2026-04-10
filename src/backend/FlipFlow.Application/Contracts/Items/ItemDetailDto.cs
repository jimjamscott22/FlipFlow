using FlipFlow.Domain.Enums;

namespace FlipFlow.Application.Contracts.Items;

public sealed class ItemDetailDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Brand { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public ItemCondition Condition { get; set; }

    public string Description { get; set; } = string.Empty;

    public decimal AskingPrice { get; set; }

    public ItemStatus Status { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }

    public List<ItemPhotoDto> Photos { get; set; } = [];
}
