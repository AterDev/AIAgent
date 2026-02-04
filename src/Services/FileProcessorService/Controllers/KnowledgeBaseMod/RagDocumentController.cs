using Entity.KnowledgeBaseMod;
using KnowledgeBaseMod.Managers;
using KnowledgeBaseMod.Models.RagDocumentDtos;
using KnowledgeBaseMod.Models.DocumentParsingDtos;
using KnowledgeBaseMod.Services;
using Share.Services;
using Share.Models;

namespace FileProcessorService.Controllers.KnowledgeBaseMod;

/// <summary>
/// 文档处理服务
/// </summary>
public class RagDocumentController(
    Localizer localizer,
    IUserContext user,
    ILogger<RagDocumentController> logger,
    RagDocumentManager manager,
    DocumentParsingResultManager parsingResultManager,
    IDocumentParser documentParser,
    NatsRagMessagePublisher messagePublisher
) : RestControllerBase<RagDocumentManager>(localizer, manager, user, logger)
{
    /// <summary>
    /// 解析并向量化文档
    /// </summary>
    [HttpPost("{id}/ingest")]
    public async Task<ActionResult<bool>> IngestAsync(
        [FromRoute] Guid id,
        RagDocumentIngestDto dto,
        CancellationToken cancellationToken
    )
    {
        var document = await _manager.FindAsync(id);
        if (document == null || document.TenantId != _user.TenantId)
        {
            return NotFound();
        }

        // 发布消息到 NATS JetStream 队列
        var message = new RagIngestionMessage
        {
            DocumentId = document.Id,
            TenantId = document.TenantId,
            CollectionId = document.CollectionId,
            FilePath = document.FilePath,
            FileType = document.FileType,
            DocumentName = document.Name,
            FileName = document.FileName,
            StorageProviderId = document.StorageProviderId
        };

        await messagePublisher.PublishAsync(message, cancellationToken);
        return Accepted(true);
    }

    /// <summary>
    /// 解析文档
    /// </summary>
    [HttpPost("{id}/parse")]
    public async Task<ActionResult> ParseDocumentAsync(
        [FromRoute] Guid id,
        [FromBody] DocumentParseRequestDto request
    )
    {
        var document = await _manager.FindAsync(id);
        if (document == null || document.TenantId != _user.TenantId)
        {
            return NotFound();
        }

        try
        {
            // 验证文件路径存在
            if (!System.IO.File.Exists(request.FilePath))
            {
                return BadRequest(new { error = "File not found at specified path" });
            }

            // 确定文档格式
            var format = GetDocumentFormat(request.FileName);
            document.FileType = System.IO.Path.GetExtension(request.FileName).TrimStart('.');
            document.FilePath = request.FilePath;
            
            // 解析文档
            var parseResult = await documentParser.ParseAsync(document, null, default);
            
            // 保存解析结果
            var result = await parsingResultManager.CreateAsync(id, format, parseResult.Text);
            
            return Ok(new { id = result.Id, wordCount = result.WordCount, status = result.ParsingStatus });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing document {DocumentId}", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 获取解析结果
    /// </summary>
    [HttpGet("{id}/parsing-results")]
    public async Task<ActionResult<List<DocumentParsingResultDto>>> GetParsingResultsAsync([FromRoute] Guid id)
    {
        var results = await parsingResultManager.GetByDocumentAsync(id);
        var dtos = results.Select(r => new DocumentParsingResultDto
        {
            Id = r.Id,
            RagDocumentId = r.RagDocumentId,
            ParsingStatus = r.ParsingStatus,
            WordCount = r.WordCount,
            PageCount = r.PageCount,
            DurationMs = r.DurationMs,
            CompletedTime = r.CompletedTime,
            CreatedAt = r.CreatedTime.DateTime
        }).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// 获取最新的解析结果
    /// </summary>
    [HttpGet("{id}/latest-parsing-result")]
    public async Task<ActionResult<DocumentParsingResultDto?>> GetLatestParsingResultAsync([FromRoute] Guid id)
    {
        var result = await parsingResultManager.GetLatestAsync(id);
        if (result == null)
        {
            return NotFound();
        }

        var dto = new DocumentParsingResultDto
        {
            Id = result.Id,
            RagDocumentId = result.RagDocumentId,
            ParsingStatus = result.ParsingStatus,
            WordCount = result.WordCount,
            PageCount = result.PageCount,
            DurationMs = result.DurationMs,
            CompletedTime = result.CompletedTime,
            CreatedAt = result.CreatedTime.DateTime
        };

        return Ok(dto);
    }

    private static DocumentFormatType GetDocumentFormat(string fileName)
    {
        var ext = Path.GetExtension(fileName ?? "").ToLower();
        return ext switch
        {
            ".pdf" => DocumentFormatType.Pdf,
            ".doc" or ".docx" => DocumentFormatType.Word,
            ".xls" or ".xlsx" => DocumentFormatType.Excel,
            ".ppt" or ".pptx" => DocumentFormatType.PowerPoint,
            ".md" => DocumentFormatType.Markdown,
            ".json" => DocumentFormatType.Json,
            ".xml" => DocumentFormatType.Xml,
            _ => DocumentFormatType.Text
        };
    }
}
