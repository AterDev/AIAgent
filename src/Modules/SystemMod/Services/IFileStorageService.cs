namespace SystemMod.Services;

/// <summary>
/// 文件存储服务接口
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="stream">文件流</param>
    /// <param name="fileName">原始文件名</param>
    /// <param name="category">分类目录</param>
    /// <param name="cancellationToken"></param>
    /// <returns>上传结果（文件路径、URL、是否云存储）</returns>
    Task<FileUploadResult> UploadAsync(
        Stream stream,
        string fileName,
        string category,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="isCloud">是否为云存储</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> DeleteAsync(
        string filePath,
        bool isCloud,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取文件签名URL （仅云存储有效）
    /// </summary>
    /// <param name="objectKey">对象键</param>
    /// <param name="expiresSeconds">有效期（秒）</param>
    /// <returns></returns>
    string? GetSignedUrl(string objectKey, int expiresSeconds = 86400);

    /// <summary>
    /// 下载文件（从存储提供商）
    /// </summary>
    /// <remarks>
    /// 本地存储：直接返回文件路径
    /// 云存储：下载到临时文件并返回路径
    /// 调用者需要负责清理临时文件
    /// </remarks>
    /// <param name="storageProviderId">存储提供商ID</param>
    /// <param name="objectKey">对象键（文件路径）</param>
    /// <param name="cancellationToken"></param>
    /// <returns>本地文件路径（可能是临时文件）</returns>
    Task<string?> DownloadFileAsync(Guid storageProviderId, string objectKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// 清理临时文件
    /// </summary>
    /// <param name="tempFilePath">临时文件路径</param>
    void CleanupTempFile(string tempFilePath);
}

/// <summary>
/// 文件上传结果
/// </summary>
public class FileUploadResult
{
    /// <summary>
    /// 文件路径（本地相对路径或云存储对象键）
    /// </summary>
    public required string FilePath { get; set; }

    /// <summary>
    /// 访问URL（云存储签名URL）
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// 是否为云存储
    /// </summary>
    public bool IsCloud { get; set; }

    /// <summary>
    /// 存储服务商ID
    /// </summary>
    public Guid StorageProviderId { get; set; }
}
