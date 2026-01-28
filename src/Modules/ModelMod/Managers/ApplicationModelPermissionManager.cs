using ModelMod.Models.ApplicationModelPermissionDtos;

namespace ModelMod.Managers;

/// <summary>
/// 应用模型权限管理
/// </summary>
public class ApplicationModelPermissionManager(
    TenantDbFactory dbContextFactory,
    ILogger<ApplicationModelPermissionManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, ApplicationModelPermission>(dbContextFactory, userContext, logger)
{
    public async Task<PageList<ApplicationModelPermissionItemDto>> FilterAsync(ApplicationModelPermissionFilterDto filter)
    {
        Queryable = Queryable
            .AsNoTracking()
            .Where(q => q.TenantId == _userContext.TenantId)
            .WhereNotNull(filter.ApplicationId, q => q.ApplicationId == filter.ApplicationId)
            .WhereNotNull(filter.ModelProfileId, q => q.ModelProfileId == filter.ModelProfileId)
            .WhereNotNull(filter.IsEnabled, q => q.IsEnabled == filter.IsEnabled);

        return await PageListAsync<ApplicationModelPermissionFilterDto, ApplicationModelPermissionItemDto>(filter);
    }

    public async Task<ApplicationModelPermission> AddAsync(ApplicationModelPermissionAddDto dto)
    {
        var entity = dto.MapTo<ApplicationModelPermission>();
        await InsertAsync(entity);
        return entity;
    }

    public async Task<int> EditAsync(Guid id, ApplicationModelPermissionUpdateDto dto)
    {
        return await UpdateAsync(id, dto);
    }

    public async Task<ApplicationModelPermissionDetailDto?> GetAsync(Guid id)
    {
        return await FindAsync<ApplicationModelPermissionDetailDto>(q => q.Id == id && q.TenantId == _userContext.TenantId);
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
