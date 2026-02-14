using Perigon.AspNetCore.Options;

namespace SystemMod.Models.FileUploadDtos;

/// <summary>
/// 文件上传请求
/// </summary>
public class FileUploadRequestDto
{
    /// <summary>
    /// 上传的文件
    /// </summary>
    public required IFormFile File { get; set; }

    /// <summary>
    /// 文件分类（如：document, image, etc）
    /// </summary>
    public string Category { get; set; } = "default";

    /// <summary>
    /// 存储类型
    /// </summary>
    public StorageType? StorageType { get; set; }
}
