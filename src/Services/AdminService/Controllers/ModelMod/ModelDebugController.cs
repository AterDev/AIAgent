using ModelMod.Models.ModelDebugDtos;
using ModelMod.Services;

namespace AdminService.Controllers.ModelMod;

public class ModelDebugController(Localizer localizer, ModelDebugService debugService) : RestControllerBase(localizer)
{
    [HttpPost]
    public async Task<ActionResult<ModelDebugResponse>> ChatAsync(ModelDebugRequest request, CancellationToken cancellationToken)
    {
        return Ok(await debugService.ChatAsync(request, cancellationToken));
    }
}
