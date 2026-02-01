using ModelMod.Models.ApplicationQuotaDtos;
using ModelMod.Services;

namespace AdminService.Controllers.ModelMod;

/// <summary>
/// 应用配额管理
/// </summary>
public class ApplicationQuotaController(
    Localizer localizer,
    IUserContext user,
    ILogger<ApplicationQuotaController> logger,
    ApplicationQuotaManager manager,
    IQuotaLimitingService quotaLimitingService
) : RestControllerBase<ApplicationQuotaManager>(localizer, manager, user, logger)
{
    private readonly IQuotaLimitingService _quotaLimitingService = quotaLimitingService;

    [HttpPost("filter")]
    public async Task<ActionResult<PageList<ApplicationQuotaItemDto>>> ListAsync(ApplicationQuotaFilterDto filter)
    {
        return await _manager.FilterAsync(filter);
    }

    [HttpPost]
    public async Task<ActionResult<ApplicationQuota>> AddAsync(ApplicationQuotaAddDto dto)
    {
        var entity = await _manager.AddAsync(dto);
        return CreatedAtRoute(null, new { id = entity.Id }, entity);
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

    /// <summary>
    /// 检查是否超出配额
    /// </summary>
    [HttpPost("check-quota")]
    public async Task<ActionResult<bool>> CheckQuotaAsync([FromBody] QuotaCheckRequestDto request)
    {
        var result = await _quotaLimitingService.CheckQuotaAsync(request.ApplicationId, request.EstimatedTokens);
        return result;
    }

    /// <summary>
    /// 消耗配额
    /// </summary>
    [HttpPost("consume")]
    public async Task<ActionResult<QuotaConsumeResultDto>> ConsumeAsync([FromBody] QuotaConsumeRequestDto request)
    {
        var result = await _quotaLimitingService.ConsumeAsync(request.ApplicationId, request.ActualTokens);
        return result;
    }

    /// <summary>
    /// 获取配额使用情况
    /// </summary>
    [HttpGet("usage/{applicationId}")]
    public async Task<ActionResult<QuotaUsageDto>> GetUsageAsync([FromRoute] Guid applicationId, [FromQuery] QuotaPeriodType periodType = QuotaPeriodType.Day)
    {
        var result = await _quotaLimitingService.GetUsageAsync(applicationId, periodType);
        return result;
    }

    /// <summary>
    /// 重置配额
    /// </summary>
    [HttpPost("reset")]
    public async Task<ActionResult<bool>> ResetQuotaAsync([FromBody] QuotaResetRequestDto request)
    {
        var result = await _quotaLimitingService.ResetQuotaAsync(request.ApplicationId, request.PeriodType);
        return result;
    }
}
