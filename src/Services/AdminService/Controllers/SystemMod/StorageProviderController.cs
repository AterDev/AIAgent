using SystemMod.Models.StorageProviderDtos;
namespace AdminService.Controllers.SystemMod;

/// <summary>
/// 存储服务商
/// </summary>
public class StorageProviderController(
    Localizer localizer,
    IUserContext user,
    ILogger<StorageProviderController> logger,
    StorageProviderManager manager
    ) : RestControllerBase<StorageProviderManager>(localizer, manager, user, logger)
{
    /// <summary>
    /// list 存储服务商 with page ✍️
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<StorageProviderItemDto>>> ListAsync(StorageProviderFilterDto filter)
    {
        return await _manager.FilterAsync(filter);
    }

    /// <summary>
    /// Add 存储服务商 ✍️
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<StorageProvider>> AddAsync(StorageProviderAddDto dto)
    {
        
        var entity = await _manager.AddAsync(dto);
        return CreatedAtRoute(null, new { id = entity.Id }, entity);
    }

    /// <summary>
    /// Update 存储服务商 ✍️
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPatch("{id}")]
    public async Task<bool> UpdateAsync([FromRoute] Guid id, StorageProviderUpdateDto dto)
    {
        return await _manager.EditAsync(id, dto) == 1;
    }

    /// <summary>
    /// Get 存储服务商 Detail ✍️
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<StorageProviderDetailDto?> DetailAsync([FromRoute] Guid id)
    {
        return await _manager.GetAsync(id);
    }

    /// <summary>
    /// Delete 存储服务商 ✍️
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteAsync([FromRoute] Guid id)
    {
        return await _manager.DeleteAsync([id], false);
    }

    /// <summary>
    /// 设置指定的存储服务商为活跃状态 ✍️
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("{id}/activate")]
    public async Task<ActionResult<bool>> SetActiveAsync([FromRoute] Guid id)
    {
        return await _manager.SetActiveAsync(id);
    }
}