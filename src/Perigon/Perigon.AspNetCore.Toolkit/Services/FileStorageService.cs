using Amazon.S3.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Perigon.AspNetCore.Options;

namespace Perigon.AspNetCore.Toolkit.Services;

/// <summary>
/// 文件存储服务 - 处理本地和云存储文件的路径解析和下载
/// </summary>
public class FileStorageService
{
    private readonly AWSS3Service _s3Service;
    private readonly IHostEnvironment _environment;
    private readonly ComponentOption _componentOptions;
    private readonly ILogger<FileStorageService> _logger;
    private const string TempDirectoryName = "file-processor-temp";

    public FileStorageService(
        AWSS3Service s3Service,
        IHostEnvironment environment,
        IOptions<ComponentOption> componentOptions,
        ILogger<FileStorageService> logger
    )
    {
        _s3Service = s3Service;
        _environment = environment;
        _componentOptions = componentOptions.Value;
        _logger = logger;
    }

    /// <summary>
    /// 解析文件路径，根据存储类型返回本地文件路径或下载云文件
    /// </summary>
    /// <param name="filePath">文件路径或对象键</param>
    /// <param name="storageType">存储类型</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>本地文件路径</returns>
    public async Task<string?> ResolveFilePathAsync(
        string filePath,
        StorageType storageType,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return null;
        }

        if (storageType == StorageType.Local)
        {
            // 本地存储：返回完整路径
            var fullPath = Path.Combine(_environment.ContentRootPath, filePath);
            if (File.Exists(fullPath))
            {
                return fullPath;
            }

            _logger.LogWarning("Local file not found: {FilePath}", fullPath);
            return null;
        }
        else
        {
            // 云存储：下载到临时目录
            return await DownloadFromCloudAsync(filePath, cancellationToken);
        }
    }

    /// <summary>
    /// 从云存储下载文件到临时目录
    /// </summary>
    /// <param name="objectKey">对象键</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>临时文件路径</returns>
    private async Task<string?> DownloadFromCloudAsync(
        string objectKey,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var response = await _s3Service.GetObjectAsync(objectKey, cancellationToken);
            if (response == null)
            {
                _logger.LogWarning("Cloud file not found: {ObjectKey}", objectKey);
                return null;
            }

            // 创建临时文件
            var tempPath = Path.Combine(Path.GetTempPath(), TempDirectoryName, Guid.NewGuid().ToString());
            Directory.CreateDirectory(Path.GetDirectoryName(tempPath)!);

            // 下载文件
            await using var fileStream = File.Create(tempPath);
            await response.ResponseStream.CopyToAsync(fileStream, cancellationToken);

            _logger.LogInformation("Downloaded cloud file {ObjectKey} to {TempPath}", objectKey, tempPath);
            return tempPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file from cloud storage: {ObjectKey}", objectKey);
            return null;
        }
    }

    /// <summary>
    /// 获取文件流
    /// </summary>
    /// <param name="filePath">文件路径或对象键</param>
    /// <param name="storageType">存储类型</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>文件流</returns>
    public async Task<Stream?> GetFileStreamAsync(
        string filePath,
        StorageType storageType,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return null;
        }

        if (storageType == StorageType.Local)
        {
            var fullPath = Path.Combine(_environment.ContentRootPath, filePath);
            if (File.Exists(fullPath))
            {
                return File.OpenRead(fullPath);
            }

            _logger.LogWarning("Local file not found: {FilePath}", fullPath);
            return null;
        }
        else
        {
            var response = await _s3Service.GetObjectAsync(filePath, cancellationToken);
            return response?.ResponseStream;
        }
    }

    /// <summary>
    /// 清理临时文件
    /// </summary>
    /// <param name="tempFilePath">临时文件路径</param>
    public void CleanupTempFile(string? tempFilePath)
    {
        if (string.IsNullOrWhiteSpace(tempFilePath))
        {
            return;
        }

        try
        {
            // 验证文件路径在临时目录中
            var normalizedPath = Path.GetFullPath(tempFilePath);
            var tempDirectory = Path.GetFullPath(Path.Combine(Path.GetTempPath(), TempDirectoryName));
            
            if (!normalizedPath.StartsWith(tempDirectory, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Attempted to delete file outside temp directory: {TempFilePath}", tempFilePath);
                return;
            }

            if (File.Exists(normalizedPath))
            {
                File.Delete(normalizedPath);
                _logger.LogDebug("Cleaned up temp file: {TempFilePath}", normalizedPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error cleaning up temp file: {TempFilePath}", tempFilePath);
        }
    }
}
