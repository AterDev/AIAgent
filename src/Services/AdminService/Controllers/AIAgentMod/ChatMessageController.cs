using AIAgentMod.Models.ChatMessageDtos;

namespace AdminService.Controllers.AIAgentMod;

/// <summary>
/// 聊天消息 控制器
/// </summary>
public class ChatMessageController(
    Localizer localizer,
    IUserContext user,
    ILogger<ChatMessageController> logger,
    ChatMessageManager manager
) : RestControllerBase<ChatMessageManager>(localizer, manager, user, logger)
{
    /// <summary>
    /// 分页查询聊天消息
    /// </summary>
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<ChatMessageItemDto>>> ListAsync(ChatMessageFilterDto filter)
    {
        return await _manager.FilterAsync(filter);
    }

    /// <summary>
    /// 新增聊天消息
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ChatMessage>> AddAsync(ChatMessageAddDto dto)
    {
        var entity = await _manager.AddAsync(dto);
        return CreatedAtAction(nameof(DetailAsync), new { id = entity.Id }, entity);
    }

    /// <summary>
    /// 更新聊天消息
    /// </summary>
    [HttpPatch("{id}")]
    public async Task<bool> UpdateAsync([FromRoute] Guid id, ChatMessageUpdateDto dto)
    {
        return await _manager.EditAsync(id, dto) == 1;
    }

    /// <summary>
    /// 获取聊天消息详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ChatMessageDetailDto?> DetailAsync([FromRoute] Guid id)
    {
        return await _manager.GetAsync(id);
    }

    /// <summary>
    /// 删除聊天消息
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<bool?>> DeleteAsync([FromRoute] Guid id)
    {
        return await _manager.DeleteAsync([id], false);
    }
}
