using ModelMod.Models.ModelProfileDtos;
using Share.Services;

namespace ModelMod.Managers;

/// <summary>
/// 模型配置管理
/// </summary>
public class ModelProfileManager(
    TenantDbFactory dbContextFactory,
    ILogger<ModelProfileManager> logger,
    IUserContext userContext,
    ICacheService? cacheService = null
) : ManagerBase<DefaultDbContext, ModelProfile>(dbContextFactory, userContext, logger)
{
    private const string CacheKeyPrefix = "ModelProfile:";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(15);

    public async Task<PageList<ModelProfileItemDto>> FilterAsync(ModelProfileFilterDto filter)
    {
        Queryable = Queryable
            .AsNoTracking()
            .Where(q => q.TenantId == _userContext.TenantId)
            .WhereNotNull(filter.ProviderId, q => q.ProviderId == filter.ProviderId)
            .WhereNotNull(filter.Name, q => q.Name == filter.Name)
            .WhereNotNull(filter.IsEnabled, q => q.IsEnabled == filter.IsEnabled);

        return await PageListAsync<ModelProfileFilterDto, ModelProfileItemDto>(filter);
    }

    public async Task<ModelProfile> AddAsync(ModelProfileAddDto dto)
    {
        var entity = dto.MapTo<ModelProfile>();
        await InsertAsync(entity);
        
        // 清除缓存
        if (cacheService != null)
        {
            await cacheService.RemoveAsync(GetCacheKey(entity.Id));
        }
        
        return entity;
    }

    public async Task<int> EditAsync(Guid id, ModelProfileUpdateDto dto)
    {
        var result = await UpdateAsync(id, dto);
        
        // 清除缓存
        if (cacheService != null && result > 0)
        {
            await cacheService.RemoveAsync(GetCacheKey(id));
        }
        
        return result;
    }

    public async Task<ModelProfileDetailDto?> GetAsync(Guid id)
    {
        // 尝试从缓存获取
        if (cacheService != null)
        {
            var cached = await cacheService.GetAsync<ModelProfileDetailDto>(GetCacheKey(id));
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for ModelProfile: {Id}", id);
                return cached;
            }
        }

        // 从数据库查询
        var result = await FindAsync<ModelProfileDetailDto>(q => q.Id == id && q.TenantId == _userContext.TenantId);
        
        // 保存到缓存
        if (cacheService != null && result != null)
        {
            await cacheService.SetAsync(GetCacheKey(id), result, CacheExpiration);
            _logger.LogDebug("Cache set for ModelProfile: {Id}", id);
        }
        
        return result;
    }

    public async Task<bool?> DeleteAsync(List<Guid> ids, bool softDelete = true)
    {
        if (!ids.Any())
        {
            return false;
        }
        
        var result = await DeleteOrUpdateAsync(ids, !softDelete) > 0;
        
        // 清除缓存
        if (cacheService != null && result)
        {
            foreach (var id in ids)
            {
                await cacheService.RemoveAsync(GetCacheKey(id));
            }
        }
        
        return result;
    }

    public override async Task<bool> HasPermissionAsync(Guid id)
    {
        return await _dbSet.AnyAsync(q => q.Id == id && q.TenantId == _userContext.TenantId);
    }

    private static string GetCacheKey(Guid id) => $"{CacheKeyPrefix}{id}";
}
