using ModelMod.Models.AIModelProviderDtos;

namespace AdminService.Controllers.ModelMod;

/// <summary>
/// AI模型提供商
/// </summary>
public class AIModelProviderController(
    Localizer localizer,
    IUserContext user,
    ILogger<AIModelProviderController> logger,
    AIModelProviderManager manager
) : RestControllerBase<AIModelProviderManager>(localizer, manager, user, logger)
{
    /// <summary>
    /// list AI模型提供商 with page
    /// </summary>
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<AIModelProviderItemDto>>> ListAsync(AIModelProviderFilterDto filter)
    {
        return await _manager.FilterAsync(filter);
    }

    /// <summary>
    /// Add AI模型提供商
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<AIModelProvider>> AddAsync(AIModelProviderAddDto dto)
    {
        var entity = await _manager.AddAsync(dto);
        return CreatedAtRoute(null, new { id = entity.Id }, entity);
    }

    /// <summary>
    /// Update AI模型提供商
    /// </summary>
    [HttpPatch("{id}")]
    public async Task<bool> UpdateAsync([FromRoute] Guid id, AIModelProviderUpdateDto dto)
    {
        return await _manager.EditAsync(id, dto) == 1;
    }

    /// <summary>
    /// Get AI模型提供商 Detail
    /// </summary>
    [HttpGet("{id}")]
    public async Task<AIModelProviderDetailDto?> DetailAsync([FromRoute] Guid id)
    {
        return await _manager.GetAsync(id);
    }

    /// <summary>
    /// Delete AI模型提供商
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteAsync([FromRoute] Guid id)
    {
        return await _manager.DeleteAsync([id], false);
    }
}
