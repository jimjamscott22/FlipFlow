using FlipFlow.Domain.Common;
using FlipFlow.Domain.Enums;

namespace FlipFlow.Domain.Entities;

public sealed class PlatformAccount : AuditableEntity
{
    public Guid OwnerUserId { get; set; }

    public MarketplacePlatform Platform { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string? ExternalAccountId { get; set; }

    public bool IsConnected { get; set; }
}
