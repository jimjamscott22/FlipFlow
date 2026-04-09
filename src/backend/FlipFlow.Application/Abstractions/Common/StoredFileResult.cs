namespace FlipFlow.Application.Abstractions.Common;

public sealed record StoredFileResult(
    string FileName,
    string StoredFileName,
    string RelativePath,
    string ContentType,
    long FileSizeBytes);
