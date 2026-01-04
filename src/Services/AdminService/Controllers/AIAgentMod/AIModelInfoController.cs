using AIAgentMod.Models.AIModelInfoDtos;
namespace AdminService.Controllers.AIAgentMod;

/// <summary>
/// 模型信息
/// </summary>
public class AIModelInfoController(
    Localizer localizer,
    IUserContext user,
    ILogger<AIModelInfoController> logger,
    AIModelInfoManager manager
    ) : RestControllerBase<AIModelInfoManager>(localizer, manager, user, logger)
{
    /// <summary>
    /// list 模型信息 with page ✍️
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<AIModelInfoItemDto>>> ListAsync(AIModelInfoFilterDto filter)
    {
        return await _manager.FilterAsync(filter);
    }

    /// <summary>
    /// Add 模型信息 ✍️
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<AIModelInfo>> AddAsync(AIModelInfoAddDto dto)
    {
        
        var entity = await _manager.AddAsync(dto);
        return CreatedAtAction(nameof(DetailAsync), new { id = entity.Id }, entity);
    }

    /// <summary>
    /// Update 模型信息 ✍️
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPatch("{id}")]
    public async Task<bool> UpdateAsync([FromRoute] Guid id, AIModelInfoUpdateDto dto)
    {
        return await _manager.EditAsync(id, dto) == 1;
    }

    /// <summary>
    /// Get 模型信息 Detail ✍️
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<AIModelInfoDetailDto?> DetailAsync([FromRoute] Guid id)
    {
        return await _manager.GetAsync(id);
    }

    /// <summary>
    /// Delete 模型信息 ✍️
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteAsync([FromRoute] Guid id)
    {
        return await _manager.DeleteAsync([id], false);
    }
}