namespace FlipFlow.Application.Abstractions.Common;

public interface IFileStorageService
{
    Task<StoredFileResult> SaveFileAsync(
        Stream fileStream,
        string originalFileName,
        string contentType,
        CancellationToken cancellationToken = default);

    Task DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default);
}
