using FlipFlow.Domain.Common;

namespace FlipFlow.Domain.Entities;

public sealed class ItemPhoto : AuditableEntity
{
    public Guid ItemId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string StoredFileName { get; set; } = string.Empty;

    public string RelativePath { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    public int SortOrder { get; set; }

    public bool IsPrimary { get; set; }

    public Item? Item { get; set; }
}
