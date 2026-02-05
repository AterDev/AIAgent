using WorkflowMod.Managers;
using WorkflowMod.Models.WorkflowDtos;
using WorkflowMod.Models.WorkflowExecutionDtos;
using WorkflowMod.Services;

namespace ApiService.Controllers.OpenPlatform;

/// <summary>
/// Open platform workflows
/// </summary>
[ApiController]
[Route("api/v1/workflows")]
public class WorkflowsController(
    WorkflowManager manager,
    WorkflowExecutionManager executionManager,
    IUserContext user,
    ILogger<WorkflowsController> logger
) : OpenApiControllerBase<WorkflowManager>(manager, user, logger)
{
    private readonly WorkflowExecutionManager _executionManager = executionManager;

    [HttpPost("filter")]
    public async Task<ActionResult<PageList<WorkflowItemDto>>> ListAsync(WorkflowFilterDto filter)
    {
        return await _manager.FilterAsync(filter);
    }

    [HttpGet("{id}")]
    public async Task<WorkflowDetailDto?> DetailAsync([FromRoute] Guid id)
    {
        return await _manager.GetAsync(id);
    }

    /// <summary>
    /// Execute workflow
    /// </summary>
    [HttpPost("{id}/execute")]
    public async Task<ActionResult<object>> ExecuteAsync(
        [FromRoute] Guid id,
        WorkflowExecuteRequestDto dto,
        [FromServices] WorkflowQueue queue
    )
    {
        var execution = await _executionManager.AddAsync(new WorkflowExecutionAddDto
        {
            WorkflowId = id,
            InputJson = dto.InputJson,
            Status = Entity.WorkflowMod.WorkflowExecutionStatus.Running,
        });

        await queue.EnqueueAsync(new WorkflowTask(execution.Id));
        return Accepted(new { executionId = execution.Id });
    }
}
