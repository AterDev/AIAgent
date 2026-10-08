using ModelMod.Models.ModelDebugDtos;
using ModelMod.Services;

namespace AdminService.Controllers.ModelMod;

public class ModelDebugController(Localizer localizer, ModelDebugService debugService) : RestControllerBase(localizer)
{
    [HttpPost]
    public async Task<ActionResult<ModelDebugResponse>> ChatAsync(ModelDebugRequest request, CancellationToken cancellationToken)
    {
        return Ok(await debugService.InvokeAsync(request, cancellationToken));
    }

    [HttpPost("invoke")]
    public async Task<ActionResult<ModelDebugResponse>> InvokeAsync(ModelDebugRequest request, CancellationToken cancellationToken)
    {
        return Ok(await debugService.InvokeAsync(request, cancellationToken));
    }
}
