using FlipFlow.Domain.Enums;

namespace FlipFlow.Application.Contracts.Items;

public sealed class SaveItemRequestDto
{
    public string Title { get; set; } = string.Empty;

    public string Brand { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public ItemCondition Condition { get; set; } = ItemCondition.Good;

    public string Description { get; set; } = string.Empty;

    public decimal AskingPrice { get; set; }

    public ItemStatus Status { get; set; } = ItemStatus.Draft;
}
