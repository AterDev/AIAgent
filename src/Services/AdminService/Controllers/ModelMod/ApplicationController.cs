using ModelMod.Models.ApplicationDtos;
using ModelMod.Models.ApplicationApiKeyDtos;
using Share.Exceptions;
namespace AdminService.Controllers.ModelMod;

/// <summary>
/// 应用定义
/// </summary>
public class ApplicationController(
    Localizer localizer,
    IUserContext user,
    ILogger<ApplicationController> logger,
    ApplicationManager manager,
    ApiKeyAuthIndexManager apiKeyAuthIndexManager
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
    public async Task<ActionResult<ApplicationDetailDto>> AddAsync(ApplicationAddDto dto)
    {

        var result = await _manager.AddAsync(dto);
        return Ok(result);
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
        var rows = await _manager.EditAsync(id, dto);
        var application = await _manager.GetEntityAsync(id);
        if (application is not null)
        {
            await apiKeyAuthIndexManager.SyncApplicationAsync(application);
        }

        return rows == 1;
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
    /// 获取应用 ApiKey 列表
    /// </summary>
    [HttpGet("{id}/api-keys")]
    public async Task<ActionResult<List<ApplicationApiKeyItemDto>>> ListApiKeysAsync([FromRoute] Guid id)
    {
        var application = await _manager.GetEntityAsync(id)
            ?? throw new BusinessException(Localizer.ApplicationNotFound, StatusCodes.Status404NotFound);

        return Ok(await apiKeyAuthIndexManager.ListAsync(application.Id));
    }

    /// <summary>
    /// 新增应用 ApiKey
    /// </summary>
    [HttpPost("{id}/api-keys")]
    public async Task<ActionResult<ApplicationApiKeyCredentialResultDto>> AddApiKeyAsync([FromRoute] Guid id, [FromBody] ApplicationApiKeyAddDto dto)
    {
        var application = await _manager.GetEntityAsync(id)
            ?? throw new BusinessException(Localizer.ApplicationNotFound, StatusCodes.Status404NotFound);

        return Ok(await apiKeyAuthIndexManager.AddAsync(application, dto));
    }

    /// <summary>
    /// 删除应用 ApiKey
    /// </summary>
    [HttpDelete("{id}/api-keys/{apiKeyId}")]
    public async Task<ActionResult<bool>> DeleteApiKeyAsync([FromRoute] Guid id, [FromRoute] Guid apiKeyId)
    {
        var application = await _manager.GetEntityAsync(id)
            ?? throw new BusinessException(Localizer.ApplicationNotFound, StatusCodes.Status404NotFound);

        return Ok(await apiKeyAuthIndexManager.DeleteAsync(application.Id, apiKeyId));
    }

    /// <summary>
    /// Delete 应用定义 ✍️
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteAsync([FromRoute] Guid id)
    {
        await apiKeyAuthIndexManager.DeleteByApplicationIdAsync(id);
        return await _manager.DeleteAsync([id], false);
    }
}