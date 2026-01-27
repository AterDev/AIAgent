using Entity.KnowledgeBaseMod;
using KnowledgeBaseMod.Managers;
using KnowledgeBaseMod.Models.RagDocumentDtos;
using KnowledgeBaseMod.Services;

namespace AdminService.Controllers.KnowledgeBaseMod;

/// <summary>
/// 文档管理
/// </summary>
public class RagDocumentController(
    Localizer localizer,
    IUserContext user,
    ILogger<RagDocumentController> logger,
    RagDocumentManager manager
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
        return CreatedAtRoute(nameof(DetailAsync), new { id = entity.Id }, entity);
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
    /// 解析并向量化文档
    /// </summary>
    [HttpPost("{id}/ingest")]
    public async Task<ActionResult<bool>> IngestAsync(
        [FromRoute] Guid id,
        RagDocumentIngestDto dto,
        [FromServices] IRagIngestionQueue ingestionQueue,
        CancellationToken cancellationToken
    )
    {
        await ingestionQueue.EnqueueAsync(new RagIngestionTask(id, dto.ContentText));
        return Accepted(true);
    }
}
