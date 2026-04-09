using FlipFlow.Application.Abstractions.Common;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace FlipFlow.Infrastructure.Files;

public sealed class LocalFileStorageService(
    IHostEnvironment environment,
    IOptions<LocalFileStorageOptions> options) : IFileStorageService
{
    private readonly LocalFileStorageOptions _options = options.Value;

    public async Task<StoredFileResult> SaveFileAsync(
        Stream fileStream,
        string originalFileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(originalFileName);
        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var relativeDirectory = _options.RootPath.Replace("\\", "/").Trim('/');
        var relativePath = $"{relativeDirectory}/{storedFileName}";
        var rootPath = Path.Combine(environment.ContentRootPath, relativeDirectory);

        Directory.CreateDirectory(rootPath);

        var fullPath = Path.Combine(rootPath, storedFileName);

        await using var outputStream = File.Create(fullPath);
        await fileStream.CopyToAsync(outputStream, cancellationToken);
        await outputStream.FlushAsync(cancellationToken);

        var fileInfo = new FileInfo(fullPath);

        return new StoredFileResult(
            originalFileName,
            storedFileName,
            $"/{relativePath}",
            contentType,
            fileInfo.Length);
    }
}
