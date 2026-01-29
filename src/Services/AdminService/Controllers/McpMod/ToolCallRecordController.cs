using Entity.McpMod;
using McpMod.Managers;
using McpMod.Models.ToolCallRecordDtos;

namespace AdminService.Controllers.McpMod;

/// <summary>
/// MCP 调用记录管理
/// </summary>
public class ToolCallRecordController(
    Localizer localizer,
    IUserContext user,
    ILogger<ToolCallRecordController> logger,
    ToolCallRecordManager manager
) : RestControllerBase<ToolCallRecordManager>(localizer, manager, user, logger)
{
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<ToolCallRecordItemDto>>> ListAsync(ToolCallRecordFilterDto filter)
    {
        return await _manager.FilterAsync(filter);
    }

    [HttpPost]
    public async Task<ActionResult<ToolCallRecord>> AddAsync(ToolCallRecordAddDto dto)
    {
        var entity = await _manager.AddAsync(dto);
        return CreatedAtRoute(null, new { id = entity.Id }, entity);
    }

    [HttpPatch("{id}")]
    public async Task<bool> UpdateAsync([FromRoute] Guid id, ToolCallRecordUpdateDto dto)
    {
        return await _manager.EditAsync(id, dto) == 1;
    }

    [HttpGet("{id}")]
    public async Task<ToolCallRecordDetailDto?> DetailAsync([FromRoute] Guid id)
    {
        return await _manager.GetAsync(id);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteAsync([FromRoute] Guid id)
    {
        return await _manager.DeleteAsync([id], false);
    }
}
