using ModelMod.Models.ModelInvocationDtos;

namespace ModelMod.Managers;

/// <summary>
/// 模型调用记录管理
/// </summary>
public class ModelInvocationManager(
    TenantDbFactory dbContextFactory,
    ILogger<ModelInvocationManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, ModelInvocation>(dbContextFactory, userContext, logger)
{
    public async Task<PageList<ModelInvocationItemDto>> FilterAsync(ModelInvocationFilterDto filter)
    {
        Queryable = Queryable.Where(q => q.TenantId == _userContext.TenantId)
            .WhereNotNull(filter.ApplicationId, q => q.ApplicationId == filter.ApplicationId)
            .WhereNotNull(filter.ModelProfileId, q => q.ModelProfileId == filter.ModelProfileId)
            .WhereNotNull(filter.Scene, q => q.Scene == filter.Scene)
            .WhereNotNull(filter.Status, q => q.Status == filter.Status);

        return await PageListAsync<ModelInvocationFilterDto, ModelInvocationItemDto>(filter);
    }

    public async Task<ModelInvocation> AddAsync(ModelInvocationAddDto dto)
    {
        var entity = dto.MapTo<ModelInvocation>();
        await InsertAsync(entity);
        return entity;
    }

    public async Task<int> EditAsync(Guid id, ModelInvocationUpdateDto dto)
    {
        return await UpdateAsync(id, dto);
    }

    public async Task<ModelInvocationDetailDto?> GetAsync(Guid id)
    {
        return await FindAsync<ModelInvocationDetailDto>(q => q.Id == id && q.TenantId == _userContext.TenantId);
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
