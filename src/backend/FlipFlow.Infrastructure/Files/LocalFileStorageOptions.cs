namespace FlipFlow.Infrastructure.Files;

public sealed class LocalFileStorageOptions
{
    public const string SectionName = "LocalFileStorage";

    public string RootPath { get; set; } = "storage/uploads";

    public string PublicBasePath { get; set; } = "/uploads";
}
