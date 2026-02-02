using SystemMod.Models.FileUploadDtos;
using Share.Models;
using Perigon.AspNetCore.Toolkit.Services;
using Perigon.AspNetCore.Options;
using Microsoft.Extensions.Options;

namespace AdminService.Controllers.SystemMod;

/// <summary>
/// 文件上传管理
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FileUploadController(
    ILogger<FileUploadController> logger,
    AWSS3Service s3Service,
    IOptions<ComponentOption> componentOptions,
    IWebHostEnvironment environment
) : ControllerBase
{
    private readonly ILogger<FileUploadController> _logger = logger;
    private readonly AWSS3Service _s3Service = s3Service;
    private readonly ComponentOption _componentOptions = componentOptions.Value;
    private readonly IWebHostEnvironment _environment = environment;

    /// <summary>
    /// 上传文件到 S3 对象存储或本地存储
    /// </summary>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<UploadResult>> UploadFileAsync(
        [FromForm] FileUploadRequestDto request,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            // 验证文件
            if (request.File == null || request.File.Length == 0)
            {
                return BadRequest(new { error = "No file uploaded" });
            }

            // 验证和清理 Category 参数，防止目录穿越
            var category = Path.GetFileName(request.Category?.Trim() ?? "default");
            if (string.IsNullOrEmpty(category) || category.Contains(".."))
            {
                category = "default";
            }

            // 使用请求中的 StorageType 或配置中的默认值
            var storageType = request.StorageType ?? _componentOptions.StorageType;

            // 生成安全的文件名
            var fileName = Path.GetFileNameWithoutExtension(request.File.FileName);
            var extension = Path.GetExtension(request.File.FileName);
            var safeFileName = $"{fileName}_{Guid.NewGuid()}{extension}";
            
            string filePath;
            string? url = null;

            if (storageType == StorageType.Local)
            {
                // 本地存储
                var uploadPath = Path.Combine(_environment.ContentRootPath, "uploads", category, DateTime.Now.ToString("yyyy/MM/dd"));
                Directory.CreateDirectory(uploadPath);

                var fullPath = Path.Combine(uploadPath, safeFileName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await request.File.CopyToAsync(stream, cancellationToken);
                }

                // 存储相对路径
                filePath = Path.Combine("uploads", category, DateTime.Now.ToString("yyyy/MM/dd"), safeFileName).Replace("\\", "/");
                _logger.LogInformation("File uploaded to local storage: {FilePath}", filePath);
            }
            else
            {
                // S3 存储
                var objectKey = $"{category}/{DateTime.Now:yyyy/MM/dd}/{safeFileName}";

                using var stream = request.File.OpenReadStream();
                var uploadSuccess = await _s3Service.UploadAsync(objectKey, stream, cancellationToken);

                if (!uploadSuccess)
                {
                    return BadRequest(new { error = "Failed to upload file to storage" });
                }

                filePath = objectKey;
                url = _s3Service.GetSignedUrl(objectKey, expiresSeconds: 86400); // 24 小时有效
                _logger.LogInformation("File uploaded to S3: {ObjectKey}", objectKey);
            }

            return Ok(new UploadResult
            {
                FilePath = filePath,
                Url = url,
                StorageType = storageType
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    [HttpDelete("delete")]
    public async Task<ActionResult> DeleteFileAsync(
        [FromQuery] string filePath,
        [FromQuery] StorageType? storageType = null,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return BadRequest(new { error = "File path is required" });
            }

            var actualStorageType = storageType ?? _componentOptions.StorageType;

            if (actualStorageType == StorageType.Local)
            {
                // 本地存储删除
                var fullPath = Path.Combine(_environment.ContentRootPath, filePath);
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                    _logger.LogInformation("File deleted from local storage: {FilePath}", filePath);
                    return Ok(new { message = "File deleted successfully" });
                }
                else
                {
                    return NotFound(new { error = "File not found" });
                }
            }
            else
            {
                // S3 删除
                var deleteSuccess = await _s3Service.DeleteAsync(filePath, cancellationToken);

                if (!deleteSuccess)
                {
                    return NotFound(new { error = "File not found or failed to delete" });
                }

                _logger.LogInformation("File deleted from S3: {ObjectKey}", filePath);
                return Ok(new { message = "File deleted successfully" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file");
            return BadRequest(new { error = ex.Message });
        }
    }
}
