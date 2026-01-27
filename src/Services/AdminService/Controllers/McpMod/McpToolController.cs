using Entity.McpMod;
using McpMod.Managers;
using McpMod.Models;
using McpMod.Models.McpToolDtos;

namespace AdminService.Controllers.McpMod;

/// <summary>
/// MCP 工具管理
/// </summary>
public class McpToolController(
    Localizer localizer,
    IUserContext user,
    ILogger<McpToolController> logger,
    McpToolManager manager
) : RestControllerBase<McpToolManager>(localizer, manager, user, logger)
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

    [HttpPost]
    public async Task<ActionResult<McpTool>> AddAsync(McpToolAddDto dto)
    {
        var entity = await _manager.AddAsync(dto);
        return CreatedAtRoute(nameof(DetailAsync), new { id = entity.Id }, entity);
    }

    [HttpPatch("{id}")]
    public async Task<bool> UpdateAsync([FromRoute] Guid id, McpToolUpdateDto dto)
    {
        return await _manager.EditAsync(id, dto) == 1;
    }

    [HttpGet("{id}")]
    public async Task<McpToolDetailDto?> DetailAsync([FromRoute] Guid id)
    {
        return await _manager.GetAsync(id);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteAsync([FromRoute] Guid id)
    {
        return await _manager.DeleteAsync([id], false);
    }
}
