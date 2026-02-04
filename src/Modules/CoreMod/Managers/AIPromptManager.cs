using CoreMod.Models.AIPromptDtos;

namespace CoreMod.Managers;
/// <summary>
/// 提示词
/// </summary>
public class AIPromptManager(
    TenantDbFactory dbContextFactory, 
    ILogger<AIPromptManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, AIPrompt>(dbContextFactory, userContext, logger)
{
    /// <summary>
    /// Filter 提示词 with paging
    /// </summary>
    public async Task<PageList<AIPromptItemDto>> FilterAsync(AIPromptFilterDto filter)
    {        
        Queryable = Queryable
            .WhereNotNull(filter.Name, q => q.Name == filter.Name)
            .WhereNotNull(filter.GroupName, q => q.GroupName == filter.GroupName);

        return await PageListAsync<AIPromptFilterDto, AIPromptItemDto>(filter);
    }

    /// <summary>
    /// Add 提示词
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<AIPrompt> AddAsync(AIPromptAddDto dto)
    {
        var entity = dto.MapTo<AIPrompt>();
        
        await InsertAsync(entity);
        return entity;
    }

    /// <summary>
    /// edit 提示词
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<int> EditAsync(Guid id, AIPromptUpdateDto dto)
    {
        if (await HasPermissionAsync(id))
        {
            return await UpdateAsync(id, dto);
        }
        throw new BusinessException(Localizer.NoPermission);
    }


    /// <summary>
    /// Get 提示词 detail
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<AIPromptDetailDto?> GetAsync(Guid id)
    {
        if (await HasPermissionAsync(id))
        {
            return await FindAsync<AIPromptDetailDto>(q => q.Id == id);
        }
        throw new BusinessException(Localizer.NoPermission);
    }

    /// <summary>
    /// Delete  提示词
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