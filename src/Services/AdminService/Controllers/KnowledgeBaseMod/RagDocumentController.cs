using Entity.KnowledgeBaseMod;
using KnowledgeBaseMod.Managers;
using KnowledgeBaseMod.Models.RagDocumentDtos;

namespace AdminService.Controllers.KnowledgeBaseMod;

/// <summary>
/// 文档管理（仅管理，不包含处理逻辑）
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
}
