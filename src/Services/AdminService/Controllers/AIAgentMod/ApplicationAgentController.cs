using AIAgentMod.Models.AIAgentDtos;

namespace AdminService.Controllers.AIAgentMod;

/// <summary>
/// 应用侧 Agent
/// </summary>
public class ApplicationAgentController(
    Localizer localizer,
    IUserContext user,
    ILogger<ApplicationAgentController> logger,
    ApplicationAgentManager manager
) : RestControllerBase<ApplicationAgentManager>(localizer, manager, user, logger)
{
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<AIAgentItemDto>>> ListAsync(AIAgentFilterDto filter)
    {
        return await _manager.FilterAsync(filter);
    }

    [HttpPost]
    public async Task<ActionResult<ApplicationAgent>> AddAsync(AIAgentAddDto dto)
    {
        var entity = await _manager.AddAsync(dto);
        return CreatedAtRoute(null, new { id = entity.Id }, entity);
    }

    [HttpPatch("{id}")]
    public async Task<bool> UpdateAsync([FromRoute] Guid id, AIAgentUpdateDto dto)
    {
        return await _manager.EditAsync(id, dto) == 1;
    }

    [HttpGet("{id}")]
    public async Task<AIAgentDetailDto?> DetailAsync([FromRoute] Guid id)
    {
        return await _manager.GetAsync(id);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteAsync([FromRoute] Guid id)
    {
        return await _manager.DeleteAsync([id], false);
    }
}