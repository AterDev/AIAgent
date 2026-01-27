using Entity.WorkflowMod;
using WorkflowMod.Managers;
using WorkflowMod.Models.WorkflowExecutionDtos;
using WorkflowMod.Services;

namespace AdminService.Controllers.WorkflowMod;

/// <summary>
/// 工作流执行管理
/// </summary>
public class WorkflowExecutionController(
    Localizer localizer,
    IUserContext user,
    ILogger<WorkflowExecutionController> logger,
    WorkflowExecutionManager manager
) : RestControllerBase<WorkflowExecutionManager>(localizer, manager, user, logger)
{
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<WorkflowExecutionItemDto>>> ListAsync(WorkflowExecutionFilterDto filter)
    {
        return await _manager.FilterAsync(filter);
    }

    [HttpPost]
    public async Task<ActionResult<WorkflowExecution>> AddAsync(WorkflowExecutionAddDto dto)
    {
        var entity = await _manager.AddAsync(dto);
        return CreatedAtRoute(nameof(DetailAsync), new { id = entity.Id }, entity);
    }

    [HttpPatch("{id}")]
    public async Task<bool> UpdateAsync([FromRoute] Guid id, WorkflowExecutionUpdateDto dto)
    {
        return await _manager.EditAsync(id, dto) == 1;
    }

    [HttpGet("{id}")]
    public async Task<WorkflowExecutionDetailDto?> DetailAsync([FromRoute] Guid id)
    {
        return await _manager.GetAsync(id);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteAsync([FromRoute] Guid id)
    {
        return await _manager.DeleteAsync([id], false);
    }

    /// <summary>
    /// 入队执行工作流
    /// </summary>
    [HttpPost("{id}/enqueue")]
    public async Task<ActionResult<bool>> EnqueueAsync([FromRoute] Guid id, [FromServices] IWorkflowQueue queue)
    {
        await queue.EnqueueAsync(new WorkflowTask(id));
        return Accepted(true);
    }
}
