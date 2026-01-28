using ModelMod.Models.ApplicationQuotaDtos;

namespace ModelMod.Managers;

/// <summary>
/// 应用配额管理
/// </summary>
public class ApplicationQuotaManager(
    TenantDbFactory dbContextFactory,
    ILogger<ApplicationQuotaManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, ApplicationQuota>(dbContextFactory, userContext, logger)
{
    public async Task<PageList<ApplicationQuotaItemDto>> FilterAsync(ApplicationQuotaFilterDto filter)
    {
        Queryable = Queryable
            .Where(q => q.TenantId == _userContext.TenantId)
            .WhereNotNull(filter.ApplicationId, q => q.ApplicationId == filter.ApplicationId)
            .WhereNotNull(filter.PeriodType, q => q.PeriodType == filter.PeriodType)
            .WhereNotNull(filter.IsEnabled, q => q.IsEnabled == filter.IsEnabled);

        return await PageListAsync<ApplicationQuotaFilterDto, ApplicationQuotaItemDto>(filter);
    }

    public async Task<ApplicationQuota> AddAsync(ApplicationQuotaAddDto dto)
    {
        var entity = dto.MapTo<ApplicationQuota>();
        await InsertAsync(entity);
        return entity;
    }

    public async Task<int> EditAsync(Guid id, ApplicationQuotaUpdateDto dto)
    {
        return await UpdateAsync(id, dto);
    }

    public async Task<ApplicationQuotaDetailDto?> GetAsync(Guid id)
    {
        return await FindAsync<ApplicationQuotaDetailDto>(q => q.Id == id && q.TenantId == _userContext.TenantId);
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
