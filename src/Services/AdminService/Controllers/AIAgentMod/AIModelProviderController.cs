using AIAgentMod.Models.AIModelProviderDtos;
namespace AdminService.Controllers.AIAgentMod;

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
    /// list AI模型提供商 with page ✍️
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<AIModelProviderItemDto>>> ListAsync(AIModelProviderFilterDto filter)
    {
        return await _manager.FilterAsync(filter);
    }

    /// <summary>
    /// Add AI模型提供商 ✍️
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<AIModelProvider>> AddAsync(AIModelProviderAddDto dto)
    {

        var entity = await _manager.AddAsync(dto);
        return CreatedAtAction(nameof(DetailAsync), new { id = entity.Id }, entity);
    }

    /// <summary>
    /// Update AI模型提供商 ✍️
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPatch("{id}")]
    public async Task<bool> UpdateAsync([FromRoute] Guid id, AIModelProviderUpdateDto dto)
    {
        return await _manager.EditAsync(id, dto) == 1;
    }

    /// <summary>
    /// Get AI模型提供商 Detail ✍️
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<AIModelProviderDetailDto?> DetailAsync([FromRoute] Guid id)
    {
        return await _manager.GetAsync(id);
    }

    /// <summary>
    /// Delete AI模型提供商 ✍️
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteAsync([FromRoute] Guid id)
    {
        return await _manager.DeleteAsync([id], false);
    }
}