using SystemMod.Models.SystemConfigDtos;
namespace AdminService.Controllers.SystemMod;

/// <summary>
/// 系统配置
/// </summary>
public class SystemConfigController(
    Localizer localizer,
    IUserContext user,
    ILogger<SystemConfigController> logger,
    SystemConfigManager manager
    ) : RestControllerBase<SystemConfigManager>(localizer, manager, user, logger)
{
    /// <summary>
    /// list 系统配置 with page ✍️
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<SystemConfigItemDto>>> ListAsync(SystemConfigFilterDto filter)
    {
        return await _manager.FilterAsync(filter);
    }

    /// <summary>
    /// Add 系统配置 ✍️
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<SystemConfig>> AddAsync(SystemConfigAddDto dto)
    {
        
        var entity = await _manager.AddAsync(dto);
        return CreatedAtRoute(nameof(DetailAsync), new { id = entity.Id }, entity);
    }

    /// <summary>
    /// Update 系统配置 ✍️
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPatch("{id}")]
    public async Task<bool> UpdateAsync([FromRoute] Guid id, SystemConfigUpdateDto dto)
    {
        return await _manager.EditAsync(id, dto) == 1;
    }

    /// <summary>
    /// Get 系统配置 Detail ✍️
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<SystemConfigDetailDto?> DetailAsync([FromRoute] Guid id)
    {
        return await _manager.GetAsync(id);
    }

    /// <summary>
    /// Delete 系统配置 ✍️
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteAsync([FromRoute] Guid id)
    {
        return await _manager.DeleteAsync([id], false);
    }
}