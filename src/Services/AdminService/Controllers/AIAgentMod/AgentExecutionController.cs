using AIAgentMod.Models.AgentExecutionDtos;
using AIAgentMod.Services;

namespace AdminService.Controllers.AIAgentMod;

/// <summary>
/// Agent 执行管理
/// </summary>
public class AgentExecutionController(
    Localizer localizer,
    IUserContext user,
    ILogger<AgentExecutionController> logger,
    AgentExecutionManager manager
) : RestControllerBase<AgentExecutionManager>(localizer, manager, user, logger)
{
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<AgentExecutionItemDto>>> ListAsync(AgentExecutionFilterDto filter)
    {
        return await _manager.FilterAsync(filter);
    }

    [HttpPost]
    public async Task<ActionResult<AgentExecution>> AddAsync(AgentExecutionAddDto dto)
    {
        var entity = await _manager.AddAsync(dto);
        return CreatedAtRoute(nameof(DetailAsync), new { id = entity.Id }, entity);
    }

    [HttpPatch("{id}")]
    public async Task<bool> UpdateAsync([FromRoute] Guid id, AgentExecutionUpdateDto dto)
    {
        return await _manager.EditAsync(id, dto) == 1;
    }

    [HttpGet("{id}")]
    public async Task<AgentExecutionDetailDto?> DetailAsync([FromRoute] Guid id)
    {
        return await _manager.GetAsync(id);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteAsync([FromRoute] Guid id)
    {
        return await _manager.DeleteAsync([id], false);
    }

    /// <summary>
    /// 入队执行 Agent
    /// </summary>
    [HttpPost("{id}/enqueue")]
    public async Task<ActionResult<bool>> EnqueueAsync([FromRoute] Guid id, AgentExecuteRequestDto dto, [FromServices] IAgentExecutionQueue queue)
    {
        await queue.EnqueueAsync(new AgentExecutionTask(id, dto.ApplicationId, dto.InputJson));
        return Accepted(true);
    }
}
