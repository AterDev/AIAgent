using SystemMod.Models.SystemConfigDtos;

namespace SystemMod.Managers;
/// <summary>
/// 系统配置
/// </summary>
public class SystemConfigManager(
    TenantDbFactory dbContextFactory,
    ILogger<SystemConfigManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, SystemConfig>(dbContextFactory, userContext, logger)
{
    /// <summary>
    /// Filter 系统配置 with paging
    /// </summary>
    public async Task<PageList<SystemConfigItemDto>> FilterAsync(SystemConfigFilterDto filter)
    {
        Queryable = Queryable
            .WhereNotNull(filter.GroupName, q => q.GroupName == filter.GroupName)
            .WhereNotNull(filter.Key, q => q.Key == filter.Key)
            .WhereNotNull(filter.Valid, q => q.Valid == filter.Valid)
            .WhereNotNull(filter.IsSystem, q => q.IsSystem == filter.IsSystem);

        return await PageListAsync<SystemConfigFilterDto, SystemConfigItemDto>(filter);
    }

    /// <summary>
    /// Add 系统配置
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<SystemConfig> AddAsync(SystemConfigAddDto dto)
    {
        EnsureAdminForSystem(dto.IsSystem);
        var entity = dto.MapTo<SystemConfig>();

        await InsertAsync(entity);
        _logger.LogInformation(
            "SystemConfig created. Id={Id}, Key={Key}, Group={Group}, IsSystem={IsSystem}, UserId={UserId}",
            entity.Id,
            entity.Key,
            entity.GroupName,
            entity.IsSystem,
            _userContext.UserId
        );
        return entity;
    }

    /// <summary>
    /// edit 系统配置
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<int> EditAsync(Guid id, SystemConfigUpdateDto dto)
    {
        if (dto.IsSystem == true)
        {
            EnsureAdminForSystem(true);
        }
        if (await HasPermissionAsync(id))
        {
            await EnsureAdminForSystemAsync(id);
            _logger.LogInformation(
                "SystemConfig update requested. Id={Id}, UserId={UserId}",
                id,
                _userContext.UserId
            );
            return await UpdateAsync(id, dto);
        }
        throw new BusinessException(Localizer.NoPermission);
    }


    /// <summary>
    /// Get 系统配置 detail
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<SystemConfigDetailDto?> GetAsync(Guid id)
    {
        if (await HasPermissionAsync(id))
        {
            return await FindAsync<SystemConfigDetailDto>(q => q.Id == id);
        }
        throw new BusinessException(Localizer.NoPermission);
    }

    /// <summary>
    /// Delete  系统配置
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
        await EnsureAdminForSystemAsync(ids);
        if (ids.Count() == 1)
        {
            Guid id = ids.First();
            if (await HasPermissionAsync(id))
            {
                _logger.LogInformation(
                    "SystemConfig delete requested. Id={Id}, SoftDelete={SoftDelete}, UserId={UserId}",
                    id,
                    softDelete,
                    _userContext.UserId
                );
                return await DeleteOrUpdateAsync(ids, !softDelete) > 0;
            }
            throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
        }
        else
        {
            var ownedIds = await GetOwnedIdsAsync(ids);
            if (ownedIds.Any())
            {
                _logger.LogInformation(
                    "SystemConfig batch delete requested. Count={Count}, SoftDelete={SoftDelete}, UserId={UserId}",
                    ownedIds.Count,
                    softDelete,
                    _userContext.UserId
                );
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

    private void EnsureAdminForSystem(bool isSystem)
    {
        if (isSystem && !_userContext.IsAdmin)
        {
            throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
        }
    }

    private async Task EnsureAdminForSystemAsync(Guid id)
    {
        var isSystem = await _dbSet.Where(q => q.Id == id).Select(q => q.IsSystem).FirstOrDefaultAsync();
        EnsureAdminForSystem(isSystem);
    }

    private async Task EnsureAdminForSystemAsync(IEnumerable<Guid> ids)
    {
        var idList = ids.ToList();
        if (idList.Count == 0)
        {
            return;
        }
        var hasSystem = await _dbSet.Where(q => idList.Contains(q.Id) && q.IsSystem).AnyAsync();
        EnsureAdminForSystem(hasSystem);
    }
}