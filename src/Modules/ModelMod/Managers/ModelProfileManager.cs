using ModelMod.Models.ModelProfileDtos;

namespace ModelMod.Managers;

/// <summary>
/// 模型配置管理
/// </summary>
public class ModelProfileManager(
    TenantDbFactory dbContextFactory,
    ILogger<ModelProfileManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, ModelProfile>(dbContextFactory, userContext, logger)
{
    public async Task<PageList<ModelProfileItemDto>> FilterAsync(ModelProfileFilterDto filter)
    {
        Queryable = Queryable.Where(q => q.TenantId == _userContext.TenantId)
            .WhereNotNull(filter.ProviderId, q => q.ProviderId == filter.ProviderId)
            .WhereNotNull(filter.Name, q => q.Name == filter.Name)
            .WhereNotNull(filter.IsEnabled, q => q.IsEnabled == filter.IsEnabled);

        return await PageListAsync<ModelProfileFilterDto, ModelProfileItemDto>(filter);
    }

    public async Task<ModelProfile> AddAsync(ModelProfileAddDto dto)
    {
        var entity = dto.MapTo<ModelProfile>();
        await InsertAsync(entity);
        return entity;
    }

    public async Task<int> EditAsync(Guid id, ModelProfileUpdateDto dto)
    {
        return await UpdateAsync(id, dto);
    }

    public async Task<ModelProfileDetailDto?> GetAsync(Guid id)
    {
        return await FindAsync<ModelProfileDetailDto>(q => q.Id == id && q.TenantId == _userContext.TenantId);
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
