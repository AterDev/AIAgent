using ModelMod.Models.ModelProviderDtos;

namespace ModelMod.Managers;

/// <summary>
/// 模型提供商管理
/// </summary>
public class ModelProviderManager(
    TenantDbFactory dbContextFactory,
    ILogger<ModelProviderManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, ModelProvider>(dbContextFactory, userContext, logger)
{
    public async Task<PageList<ModelProviderItemDto>> FilterAsync(ModelProviderFilterDto filter)
    {
        Queryable = Queryable
            .Where(q => q.TenantId == _userContext.TenantId)
            .WhereNotNull(filter.Name, q => q.Name == filter.Name)
            .WhereNotNull(filter.ProviderType, q => q.ProviderType == filter.ProviderType)
            .WhereNotNull(filter.IsEnabled, q => q.IsEnabled == filter.IsEnabled);

        return await PageListAsync<ModelProviderFilterDto, ModelProviderItemDto>(filter);
    }

    public async Task<ModelProvider> AddAsync(ModelProviderAddDto dto)
    {
        var entity = dto.MapTo<ModelProvider>();
        await InsertAsync(entity);
        return entity;
    }

    public async Task<int> EditAsync(Guid id, ModelProviderUpdateDto dto)
    {
        return await UpdateAsync(id, dto);
    }

    public async Task<ModelProviderDetailDto?> GetAsync(Guid id)
    {
        return await FindAsync<ModelProviderDetailDto>(q => q.Id == id && q.TenantId == _userContext.TenantId);
    }

    public async Task<bool?> DeleteAsync(List<Guid> ids, bool softDelete = true)
    {
        if (!ids.Any())
        {
            return false;
        }
        return await DeleteOrUpdateAsync(ids, !softDelete) > 0;
    }

    public override async Task<bool> HasPermissionAsync(Guid id)
    {
        return await _dbSet.AnyAsync(q => q.Id == id && q.TenantId == _userContext.TenantId);
    }
}
