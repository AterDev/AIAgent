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

    // File type allowlist
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".txt", ".md", ".json", ".xml",
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp",
        ".xls", ".xlsx", ".csv",
        ".zip", ".rar", ".7z"
    };

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
            // Validate file
            if (request.File == null || request.File.Length == 0)
            {
                return BadRequest(new { error = "No file uploaded" });
            }

            // Validate file type (extension allowlist)
            var extension = Path.GetExtension(request.File.FileName);
            if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
            {
                return BadRequest(new { error = $"File type '{extension}' is not allowed" });
            }

            // Validate and sanitize category parameter to prevent path traversal
            var category = SanitizeCategory(request.Category);

            // Use requested StorageType or configuration default
            var storageType = request.StorageType ?? _componentOptions.StorageType;

            // Generate safe filename using only GUID and validated extension
            var safeFileName = $"{Guid.NewGuid()}{extension}";
            
            string filePath;
            string? url = null;

            if (storageType == StorageType.Local)
            {
                // Local storage
                var uploadPath = Path.Combine(_environment.ContentRootPath, "uploads", category, DateTime.Now.ToString("yyyy/MM/dd"));
                Directory.CreateDirectory(uploadPath);

                var fullPath = Path.Combine(uploadPath, safeFileName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await request.File.CopyToAsync(stream, cancellationToken);
                }

                // Store relative path
                filePath = Path.Combine("uploads", category, DateTime.Now.ToString("yyyy/MM/dd"), safeFileName).Replace("\\", "/");
                _logger.LogInformation("File uploaded to local storage: {FilePath}", filePath);
            }
            else
            {
                // S3 storage
                var objectKey = $"{category}/{DateTime.Now:yyyy/MM/dd}/{safeFileName}";

                using var stream = request.File.OpenReadStream();
                var uploadSuccess = await _s3Service.UploadAsync(objectKey, stream, cancellationToken);

                if (!uploadSuccess)
                {
                    return BadRequest(new { error = "Failed to upload file to storage" });
                }

                filePath = objectKey;
                url = _s3Service.GetSignedUrl(objectKey, expiresSeconds: 86400); // 24 hour validity
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
                // Local storage
                // Normalize paths for case-insensitive comparison on all platforms
                var uploadsBasePath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, "uploads"));
                var fullPath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, filePath));
                
                // Security check: ensure resolved path is within uploads directory
                // Use case-insensitive comparison to work on both case-sensitive and case-insensitive filesystems
                if (!fullPath.StartsWith(uploadsBasePath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) &&
                    !fullPath.Equals(uploadsBasePath, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Attempted to delete file outside uploads directory: {FilePath}", filePath);
                    return BadRequest(new { error = "Invalid file path" });
                }
                
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
                // S3 storage
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

    /// <summary>
    /// Sanitize category name to prevent path traversal attacks
    /// </summary>
    private static string SanitizeCategory(string? category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return "default";
        }

        // Path.GetFileName extracts only the final path component, neutralizing
        // path traversal attempts (e.g., '../../../etc/passwd' becomes 'passwd')
        category = Path.GetFileName(category.Trim());
        
        // Return default if result is empty
        return string.IsNullOrEmpty(category) ? "default" : category;
    }
}
