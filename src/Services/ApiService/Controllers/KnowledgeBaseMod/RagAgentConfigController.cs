using KnowledgeBaseMod.Models.RagAgentConfigDtos;
namespace ApiService.Controllers.KnowledgeBaseMod;

/// <summary>
/// RAG 模型配置
/// </summary>
public class RagAgentConfigController(
    Localizer localizer,
    IUserContext user,
    ILogger<RagAgentConfigController> logger,
    RagAgentConfigManager manager
    ) : RestControllerBase<RagAgentConfigManager>(localizer, manager, user, logger)
{
    /// <summary>
    /// list RAG 模型配置 with page ✍️
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<RagAgentConfigItemDto>>> ListAsync(RagAgentConfigFilterDto filter)
    {
        return await _manager.FilterAsync(filter);
    }

    /// <summary>
    /// Add RAG 模型配置 ✍️
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<RagAgentConfig>> AddAsync(RagAgentConfigAddDto dto)
    {
        
        var entity = await _manager.AddAsync(dto);
        return CreatedAtRoute(null, new { id = entity.Id }, entity);
    }

    /// <summary>
    /// Update RAG 模型配置 ✍️
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPatch("{id}")]
    public async Task<bool> UpdateAsync([FromRoute] Guid id, RagAgentConfigUpdateDto dto)
    {
        return await _manager.EditAsync(id, dto) == 1;
    }

    /// <summary>
    /// Get RAG 模型配置 Detail ✍️
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<RagAgentConfigDetailDto?> DetailAsync([FromRoute] Guid id)
    {
        return await _manager.GetAsync(id);
    }

    /// <summary>
    /// Delete RAG 模型配置 ✍️
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteAsync([FromRoute] Guid id)
    {
        return await _manager.DeleteAsync([id], false);
    }
}