using Amazon.S3;
using Amazon.S3.Model;
using Share.Abstraction;
using Share.Models;
using System.Net;

namespace SystemMod.Services;

/// <summary>
/// 文件存储服务实现
/// 根据活跃的存储服务商配置进行文件存储操作
/// </summary>
public class FileStorageService(
    StorageProviderManager storageProviderManager,
    IWebHostEnvironment environment,
    IHttpClientFactory httpClientFactory,
    ILogger<FileStorageService> logger
) : IFileStorageService
{
    private readonly StorageProviderManager _storageProviderManager = storageProviderManager;
    private readonly IWebHostEnvironment _environment = environment;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ILogger<FileStorageService> _logger = logger;

    // 缓存的S3客户端（根据提供商ID缓存）
    private AmazonS3Client? _cachedS3Client;
    private Guid? _cachedProviderId;
    private StorageProvider? _cachedProvider;

    /// <inheritdoc />
    public async Task<FileUploadResult> UploadAsync(
        Stream stream,
        string fileName,
        string category,
        CancellationToken cancellationToken = default)
    {
        var provider = await GetActiveProviderAsync();
        if (provider == null)
        {
            throw new BusinessException(Localizer.NoActiveStorageProviderConfigured);
        }

        var extension = Path.GetExtension(fileName);
        var safeFileName = $"{Guid.NewGuid()}{extension}";
        var datePath = DateTime.Now.ToString("yyyy/MM/dd");

        if (provider.IsCloud)
        {
            return await UploadToS3Async(provider, stream, safeFileName, category, datePath, cancellationToken);
        }
        else
        {
            return await UploadToLocalAsync(provider, stream, safeFileName, category, datePath, cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(
        string filePath,
        bool isCloud,
        CancellationToken cancellationToken = default)
    {
        if (isCloud)
        {
            var provider = await GetActiveProviderAsync();
            if (provider == null || !provider.IsCloud)
            {
                _logger.LogWarning("尝试删除云存储文件但没有活跃的云存储服务商");
                return false;
            }
            return await DeleteFromS3Async(provider, filePath, cancellationToken);
        }
        else
        {
            return DeleteFromLocal(filePath);
        }
    }

    /// <inheritdoc />
    public string? GetSignedUrl(string objectKey, int expiresSeconds = 86400)
    {
        if (_cachedS3Client == null || _cachedProvider == null || !_cachedProvider.IsCloud)
        {
            return null;
        }

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _cachedProvider.BucketName,
            Key = objectKey,
            Expires = DateTime.Now.AddSeconds(expiresSeconds),
        };
        return _cachedS3Client.GetPreSignedURL(request);
    }

    private async Task<StorageProvider?> GetActiveProviderAsync()
    {
        var provider = await _storageProviderManager.GetActiveProviderAsync();
        if (provider != null && _cachedProviderId != provider.Id)
        {
            _cachedProviderId = provider.Id;
            _cachedProvider = provider;
            if (provider.IsCloud)
            {
                _cachedS3Client = CreateS3Client(provider);
            }
        }
        return provider;
    }

    private static AmazonS3Client CreateS3Client(StorageProvider provider)
    {
        if (string.IsNullOrEmpty(provider.Endpoint) ||
            string.IsNullOrEmpty(provider.AccessKeyId) ||
            string.IsNullOrEmpty(provider.AccessKeySecret))
        {
            throw new BusinessException(Localizer.IncompleteCloudStorageConfiguration);
        }

        return new AmazonS3Client(
            provider.AccessKeyId,
            provider.AccessKeySecret,
            new AmazonS3Config { ServiceURL = provider.Endpoint }
        );
    }

    private async Task<FileUploadResult> UploadToS3Async(
        StorageProvider provider,
        Stream stream,
        string safeFileName,
        string category,
        string datePath,
        CancellationToken cancellationToken)
    {
        var objectKey = $"{category}/{datePath}/{safeFileName}";

        var client = _cachedS3Client ?? CreateS3Client(provider);
        var request = new PutObjectRequest
        {
            BucketName = provider.BucketName,
            Key = objectKey,
            InputStream = stream,
        };

        var response = await client.PutObjectAsync(request, cancellationToken);
        if (response.HttpStatusCode != HttpStatusCode.OK)
        {
            throw new IOException($"上传文件到S3失败: {response.HttpStatusCode}");
        }

        var url = GetSignedUrl(objectKey);
        _logger.LogInformation("文件上传到S3: {ObjectKey}", objectKey);

        return new FileUploadResult
        {
            FilePath = objectKey,
            Url = url,
            IsCloud = true,
            StorageProviderId = provider.Id
        };
    }

    private async Task<FileUploadResult> UploadToLocalAsync(
        StorageProvider provider,
        Stream stream,
        string safeFileName,
        string category,
        string datePath,
        CancellationToken cancellationToken)
    {

        if (provider.Path == null)
        {
            _logger.LogWarning("Local storage path not configured: {StorageProviderId}", provider.Id);
            throw new BusinessException(Localizer.LocalStoragePathNotConfigured);
        }

        var uploadPath = Path.Combine(provider.Path, category, datePath);
        Directory.CreateDirectory(uploadPath);

        var fullPath = Path.Combine(uploadPath, safeFileName);
        using (var fileStream = new FileStream(fullPath, FileMode.Create))
        {
            await stream.CopyToAsync(fileStream, cancellationToken);
        }

        // 存储相对路径
        var relativePath = Path.Combine("uploads", category, datePath, safeFileName).Replace("\\", "/");
        _logger.LogInformation("文件上传到本地存储: {FilePath}", relativePath);

        return new FileUploadResult
        {
            FilePath = relativePath,
            Url = null,
            IsCloud = false,
            StorageProviderId = provider.Id
        };
    }

    private async Task<bool> DeleteFromS3Async(
        StorageProvider provider,
        string objectKey,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = _cachedS3Client ?? CreateS3Client(provider);
            var request = new DeleteObjectRequest
            {
                BucketName = provider.BucketName,
                Key = objectKey,
            };
            var response = await client.DeleteObjectAsync(request, cancellationToken);
            _logger.LogInformation("文件从S3删除: {ObjectKey}", objectKey);
            return response.HttpStatusCode == HttpStatusCode.OK ||
                   response.HttpStatusCode == HttpStatusCode.NoContent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除S3文件失败: {ObjectKey}", objectKey);
            return false;
        }
    }

    private bool DeleteFromLocal(string filePath)
    {
        var uploadsBasePath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, "uploads"));
        var fullPath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, filePath));

        // 安全检查：确保文件路径在uploads目录下
        if (!fullPath.StartsWith(uploadsBasePath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) &&
            !fullPath.Equals(uploadsBasePath, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("尝试删除uploads目录外的文件: {FilePath}", filePath);
            return false;
        }

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogInformation("文件从本地存储删除: {FilePath}", filePath);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public async Task<string?> DownloadFileAsync(
        Guid storageProviderId,
        string objectKey,
        CancellationToken cancellationToken = default)
    {
        var provider = await _storageProviderManager.FindAsync(storageProviderId);
        if (provider == null)
        {
            _logger.LogWarning("存储提供商未找到: {StorageProviderId}", storageProviderId);
            return null;
        }

        // 本地存储：直接返回文件路径
        if (!provider.IsCloud)
        {
            if (provider.Path == null)
            {
                _logger.LogWarning("Local storage path not configured: {StorageProviderId}", storageProviderId);
                return null;
            }
            var fullPath = Path.GetFullPath(Path.Combine(provider.Path, objectKey));

            if (File.Exists(fullPath))
            {
                return fullPath;
            }
            _logger.LogWarning("File not exist: {FilePath}", fullPath);
            return null;
        }

        // 云存储：下载到临时文件
        try
        {
            // 确保 S3 客户端已初始化（_cachedS3Client 仅在 GetActiveProviderAsync 中初始化）
            var client = _cachedS3Client;
            if (client == null || _cachedProviderId != provider.Id)
            {
                client = CreateS3Client(provider);
                _cachedS3Client = client;
                _cachedProviderId = provider.Id;
                _cachedProvider = provider;
            }

            var request = new GetPreSignedUrlRequest
            {
                BucketName = provider.BucketName,
                Key = objectKey,
                Expires = DateTime.Now.AddSeconds(600), // 10分钟有效期
            };
            var signedUrl = client.GetPreSignedURL(request);

            // 下载文件到临时文件
            var tempFilePath = Path.Combine(Path.GetTempPath(), $"rag_{Guid.NewGuid()}{Path.GetExtension(objectKey)}");
            using (var httpClient = _httpClientFactory.CreateClient())
            {
                using (var response = await httpClient.GetAsync(signedUrl, cancellationToken))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError("下载S3文件失败: {ObjectKey}, StatusCode: {StatusCode}", objectKey, response.StatusCode);
                        return null;
                    }

                    using (var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken))
                    using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
                    {
                        await contentStream.CopyToAsync(fileStream, cancellationToken);
                    }
                }
            }

            _logger.LogInformation("S3文件已下载到临时位置: {TempPath}", tempFilePath);
            return tempFilePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "下载S3文件异常: {ObjectKey}", objectKey);
            return null;
        }
    }

    /// <inheritdoc />
    public void CleanupTempFile(string tempFilePath)
    {
        try
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
                _logger.LogInformation("临时文件已删除: {TempPath}", tempFilePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除临时文件失败: {TempPath}", tempFilePath);
        }
    }
}


