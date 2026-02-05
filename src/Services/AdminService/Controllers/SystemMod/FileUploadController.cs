using Share.Models;
using Share.Services;
using SystemMod.Models.FileUploadDtos;
using SystemMod.Services;

namespace AdminService.Controllers.SystemMod;

/// <summary>
/// 文件上传管理
/// </summary>
public class FileUploadController(
    Localizer localizer,
    ILogger<FileUploadController> logger,
    IFileStorageService fileStorageService
) : RestControllerBase(localizer)
{
    private readonly ILogger<FileUploadController> _logger = logger;
    private readonly IFileStorageService _fileStorageService = fileStorageService;

    // File type allowlist
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".docx", ".txt", ".md", ".json", ".xml",
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp",
        ".xls", ".xlsx", ".csv",
        ".zip", ".rar", ".7z"
    };

    // File size limits: PDF 50MB, others 20MB
    private static readonly Dictionary<string, long> FileSizeLimits = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".pdf", 50 * 1024 * 1024 },     // 50 MB for PDF
        { "*", 20 * 1024 * 1024 }         // 20 MB for others (default)
    };

    /// <summary>
    /// 上传文件到存储服务商配置的存储位置
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
                return BadRequest(Localizer.NoFileUploaded);
            }

            // Validate file type (extension allowlist)
            var extension = Path.GetExtension(request.File.FileName);
            if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
            {
                return BadRequest(Localizer.FileTypeNotAllowed, extension);
            }

            // Validate file size based on type
            var maxSize = FileSizeLimits.TryGetValue(extension, out var size) ? size : FileSizeLimits["*"];
            if (request.File.Length > maxSize)
            {
                return BadRequest(Localizer.FileSizeExceededLimit);
            }

            // Validate and sanitize category parameter to prevent path traversal
            var category = SanitizeCategory(request.Category);

            using var stream = request.File.OpenReadStream();
            var result = await _fileStorageService.UploadAsync(stream, request.File.FileName, category, cancellationToken);

            _logger.LogInformation("File uploaded: {FilePath}, IsCloud: {IsCloud}", result.FilePath, result.IsCloud);

            return Ok(new UploadResult
            {
                FilePath = result.FilePath,
                Url = result.Url,
                StorageProviderId = result.StorageProviderId
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Storage provider not configured");
            return Problem(detail: Localizer.NoActiveStorageProviderConfigured);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return Problem(detail: Localizer.BadRequest);
        }
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    [HttpDelete("delete")]
    public async Task<ActionResult> DeleteFileAsync(
        [FromQuery] string filePath,
        [FromQuery] bool? isCloud = null,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return BadRequest(Localizer.FilePathRequired);
            }

            // 默认根据路径格式判断是否为云存储
            var actualIsCloud = isCloud ?? !filePath.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase);

            var success = await _fileStorageService.DeleteAsync(filePath, actualIsCloud, cancellationToken);

            if (!success)
            {
                return NotFound();
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file");
            return Problem(detail: Localizer.BadRequest);
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
