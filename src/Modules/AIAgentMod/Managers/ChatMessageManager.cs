using AIAgentMod.Models.ChatMessageDtos;

namespace AIAgentMod.Managers;
/// <summary>
/// 聊天消息
/// </summary>
public class ChatMessageManager(
    TenantDbFactory dbContextFactory, 
    ILogger<ChatMessageManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, ChatMessage>(dbContextFactory, userContext, logger)
{
    /// <summary>
    /// Filter 聊天消息: 仅根据userId和conversationId筛选，并按时间正序排列
    /// </summary>
    public async Task<PageList<ChatMessageItemDto>> FilterAsync(ChatMessageFilterDto filter)
    {
        Queryable = Queryable
            .WhereNotNull(filter.UserId, q => q.UserId == filter.UserId)
            .WhereNotNull(filter.ConversationId, q => q.ConversationId == filter.ConversationId)
            .OrderBy(q => q.CreatedTime);
        return await PageListAsync<ChatMessageFilterDto, ChatMessageItemDto>(filter);
    }

    /// <summary>
    /// Add 聊天消息
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<ChatMessage> AddAsync(ChatMessageAddDto dto)
    {
        var entity = dto.MapTo<ChatMessage>();
        
        await InsertAsync(entity);
        return entity;
    }

    /// <summary>
    /// edit 聊天消息
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<int> EditAsync(Guid id, ChatMessageUpdateDto dto)
    {
        if (await HasPermissionAsync(id))
        {
            return await UpdateAsync(id, dto);
        }
        throw new BusinessException(Localizer.NoPermission);
    }


    /// <summary>
    /// Get 聊天消息 detail
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<ChatMessageDetailDto?> GetAsync(Guid id)
    {
        if (await HasPermissionAsync(id))
        {
            return await FindAsync<ChatMessageDetailDto>(q => q.Id == id);
        }
        throw new BusinessException(Localizer.NoPermission);
    }

    /// <summary>
    /// Delete  聊天消息
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="softDelete"></param>
    /// <returns></returns>
    public async Task<bool?> DeleteAsync(List<Guid> ids, bool softDelete = true)
    {
        if (!ids.Any())
        {
            return false;
        }
        if (ids.Count() == 1)
        {
            Guid id = ids.First();
            if (await HasPermissionAsync(id))
            {
                return await DeleteOrUpdateAsync(ids, !softDelete) > 0;
            }
            throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
        }
        else
        {
            var ownedIds = await GetOwnedIdsAsync(ids);
            if (ownedIds.Any())
            {
                return await DeleteOrUpdateAsync(ownedIds, !softDelete) > 0;
            }
            throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
        }
    }

    public override async Task<bool> HasPermissionAsync(Guid id)
    {
        var query = _dbSet
            .Where(q => q.Id == id);
        return await query.AnyAsync();
    }

    public async Task<List<Guid>> GetOwnedIdsAsync(IEnumerable<Guid> ids)
    {
        if (!ids.Any())
        {
            return [];
        }
        var query = _dbSet
            .Where(q => ids.Contains(q.Id))
            .Select(q => q.Id);
        return await query.ToListAsync();
    }
}