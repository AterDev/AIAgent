using ModelMod.Models.ApplicationDtos;

namespace ApiService.Controllers.OpenPlatform;

/// <summary>
/// Open platform apps
/// </summary>
[ApiController]
[Route("api/v1/apps")]
public class AppsController(
    ApplicationManager manager,
    IUserContext user,
    ILogger<AppsController> logger
) : OpenApiControllerBase<ApplicationManager>(manager, user, logger)
{
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<ApplicationItemDto>>> ListAsync(ApplicationFilterDto filter)
    {
        return await _manager.FilterAsync(filter);
    }

    [HttpGet("{id}")]
    public async Task<ApplicationDetailDto?> DetailAsync([FromRoute] Guid id)
    {
        return await _manager.GetAsync(id);
    }
}
