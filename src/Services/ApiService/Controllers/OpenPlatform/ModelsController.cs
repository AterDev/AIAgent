using ModelMod.Models.ModelProfileDtos;

namespace ApiService.Controllers.OpenPlatform;

/// <summary>
/// Open platform models
/// </summary>
[ApiController]
[Route("api/v1/models")]
public class ModelsController(
    ModelProfileManager manager,
    IUserContext user,
    ILogger<ModelsController> logger
) : OpenApiControllerBase<ModelProfileManager>(manager, user, logger)
{
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<ModelProfileItemDto>>> ListAsync(ModelProfileFilterDto filter)
    {
        return await _manager.FilterAsync(filter);
    }

    [HttpGet("{id}")]
    public async Task<ModelProfileDetailDto?> DetailAsync([FromRoute] Guid id)
    {
        return await _manager.GetAsync(id);
    }
}
