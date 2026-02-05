using CoreMod.Services;

namespace ApiService.Controllers.OpenPlatform;

/// <summary>
/// Open platform RAG search
/// </summary>
[ApiController]
[Route("api/v1/rag/search")]
public class RagSearchController(
    IRagQueryService ragQueryService,
    IUserContext user,
    ILogger<RagSearchController> logger
) : OpenApiControllerBase<IRagQueryService>(ragQueryService, user, logger)
{
    [HttpPost]
    public async Task<ActionResult<RagQueryResult>> SearchAsync(RagQueryRequest request, CancellationToken cancellationToken)
    {
        return await _manager.QueryAsync(request, cancellationToken);
    }
}
