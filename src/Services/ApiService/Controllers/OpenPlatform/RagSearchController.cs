using Microsoft.AspNetCore.Mvc;
using Share.Services;

namespace ApiService.Controllers.OpenPlatform;

/// <summary>
/// Open platform RAG search
/// </summary>
[ApiController]
[Route("api/v1/rag/search")]
public class RagSearchController(
    IRagQueryFacade ragQueryFacade,
    IUserContext user,
    ILogger<RagSearchController> logger
) : OpenApiControllerBase<IRagQueryFacade>(ragQueryFacade, user, logger)
{
    [HttpPost]
    public async Task<ActionResult<RagQueryResult>> SearchAsync(RagQueryRequest request, CancellationToken cancellationToken)
    {
        return await _manager.QueryAsync(request, cancellationToken);
    }
}
