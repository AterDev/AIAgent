using Entity.ModelMod;
using ModelMod.Managers;
using ModelMod.Models.ApplicationToolPermissionDtos;

namespace AdminService.Controllers.ModelMod;

/// <summary>
/// 应用工具权限管理
/// </summary>
public class ApplicationToolPermissionController(
    Localizer localizer,
    IUserContext user,
    ILogger<ApplicationToolPermissionController> logger,
    ApplicationToolPermissionManager manager
) : RestControllerBase<ApplicationToolPermissionManager>(localizer, manager, user, logger)
{
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<ApplicationToolPermissionItemDto>>> ListAsync(ApplicationToolPermissionFilterDto filter)
    {
        return await _manager.FilterAsync(filter);
    }

    [HttpPost]
    public async Task<ActionResult<ApplicationToolPermission>> AddAsync(ApplicationToolPermissionAddDto dto)
    {
        var entity = await _manager.AddAsync(dto);
        return CreatedAtRoute(nameof(DetailAsync), new { id = entity.Id }, entity);
    }

    [HttpPatch("{id}")]
    public async Task<bool> UpdateAsync([FromRoute] Guid id, ApplicationToolPermissionUpdateDto dto)
    {
        return await _manager.EditAsync(id, dto) == 1;
    }

    [HttpGet("{id}")]
    public async Task<ApplicationToolPermissionDetailDto?> DetailAsync([FromRoute] Guid id)
    {
        return await _manager.GetAsync(id);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteAsync([FromRoute] Guid id)
    {
        return await _manager.DeleteAsync([id], false);
    }
}
