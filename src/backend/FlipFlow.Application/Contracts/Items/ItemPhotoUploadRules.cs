namespace FlipFlow.Application.Contracts.Items;

public static class ItemPhotoUploadRules
{
    public const long MaxFileSizeBytes = 5 * 1024 * 1024;

    public const int MaxPhotosPerItem = 8;

    public static readonly IReadOnlySet<string> AllowedContentTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };
}
