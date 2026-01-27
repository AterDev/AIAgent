using KnowledgeBaseMod.Managers;
using KnowledgeBaseMod.Models.RagDocumentDtos;
using KnowledgeBaseMod.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiService.Controllers.OpenPlatform;

/// <summary>
/// Open platform RAG documents
/// </summary>
[ApiController]
[Route("api/v1/rag/documents")]
public class RagDocumentsController(
    RagDocumentManager manager,
    IUserContext user,
    ILogger<RagDocumentsController> logger
) : OpenApiControllerBase<RagDocumentManager>(manager, user, logger)
{
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<RagDocumentItemDto>>> ListAsync(RagDocumentFilterDto filter)
    {
        return await _manager.FilterAsync(filter);
    }

    [HttpPost]
    public async Task<ActionResult<Entity.KnowledgeBaseMod.RagDocument>> AddAsync(RagDocumentAddDto dto)
    {
        var entity = await _manager.AddAsync(dto);
        return CreatedAtRoute(nameof(DetailAsync), new { id = entity.Id }, entity);
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
        [FromServices] IRagIngestionQueue ingestionQueue
    )
    {
        await ingestionQueue.EnqueueAsync(new RagIngestionTask(id, dto.ContentText));
        return Accepted(true);
    }
}
