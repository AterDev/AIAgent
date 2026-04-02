using Entity.KnowledgeBaseMod;
using KnowledgeBaseMod.Managers;
using KnowledgeBaseMod.Models.RagDocumentDtos;
using CoreMod.Services;
using Share.Models;

namespace AdminService.Controllers.KnowledgeBaseMod;

/// <summary>
/// 文档管理（仅管理，不包含处理逻辑）
/// </summary>
public class RagDocumentController(
    Localizer localizer,
    IUserContext user,
    ILogger<RagDocumentController> logger,
    RagDocumentManager manager,
    NatsRagMessagePublisher messagePublisher
) : RestControllerBase<RagDocumentManager>(localizer, manager, user, logger)
{
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<RagDocumentItemDto>>> ListAsync(RagDocumentFilterDto filter)
    {
        return await _manager.FilterAsync(filter);
    }

    [HttpPost]
    public async Task<ActionResult<RagDocument>> AddAsync(RagDocumentAddDto dto)
    {
        var entity = await _manager.AddAsync(dto);
        return CreatedAtRoute(null, new { id = entity.Id }, entity);
    }

    [HttpPatch("{id}")]
    public async Task<bool> UpdateAsync([FromRoute] Guid id, RagDocumentUpdateDto dto)
    {
        return await _manager.EditAsync(id, dto) == 1;
    }

    [HttpGet("{id}")]
    public async Task<RagDocumentDetailDto?> DetailAsync([FromRoute] Guid id)
    {
        return await _manager.GetAsync(id);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteAsync([FromRoute] Guid id)
    {
        return await _manager.DeleteAsync([id], false);
    }

    /// <summary>
    /// 手动触发文档入队解析/向量化
    /// </summary>
    [HttpPost("{id}/ingest")]
    public async Task<ActionResult<bool>> IngestAsync(
        [FromRoute] Guid id,
        RagDocumentIngestDto dto,
        CancellationToken cancellationToken = default)
    {
        var document = await _manager.GetAsync(id);
        if (document == null)
        {
            return NotFound();
        }

        _ = dto;
        _ = cancellationToken;

        await _manager.QueueIngestionAsync(id);
        return Accepted(true);
    }

    /// <summary>
    /// 手动触发文档解析
    /// </summary>
    [HttpPost("{id}/parse")]
    public async Task<ActionResult> TriggerParseAsync([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        var document = await _manager.GetAsync(id);
        if (document == null)
        {
            return NotFound();
        }

        // 发布消息到 NATS 队列，由 FileProcessorService 异步处理
        var message = new RagIngestionMessage
        {
            DocumentId = id,
            TenantId = _user.TenantId,
            CollectionId = document.CollectionId,
            FilePath = document.FilePath ?? string.Empty,
            FileType = document.FileType ?? "txt",
            DocumentName = document.Name ?? string.Empty,
            FileName = document.FileName ?? string.Empty,
            StorageProviderId = document.StorageProviderId
        };
        
        await messagePublisher.PublishAsync(message, cancellationToken);
        
        return Ok();
    }
}
