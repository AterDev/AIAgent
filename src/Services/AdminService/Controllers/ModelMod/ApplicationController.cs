using ModelMod.Models.ApplicationDtos;
namespace AdminService.Controllers.ModelMod;

/// <summary>
/// 应用定义
/// </summary>
public class ApplicationController(
    Localizer localizer,
    IUserContext user,
    ILogger<ApplicationController> logger,
    ApplicationManager manager
    ) : RestControllerBase<ApplicationManager>(localizer, manager, user, logger)
{
    /// <summary>
    /// list 应用定义 with page ✍️
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<ApplicationItemDto>>> ListAsync(ApplicationFilterDto filter)
    {
        return await _manager.FilterAsync(filter);
    }

    /// <summary>
    /// Add 应用定义 ✍️
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<ApplicationCredentialResultDto>> AddAsync(ApplicationAddDto dto)
    {

        var result = await _manager.AddAsync(dto);
        return CreatedAtRoute(null, new { id = result.Id }, result);
    }

    /// <summary>
    /// Update 应用定义 ✍️
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPatch("{id}")]
    public async Task<bool> UpdateAsync([FromRoute] Guid id, ApplicationUpdateDto dto)
    {
        return await _manager.EditAsync(id, dto) == 1;
    }

    /// <summary>
    /// Get 应用定义 Detail ✍️
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ApplicationDetailDto?> DetailAsync([FromRoute] Guid id)
    {
        return await _manager.GetAsync(id);
    }

    /// <summary>
    /// 重置应用密钥
    /// </summary>
    [HttpPost("{id}/reset-secret")]
    public async Task<ActionResult<ApplicationCredentialResultDto>> ResetSecretAsync([FromRoute] Guid id)
    {
        return Ok(await _manager.ResetSecretAsync(id));
    }

    /// <summary>
    /// Delete 应用定义 ✍️
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteAsync([FromRoute] Guid id)
    {
        return await _manager.DeleteAsync([id], false);
    }
}