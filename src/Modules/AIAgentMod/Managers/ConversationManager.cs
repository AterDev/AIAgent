using AIAgentMod.Models.ConversationDtos;

namespace AIAgentMod.Managers;
/// <summary>
/// 对话实例
/// </summary>
public class ConversationManager(
    TenantDbFactory dbContextFactory, 
    ILogger<ConversationManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, Conversation>(dbContextFactory, userContext, logger)
{
    /// <summary>
    /// Filter 对话实例 with paging
    /// </summary>
    public async Task<PageList<ConversationItemDto>> FilterAsync(ConversationFilterDto filter)
    {        
        Queryable = Queryable
            .WhereNotNull(filter.IsPinned, q => q.IsPinned == filter.IsPinned)
            .WhereNotNull(filter.UserId, q => q.UserId == filter.UserId);

        return await PageListAsync<ConversationFilterDto, ConversationItemDto>(filter);
    }

    /// <summary>
    /// Add 对话实例
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<Conversation> AddAsync(ConversationAddDto dto)
    {
        var entity = dto.MapTo<Conversation>();
        
        await InsertAsync(entity);
        return entity;
    }

    /// <summary>
    /// edit 对话实例
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<int> EditAsync(Guid id, ConversationUpdateDto dto)
    {
        if (await HasPermissionAsync(id))
        {
            return await UpdateAsync(id, dto);
        }
        throw new BusinessException(Localizer.NoPermission);
    }


    /// <summary>
    /// Get 对话实例 detail
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<ConversationDetailDto?> GetAsync(Guid id)
    {
        if (await HasPermissionAsync(id))
        {
            return await FindAsync<ConversationDetailDto>(q => q.Id == id);
        }
        throw new BusinessException(Localizer.NoPermission);
    }

    /// <summary>
    /// Delete  对话实例
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