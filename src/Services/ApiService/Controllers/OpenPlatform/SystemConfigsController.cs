using SystemMod.Managers;
using SystemMod.Models.SystemConfigDtos;

namespace ApiService.Controllers.OpenPlatform;

/// <summary>
/// Open platform system configs
/// </summary>
[ApiController]
[Route("api/v1/system-configs")]
public class SystemConfigsController(
    SystemConfigManager manager,
    IUserContext user,
    ILogger<SystemConfigsController> logger
) : OpenApiControllerBase<SystemConfigManager>(manager, user, logger)
{
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<SystemConfigItemDto>>> ListAsync(SystemConfigFilterDto filter)
    {
        return await _manager.FilterAsync(filter);
    }

    [HttpGet("{id}")]
    public async Task<SystemConfigDetailDto?> DetailAsync([FromRoute] Guid id)
    {
        return await _manager.GetAsync(id);
    }
}
