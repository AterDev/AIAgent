using Entity.WorkflowMod;
using WorkflowMod.Managers;
using WorkflowMod.Models;
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
        return CreatedAtRoute(null, new { id = entity.Id }, entity);
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

    /// <summary>
    /// 获取执行进度
    /// </summary>
    [HttpGet("{id}/progress")]
    public async Task<ActionResult<WorkflowExecutionProgress>> GetProgressAsync([FromRoute] Guid id, [FromServices] IWorkflowExecutor executor)
    {
        var progress = await executor.GetProgressAsync(id);
        if (progress is null)
        {
            return NotFound();
        }
        return Ok(progress);
    }

    /// <summary>
    /// 断点续传执行
    /// </summary>
    [HttpPost("{id}/resume")]
    public async Task<ActionResult<bool>> ResumeAsync([FromRoute] Guid id, [FromQuery] int fromStep, [FromServices] IWorkflowExecutor executor)
    {
        var result = await executor.ResumeAsync(id, fromStep);
        return Ok(result);
    }

    /// <summary>
    /// 重试失败的执行
    /// </summary>
    [HttpPost("{id}/retry")]
    public async Task<ActionResult<bool>> RetryAsync([FromRoute] Guid id, [FromServices] IWorkflowExecutor executor)
    {
        var result = await executor.RetryAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// 取消执行
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<bool>> CancelAsync([FromRoute] Guid id, [FromServices] IWorkflowExecutor executor)
    {
        var result = await executor.CancelAsync(id);
        return Ok(result);
    }
}
