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
            .Where(q => q.TenantId == _userContext.TenantId)
            .WhereNotNull(filter.ApplicationId, q => q.ApplicationId == filter.ApplicationId)
            .WhereNotNull(filter.AIModelInfoId, q => q.AIModelInfoId == filter.AIModelInfoId)
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

    public async Task SyncAsync(ApplicationModelPermissionSyncDto dto)
    {
        var selectedIds = dto.AIModelInfoIds
            .Where(q => q != Guid.Empty)
            .Distinct()
            .ToList();

        await ExecuteInTransactionAsync(async () =>
        {
            var existingPermissions = await _dbSet
                .Where(q => q.TenantId == _userContext.TenantId && q.ApplicationId == dto.ApplicationId)
                .ToListAsync();

            var existingModelIds = existingPermissions.Select(q => q.AIModelInfoId).ToHashSet();
            var removedIds = existingPermissions
                .Where(q => !selectedIds.Contains(q.AIModelInfoId))
                .Select(q => q.Id)
                .ToList();

            if (removedIds.Count > 0)
            {
                await _dbSet.Where(q => removedIds.Contains(q.Id)).ExecuteDeleteAsync();
            }

            var disabledIds = existingPermissions
                .Where(q => selectedIds.Contains(q.AIModelInfoId) && !q.IsEnabled)
                .Select(q => q.Id)
                .ToList();

            if (disabledIds.Count > 0)
            {
                await _dbSet
                    .Where(q => disabledIds.Contains(q.Id))
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(q => q.IsEnabled, true)
                        .SetProperty(q => q.UpdatedTime, DateTimeOffset.UtcNow));
            }

            var newPermissions = selectedIds
                .Where(q => !existingModelIds.Contains(q))
                .Select(modelId => new ApplicationModelPermission
                {
                    ApplicationId = dto.ApplicationId,
                    AIModelInfoId = modelId,
                    IsEnabled = true,
                    TenantId = _userContext.TenantId,
                })
                .ToList();

            if (newPermissions.Count > 0)
            {
                await BulkInsertAsync(newPermissions);
            }
        });
    }

    public async Task<bool> DeleteAsync(List<Guid> ids, bool softDelete = true)
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
