using AIAgentMod.Models.AIAgentDtos;

namespace AIAgentMod.Managers;
/// <summary>
/// agent
/// </summary>
public class AIAgentManager(
    TenantDbFactory dbContextFactory, 
    ILogger<AIAgentManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, AIAgent>(dbContextFactory, userContext, logger)
{
    /// <summary>
    /// Filter agent with paging
    /// </summary>
    public async Task<PageList<AIAgentItemDto>> FilterAsync(AIAgentFilterDto filter)
    {        
        Queryable = Queryable.Where(q => q.UserId == _userContext.UserId)
            .WhereNotNull(filter.Enable, q => q.Enable == filter.Enable)
            .WhereNotNull(filter.IsTemplate, q => q.IsTemplate == filter.IsTemplate)
            .WhereNotNull(filter.Name, q => q.Name == filter.Name)
            .WhereNotNull(filter.UserId, q => q.UserId == filter.UserId);

        return await PageListAsync<AIAgentFilterDto, AIAgentItemDto>(filter);
    }

    /// <summary>
    /// Add agent
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<AIAgent> AddAsync(AIAgentAddDto dto)
    {
        var entity = dto.MapTo<AIAgent>();
        entity.UserId = _userContext.UserId;
        await InsertAsync(entity);
        return entity;
    }

    /// <summary>
    /// edit agent
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<int> EditAsync(Guid id, AIAgentUpdateDto dto)
    {
        if (await HasPermissionAsync(id))
        {
            return await UpdateAsync(id, dto);
        }
        throw new BusinessException(Localizer.NoPermission);
    }


    /// <summary>
    /// Get agent detail
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<AIAgentDetailDto?> GetAsync(Guid id)
    {
        if (await HasPermissionAsync(id))
        {
            return await FindAsync<AIAgentDetailDto>(q => q.Id == id);
        }
        throw new BusinessException(Localizer.NoPermission);
    }

    /// <summary>
    /// Delete  agent
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
        var query = _dbSet.Where(q => q.UserId == _userContext.UserId)
            .Where(q => q.Id == id && q.TenantId == _userContext.TenantId);
        return await query.AnyAsync();
    }

    public async Task<List<Guid>> GetOwnedIdsAsync(IEnumerable<Guid> ids)
    {
        if (!ids.Any())
        {
            return [];
        }
        var query = _dbSet.Where(q => q.UserId == _userContext.UserId)
            .Where(q => ids.Contains(q.Id) && q.TenantId == _userContext.TenantId)
            .Select(q => q.Id);
        return await query.ToListAsync();
    }
}