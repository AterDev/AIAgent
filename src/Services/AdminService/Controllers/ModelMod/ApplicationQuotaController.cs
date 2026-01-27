using ModelMod.Models.ApplicationQuotaDtos;

namespace AdminService.Controllers.ModelMod;

/// <summary>
/// 应用配额管理
/// </summary>
public class ApplicationQuotaController(
    Localizer localizer,
    IUserContext user,
    ILogger<ApplicationQuotaController> logger,
    ApplicationQuotaManager manager
) : RestControllerBase<ApplicationQuotaManager>(localizer, manager, user, logger)
{
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<ApplicationQuotaItemDto>>> ListAsync(ApplicationQuotaFilterDto filter)
    {
        return await _manager.FilterAsync(filter);
    }

    [HttpPost]
    public async Task<ActionResult<ApplicationQuota>> AddAsync(ApplicationQuotaAddDto dto)
    {
        var entity = await _manager.AddAsync(dto);
        return CreatedAtRoute(nameof(DetailAsync), new { id = entity.Id }, entity);
    }

    [HttpPatch("{id}")]
    public async Task<bool> UpdateAsync([FromRoute] Guid id, ApplicationQuotaUpdateDto dto)
    {
        return await _manager.EditAsync(id, dto) == 1;
    }

    [HttpGet("{id}")]
    public async Task<ApplicationQuotaDetailDto?> DetailAsync([FromRoute] Guid id)
    {
        return await _manager.GetAsync(id);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteAsync([FromRoute] Guid id)
    {
        return await _manager.DeleteAsync([id], false);
    }
}
