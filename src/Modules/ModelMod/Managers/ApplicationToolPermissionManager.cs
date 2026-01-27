using ModelMod.Models.ApplicationToolPermissionDtos;

namespace ModelMod.Managers;

/// <summary>
/// 应用工具权限管理
/// </summary>
public class ApplicationToolPermissionManager(
    TenantDbFactory dbContextFactory,
    ILogger<ApplicationToolPermissionManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, ApplicationToolPermission>(dbContextFactory, userContext, logger)
{
    public async Task<PageList<ApplicationToolPermissionItemDto>> FilterAsync(ApplicationToolPermissionFilterDto filter)
    {
        Queryable = Queryable.Where(q => q.TenantId == _userContext.TenantId)
            .WhereNotNull(filter.ApplicationId, q => q.ApplicationId == filter.ApplicationId)
            .WhereNotNull(filter.ToolName, q => q.ToolName == filter.ToolName)
            .WhereNotNull(filter.IsEnabled, q => q.IsEnabled == filter.IsEnabled);

        return await PageListAsync<ApplicationToolPermissionFilterDto, ApplicationToolPermissionItemDto>(filter);
    }

    public async Task<ApplicationToolPermission> AddAsync(ApplicationToolPermissionAddDto dto)
    {
        var entity = dto.MapTo<ApplicationToolPermission>();
        await InsertAsync(entity);
        return entity;
    }

    public async Task<int> EditAsync(Guid id, ApplicationToolPermissionUpdateDto dto)
    {
        return await UpdateAsync(id, dto);
    }

    public async Task<ApplicationToolPermissionDetailDto?> GetAsync(Guid id)
    {
        return await FindAsync<ApplicationToolPermissionDetailDto>(q => q.Id == id && q.TenantId == _userContext.TenantId);
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
