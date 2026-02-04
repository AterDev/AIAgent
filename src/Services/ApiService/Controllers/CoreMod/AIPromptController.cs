using CoreMod.Models.AIPromptDtos;
namespace ApiService.Controllers.CoreMod;

/// <summary>
/// 提示词
/// </summary>
public class AIPromptController(
    Localizer localizer,
    IUserContext user,
    ILogger<AIPromptController> logger,
    AIPromptManager manager
    ) : RestControllerBase<AIPromptManager>(localizer, manager, user, logger)
{
    /// <summary>
    /// list 提示词 with page ✍️
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<AIPromptItemDto>>> ListAsync(AIPromptFilterDto filter)
    {
        return await _manager.FilterAsync(filter);
    }

    /// <summary>
    /// Add 提示词 ✍️
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<AIPrompt>> AddAsync(AIPromptAddDto dto)
    {
        
        var entity = await _manager.AddAsync(dto);
        return CreatedAtRoute(null, new { id = entity.Id }, entity);
    }

    /// <summary>
    /// Update 提示词 ✍️
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPatch("{id}")]
    public async Task<bool> UpdateAsync([FromRoute] Guid id, AIPromptUpdateDto dto)
    {
        return await _manager.EditAsync(id, dto) == 1;
    }

    /// <summary>
    /// Get 提示词 Detail ✍️
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<AIPromptDetailDto?> DetailAsync([FromRoute] Guid id)
    {
        return await _manager.GetAsync(id);
    }

    /// <summary>
    /// Delete 提示词 ✍️
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteAsync([FromRoute] Guid id)
    {
        return await _manager.DeleteAsync([id], false);
    }
}