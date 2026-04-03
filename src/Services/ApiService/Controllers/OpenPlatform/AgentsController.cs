using AIAgentMod.Managers;
using AIAgentMod.Models.AgentExecutionDtos;
using AIAgentMod.Models.AIAgentDtos;
using AIAgentMod.Services;
using Entity.AIAgentMod;
using Share.Exceptions;

namespace ApiService.Controllers.OpenPlatform;

/// <summary>
/// Open platform agents
/// </summary>
[ApiController]
[Route("api/v1/agents")]
public class AgentsController(
    ApplicationAgentManager applicationAgentManager,
    AIAgentManager publicAgentManager,
    AgentExecutionManager executionManager,
    IUserContext user,
    ILogger<AgentsController> logger
) : OpenApiControllerBase<ApplicationAgentManager>(applicationAgentManager, user, logger)
{
    private readonly AIAgentManager _publicAgentManager = publicAgentManager;
    private readonly AgentExecutionManager _executionManager = executionManager;

    [HttpPost]
    public async Task<ActionResult<ApplicationAgent>> AddAsync(AIAgentAddDto dto)
    {
        var entity = await _manager.AddAsync(dto);
        return CreatedAtRoute(null, new { id = entity.Id }, entity);
    }

    [HttpPost("filter")]
    public async Task<ActionResult<PageList<AIAgentItemDto>>> ListAsync(AIAgentFilterDto filter)
    {
        return await _manager.FilterAsync(filter);
    }

    [HttpPost("templates/filter")]
    public async Task<ActionResult<PageList<AIAgentItemDto>>> ListTemplatesAsync(AIAgentFilterDto filter)
    {
        return await _publicAgentManager.FilterPublicTemplatesAsync(filter);
    }

    [HttpGet("{id}")]
    public async Task<AIAgentDetailDto?> DetailAsync([FromRoute] Guid id)
    {
        return await _manager.GetAsync(id);
    }

    [HttpPatch("{id}")]
    public async Task<bool> UpdateAsync([FromRoute] Guid id, AIAgentUpdateDto dto)
    {
        return await _manager.EditAsync(id, dto) == 1;
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteAsync([FromRoute] Guid id)
    {
        return await _manager.DeleteAsync([id], false);
    }

    [HttpPost("templates/{id}/clone")]
    public async Task<ActionResult<ApplicationAgent>> CloneTemplateAsync([FromRoute] Guid id)
    {
        if (!_user.IsRole(WebConst.Application))
        {
            throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
        }

        var entity = await _manager.ClonePublicAsync(id, _user.UserId);
        return CreatedAtRoute(null, new { id = entity.Id }, entity);
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

        if (!await _manager.HasPermissionAsync(id))
        {
            throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
        }

        var execution = await _executionManager.AddAsync(new AgentExecutionAddDto
        {
            AgentId = id,
            IsApplicationAgent = true,
            InputJson = dto.InputJson,
            Status = Entity.AIAgentMod.AgentExecutionStatus.Running,
        });

        await queue.EnqueueAsync(new AgentExecutionTask(execution.Id, applicationId.Value, dto.InputJson));
        return Accepted(new { executionId = execution.Id });
    }
}
