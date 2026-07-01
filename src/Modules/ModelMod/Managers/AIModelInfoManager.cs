using ModelMod.Models.AIModelInfoDtos;

namespace ModelMod.Managers;

/// <summary>
/// 模型信息
/// </summary>
public class AIModelInfoManager(
    TenantDbFactory dbContextFactory,
    ILogger<AIModelInfoManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, AIModelInfo>(dbContextFactory, userContext, logger)
{
    /// <summary>
    /// Filter 模型信息 with paging
    /// </summary>
    public async Task<PageList<AIModelInfoItemDto>> FilterAsync(AIModelInfoFilterDto filter)
    {
        Queryable = Queryable.WhereNotNull(filter.ProviderId, q => q.ProviderId == filter.ProviderId);
        return await PageListAsync<AIModelInfoFilterDto, AIModelInfoItemDto>(filter);
    }

    /// <summary>
    /// Add 模型信息
    /// </summary>
    public async Task<AIModelInfo> AddAsync(AIModelInfoAddDto dto)
    {
        var entity = dto.MapTo<AIModelInfo>();
        await InsertAsync(entity);
        return entity;
    }

    /// <summary>
    /// Edit 模型信息
    /// </summary>
    public async Task<int> EditAsync(Guid id, AIModelInfoUpdateDto dto)
    {
        if (await HasPermissionAsync(id))
        {
            return await UpdateAsync(id, dto);
        }
        throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
    }

    /// <summary>
    /// Get 模型信息 detail
    /// </summary>
    public async Task<AIModelInfoDetailDto?> GetAsync(Guid id)
    {
        if (await HasPermissionAsync(id))
        {
            return await FindAsync<AIModelInfoDetailDto>(q => q.Id == id);
        }
        throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
    }

    /// <summary>
    /// Delete 模型信息
    /// </summary>
    public async Task<bool> DeleteAsync(List<Guid> ids, bool softDelete = true)
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
