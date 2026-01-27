using Entity.ModelMod;
using ModelMod.Managers;
using ModelMod.Models.ApplicationModelPermissionDtos;

namespace AdminService.Controllers.ModelMod;

/// <summary>
/// 应用模型权限管理
/// </summary>
public class ApplicationModelPermissionController(
    Localizer localizer,
    IUserContext user,
    ILogger<ApplicationModelPermissionController> logger,
    ApplicationModelPermissionManager manager
) : RestControllerBase<ApplicationModelPermissionManager>(localizer, manager, user, logger)
{
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<ApplicationModelPermissionItemDto>>> ListAsync(ApplicationModelPermissionFilterDto filter)
    {
        return await _manager.FilterAsync(filter);
    }

    [HttpPost]
    public async Task<ActionResult<ApplicationModelPermission>> AddAsync(ApplicationModelPermissionAddDto dto)
    {
        var entity = await _manager.AddAsync(dto);
        return CreatedAtRoute(nameof(DetailAsync), new { id = entity.Id }, entity);
    }

    [HttpPatch("{id}")]
    public async Task<bool> UpdateAsync([FromRoute] Guid id, ApplicationModelPermissionUpdateDto dto)
    {
        return await _manager.EditAsync(id, dto) == 1;
    }

    [HttpGet("{id}")]
    public async Task<ApplicationModelPermissionDetailDto?> DetailAsync([FromRoute] Guid id)
    {
        return await _manager.GetAsync(id);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteAsync([FromRoute] Guid id)
    {
        return await _manager.DeleteAsync([id], false);
    }
}
