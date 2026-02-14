using Share.Models;

namespace Share.Abstraction;

/// <summary>
/// 文件存储服务抽象
/// </summary>
public interface IFileStorageService
{
    Task<FileUploadResult> UploadAsync(
        Stream stream,
        string fileName,
        string category,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        string filePath,
        bool isCloud,
        CancellationToken cancellationToken = default);

    string? GetSignedUrl(string objectKey, int expiresSeconds = 86400);

    Task<string?> DownloadFileAsync(Guid storageProviderId, string objectKey, CancellationToken cancellationToken = default);

    void CleanupTempFile(string tempFilePath);
}

/// <summary>
/// 存储提供商查询抽象
/// </summary>
public interface IStorageProviderQuery
{
    Task<StorageProviderInfo?> GetProviderAsync(Guid storageProviderId, CancellationToken cancellationToken = default);

    Task<StorageProviderInfo?> GetActiveProviderAsync(CancellationToken cancellationToken = default);
}