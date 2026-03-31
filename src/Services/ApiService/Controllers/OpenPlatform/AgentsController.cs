using AIAgentMod.Managers;
using AIAgentMod.Models.AgentExecutionDtos;
using AIAgentMod.Models.AIAgentDtos;
using AIAgentMod.Services;
using Share.Exceptions;

namespace ApiService.Controllers.OpenPlatform;

/// <summary>
/// Open platform agents
/// </summary>
[ApiController]
[Route("api/v1/agents")]
public class AgentsController(
    AIAgentManager agentManager,
    AgentExecutionManager executionManager,
    IUserContext user,
    ILogger<AgentsController> logger
) : OpenApiControllerBase<AIAgentManager>(agentManager, user, logger)
{
    private readonly AgentExecutionManager _executionManager = executionManager;

    [HttpPost("filter")]
    public async Task<ActionResult<PageList<AIAgentItemDto>>> ListAsync(AIAgentFilterDto filter)
    {
        return await _manager.FilterAsync(filter);
    }

    [HttpGet("{id}")]
    public async Task<AIAgentDetailDto?> DetailAsync([FromRoute] Guid id)
    {
        return await _manager.GetAsync(id);
    }

    /// <summary>
    /// Execute agent
    /// </summary>
    [HttpPost("{id}/execute")]
    public async Task<ActionResult<object>> ExecuteAsync(
        [FromRoute] Guid id,
        AgentExecuteRequestDto dto,
        [FromServices] AgentExecutionQueue queue
    )
    {
        var applicationId = _user.IsRole(WebConst.Application)
            ? _user.UserId
            : dto.ApplicationId;

        if (!applicationId.HasValue || applicationId == Guid.Empty)
        {
            throw new BusinessException("Application is required");
        }

        var execution = await _executionManager.AddAsync(new AgentExecutionAddDto
        {
            AgentId = id,
            InputJson = dto.InputJson,
            Status = Entity.AIAgentMod.AgentExecutionStatus.Running,
        });

        await queue.EnqueueAsync(new AgentExecutionTask(execution.Id, applicationId.Value, dto.InputJson));
        return Accepted(new { executionId = execution.Id });
    }
}
