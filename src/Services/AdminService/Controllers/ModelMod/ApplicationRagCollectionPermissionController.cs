using ModelMod.Models.ApplicationRagCollectionPermissionDtos;

namespace AdminService.Controllers.ModelMod;

/// <summary>
/// 应用知识库关联管理
/// </summary>
public class ApplicationRagCollectionPermissionController(
    Localizer localizer,
    IUserContext user,
    ILogger<ApplicationRagCollectionPermissionController> logger,
    ApplicationRagCollectionPermissionManager manager
) : RestControllerBase<ApplicationRagCollectionPermissionManager>(localizer, manager, user, logger)
{
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<ApplicationRagCollectionPermissionItemDto>>> ListAsync(ApplicationRagCollectionPermissionFilterDto filter)
    {
        return await _manager.FilterAsync(filter);
    }

    [HttpPost]
    public async Task<ActionResult<ApplicationRagCollectionPermission>> AddAsync(ApplicationRagCollectionPermissionAddDto dto)
    {
        var entity = await _manager.AddAsync(dto);
        return CreatedAtRoute(null, new { id = entity.Id }, entity);
    }

    [HttpPatch("{id}")]
    public async Task<bool> UpdateAsync([FromRoute] Guid id, ApplicationRagCollectionPermissionUpdateDto dto)
    {
        return await _manager.EditAsync(id, dto) == 1;
    }

    [HttpGet("{id}")]
    public async Task<ApplicationRagCollectionPermissionDetailDto?> DetailAsync([FromRoute] Guid id)
    {
        return await _manager.GetAsync(id);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteAsync([FromRoute] Guid id)
    {
        return await _manager.DeleteAsync([id], false);
    }
}