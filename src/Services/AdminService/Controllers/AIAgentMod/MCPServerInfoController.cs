using AIAgentMod.Models.MCPServerInfoDtos;

namespace AdminService.Controllers.AIAgentMod;

/// <summary>
/// MCP Server 管理
/// </summary>
public class MCPServerInfoController(
    Localizer localizer,
    IUserContext user,
    ILogger<MCPServerInfoController> logger,
    MCPServerInfoManager manager
) : RestControllerBase<MCPServerInfoManager>(localizer, manager, user, logger)
{
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<MCPServerInfoItemDto>>> ListAsync(MCPServerInfoFilterDto filter)
    {
        return await _manager.FilterAsync(filter);
    }

    [HttpPost]
    public async Task<ActionResult<MCPServerInfo>> AddAsync(MCPServerInfoAddDto dto)
    {
        var entity = await _manager.AddAsync(dto);
        return CreatedAtRoute(null, new { id = entity.Id }, entity);
    }

    [HttpPatch("{id}")]
    public async Task<bool> UpdateAsync([FromRoute] Guid id, MCPServerInfoUpdateDto dto)
    {
        return await _manager.EditAsync(id, dto) == 1;
    }

    [HttpGet("{id}")]
    public async Task<MCPServerInfoDetailDto?> DetailAsync([FromRoute] Guid id)
    {
        return await _manager.GetAsync(id);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteAsync([FromRoute] Guid id)
    {
        return await _manager.DeleteAsync([id], false);
    }
}
