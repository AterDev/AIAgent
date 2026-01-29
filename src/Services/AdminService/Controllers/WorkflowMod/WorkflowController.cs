using Entity.WorkflowMod;
using WorkflowMod.Managers;
using WorkflowMod.Models.WorkflowDtos;

namespace AdminService.Controllers.WorkflowMod;

/// <summary>
/// 工作流管理
/// </summary>
public class WorkflowController(
    Localizer localizer,
    IUserContext user,
    ILogger<WorkflowController> logger,
    WorkflowManager manager
) : RestControllerBase<WorkflowManager>(localizer, manager, user, logger)
{
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<WorkflowItemDto>>> ListAsync(WorkflowFilterDto filter)
    {
        return await _manager.FilterAsync(filter);
    }

    [HttpPost]
    public async Task<ActionResult<Workflow>> AddAsync(WorkflowAddDto dto)
    {
        var entity = await _manager.AddAsync(dto);
        return CreatedAtRoute(null, new { id = entity.Id }, entity);
    }

    [HttpPatch("{id}")]
    public async Task<bool> UpdateAsync([FromRoute] Guid id, WorkflowUpdateDto dto)
    {
        return await _manager.EditAsync(id, dto) == 1;
    }

    [HttpGet("{id}")]
    public async Task<WorkflowDetailDto?> DetailAsync([FromRoute] Guid id)
    {
        return await _manager.GetAsync(id);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteAsync([FromRoute] Guid id)
    {
        return await _manager.DeleteAsync([id], false);
    }
}
