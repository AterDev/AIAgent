using AIAgentMod.Models.AIModelProviderDtos;

namespace AIAgentMod.Managers;
/// <summary>
/// AI模型提供商
/// </summary>
public class AIModelProviderManager(
    TenantDbFactory dbContextFactory, 
    ILogger<AIModelProviderManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, AIModelProvider>(dbContextFactory, userContext, logger)
{
    /// <summary>
    /// Filter AI模型提供商 with paging
    /// </summary>
    public async Task<PageList<AIModelProviderItemDto>> FilterAsync(AIModelProviderFilterDto filter)
    {        
        

        return await PageListAsync<AIModelProviderFilterDto, AIModelProviderItemDto>(filter);
    }

    /// <summary>
    /// Add AI模型提供商
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<AIModelProvider> AddAsync(AIModelProviderAddDto dto)
    {
        var entity = dto.MapTo<AIModelProvider>();
        
        await InsertAsync(entity);
        return entity;
    }

    /// <summary>
    /// edit AI模型提供商
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<int> EditAsync(Guid id, AIModelProviderUpdateDto dto)
    {
        if (await HasPermissionAsync(id))
        {
            return await UpdateAsync(id, dto);
        }
        throw new BusinessException(Localizer.NoPermission);
    }


    /// <summary>
    /// Get AI模型提供商 detail
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<AIModelProviderDetailDto?> GetAsync(Guid id)
    {
        if (await HasPermissionAsync(id))
        {
            return await FindAsync<AIModelProviderDetailDto>(q => q.Id == id);
        }
        throw new BusinessException(Localizer.NoPermission);
    }

    /// <summary>
    /// Delete  AI模型提供商
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
            .Where(q => q.Id == id && q.TenantId == _userContext.TenantId);
        return await query.AnyAsync();
    }

    public async Task<List<Guid>> GetOwnedIdsAsync(IEnumerable<Guid> ids)
    {
        if (!ids.Any())
        {
            return [];
        }
        var query = _dbSet
            .Where(q => ids.Contains(q.Id) && q.TenantId == _userContext.TenantId)
            .Select(q => q.Id);
        return await query.ToListAsync();
    }
}