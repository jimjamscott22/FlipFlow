namespace FlipFlow.Application.Contracts.Items;

public sealed class ItemPhotoDto
{
    public Guid Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    public int SortOrder { get; set; }

    public bool IsPrimary { get; set; }

    public string RelativePath { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; }
}
