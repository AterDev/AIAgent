using SystemMod.Models.FileUploadDtos;
using Share.Models;
using Perigon.AspNetCore.Toolkit.Services;

namespace AdminService.Controllers.SystemMod;

/// <summary>
/// 文件上传管理
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FileUploadController(
    ILogger<FileUploadController> logger,
    AWSS3Service s3Service
) : ControllerBase
{
    private readonly ILogger<FileUploadController> _logger = logger;
    private readonly AWSS3Service _s3Service = s3Service;

    /// <summary>
    /// 上传文件到 S3 对象存储
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

            // 生成安全的对象 Key
            var fileName = Path.GetFileNameWithoutExtension(request.File.FileName);
            var extension = Path.GetExtension(request.File.FileName);
            var safeFileName = $"{fileName}_{Guid.NewGuid()}{extension}";
            var objectKey = $"{request.Category}/{DateTime.Now:yyyy/MM/dd}/{safeFileName}";

            // 上传到 S3
            using var stream = request.File.OpenReadStream();
            var uploadSuccess = await _s3Service.UploadAsync(objectKey, stream, cancellationToken);

            if (!uploadSuccess)
            {
                return BadRequest(new { error = "Failed to upload file to storage" });
            }

            _logger.LogInformation("File uploaded to S3: {ObjectKey}", objectKey);

            // 生成访问 URL（带有时间限制的签名 URL）
            var signedUrl = _s3Service.GetSignedUrl(objectKey, expiresSeconds: 86400); // 24 小时有效

            return Ok(new UploadResult
            {
                FilePath = objectKey,
                Url = signedUrl
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to S3");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 删除 S3 中的文件
    /// </summary>
    [HttpDelete("delete")]
    public async Task<ActionResult> DeleteFileAsync(
        [FromQuery] string objectKey,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            if (string.IsNullOrEmpty(objectKey))
            {
                return BadRequest(new { error = "Object key is required" });
            }

            // 从 S3 删除文件
            var deleteSuccess = await _s3Service.DeleteAsync(objectKey, cancellationToken);

            if (!deleteSuccess)
            {
                return NotFound(new { error = "File not found or failed to delete" });
            }

            _logger.LogInformation("File deleted from S3: {ObjectKey}", objectKey);
            return Ok(new { message = "File deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from S3");
            return BadRequest(new { error = ex.Message });
        }
    }
}
