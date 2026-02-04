using SystemMod.Models.StorageProviderDtos;

namespace SystemMod.Managers;
/// <summary>
/// 存储服务商
/// </summary>
public class StorageProviderManager(
    TenantDbFactory dbContextFactory, 
    ILogger<StorageProviderManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, StorageProvider>(dbContextFactory, userContext, logger)
{
    /// <summary>
    /// Filter 存储服务商 with paging
    /// </summary>
    public async Task<PageList<StorageProviderItemDto>> FilterAsync(StorageProviderFilterDto filter)
    {        
        Queryable = Queryable
            .WhereNotNull(filter.Name, q => q.Name == filter.Name)
            .WhereNotNull(filter.IsCloud, q => q.IsCloud == filter.IsCloud)
            .WhereNotNull(filter.IsActive, q => q.IsActive == filter.IsActive);

        return await PageListAsync<StorageProviderFilterDto, StorageProviderItemDto>(filter);
    }

    /// <summary>
    /// Add 存储服务商
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<StorageProvider> AddAsync(StorageProviderAddDto dto)
    {
        var entity = dto.MapTo<StorageProvider>();
        
        // 如果设置为启用，则禁用其他所有存储服务商
        if (entity.IsActive)
        {
            await DeactivateAllProvidersAsync();
        }
        
        await InsertAsync(entity);
        return entity;
    }

    /// <summary>
    /// edit 存储服务商
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<int> EditAsync(Guid id, StorageProviderUpdateDto dto)
    {
        if (await HasPermissionAsync(id))
        {
            // 如果设置为启用，则禁用其他所有存储服务商
            if (dto.IsActive == true)
            {
                await DeactivateAllProvidersAsync(id);
            }
            return await UpdateAsync(id, dto);
        }
        throw new BusinessException(Localizer.NoPermission);
    }

    /// <summary>
    /// 设置指定的存储服务商为活跃状态（同时禁用其他所有）
    /// </summary>
    /// <param name="id">存储服务商ID</param>
    /// <returns></returns>
    public async Task<bool> SetActiveAsync(Guid id)
    {
        if (!await HasPermissionAsync(id))
        {
            throw new BusinessException(Localizer.NoPermission);
        }

        await DeactivateAllProvidersAsync();
        return await UpdateAsync(id, new StorageProviderUpdateDto { IsActive = true }) > 0;
    }

    /// <summary>
    /// 获取当前活跃的存储服务商
    /// </summary>
    /// <returns></returns>
    public async Task<StorageProvider?> GetActiveProviderAsync()
    {
        return await _dbSet.FirstOrDefaultAsync(q => q.IsActive);
    }

    /// <summary>
    /// 禁用所有存储服务商（除了指定的ID）
    /// </summary>
    /// <param name="excludeId">排除的ID（可选）</param>
    private async Task DeactivateAllProvidersAsync(Guid? excludeId = null)
    {
        var activeProviders = _dbSet.Where(q => q.IsActive);
        if (excludeId.HasValue)
        {
            activeProviders = activeProviders.Where(q => q.Id != excludeId.Value);
        }
        await activeProviders.ExecuteUpdateAsync(setters => setters.SetProperty(p => p.IsActive, false));
    }


    /// <summary>
    /// Get 存储服务商 detail
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<StorageProviderDetailDto?> GetAsync(Guid id)
    {
        if (await HasPermissionAsync(id))
        {
            return await FindAsync<StorageProviderDetailDto>(q => q.Id == id);
        }
        throw new BusinessException(Localizer.NoPermission);
    }

    /// <summary>
    /// Delete  存储服务商
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
        
        // 检查是否包含活跃的存储服务商，禁止删除
        var hasActiveProvider = await _dbSet
            .Where(q => ids.Contains(q.Id) && q.IsActive)
            .AnyAsync();
        if (hasActiveProvider)
        {
            throw new BusinessException("无法删除活跃的存储服务商，请先激活其他服务商");
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