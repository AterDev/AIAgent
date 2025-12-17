using AIAgentMod.Models.ConversationDtos;
namespace AdminService.Controllers.AIAgentMod;

/// <summary>
/// 对话实例
/// </summary>
public class ConversationController(
    Localizer localizer,
    IUserContext user,
    ILogger<ConversationController> logger,
    ConversationManager manager
    ) : RestControllerBase<ConversationManager>(localizer, manager, user, logger)
{
    /// <summary>
    /// 分页查询对话实例
    /// </summary>
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<ConversationItemDto>>> ListAsync(ConversationFilterDto filter)
    {
        return await _manager.FilterAsync(filter);
    }

    /// <summary>
    /// 新增对话实例
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Conversation>> AddAsync(ConversationAddDto dto)
    {
        var entity = await _manager.AddAsync(dto);
        return CreatedAtAction(nameof(DetailAsync), new { id = entity.Id }, entity);
    }

    /// <summary>
    /// 更新对话实例
    /// </summary>
    [HttpPatch("{id}")]
    public async Task<bool> UpdateAsync([FromRoute] Guid id, ConversationUpdateDto dto)
    {
        return await _manager.EditAsync(id, dto) == 1;
    }

    /// <summary>
    /// 获取对话实例详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ConversationDetailDto?> DetailAsync([FromRoute] Guid id)
    {
        return await _manager.GetAsync(id);
    }

    /// <summary>
    /// 删除对话实例
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<bool?>> DeleteAsync([FromRoute] Guid id)
    {
        return await _manager.DeleteAsync([id], false);
    }
}
