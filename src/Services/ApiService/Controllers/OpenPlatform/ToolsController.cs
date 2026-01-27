using McpMod.Managers;
using McpMod.Models;
using McpMod.Models.McpToolDtos;

namespace ApiService.Controllers.OpenPlatform;

/// <summary>
/// Open platform tools
/// </summary>
[ApiController]
[Route("api/v1/tools")]
public class ToolsController(
    McpToolManager manager,
    IUserContext user,
    ILogger<ToolsController> logger
) : OpenApiControllerBase<McpToolManager>(manager, user, logger)
{
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<McpToolItemDto>>> ListAsync(McpToolFilterDto filter)
    {
        return await _manager.FilterAsync(filter);
    }

    [HttpGet("definitions")]
    public async Task<ActionResult<List<ToolDefinitionDto>>> DefinitionsAsync()
    {
        return await _manager.GetDefinitionsAsync();
    }

    [HttpGet("{id}")]
    public async Task<McpToolDetailDto?> DetailAsync([FromRoute] Guid id)
    {
        return await _manager.GetAsync(id);
    }
}
