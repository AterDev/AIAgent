using ModelMod.Models.ApplicationDtos;

namespace ModelMod.Managers;
/// <summary>
/// 应用定义
/// </summary>
public class ApplicationManager(
    TenantDbFactory dbContextFactory,
    ILogger<ApplicationManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, Application>(dbContextFactory, userContext, logger)
{
    /// <summary>
    /// Filter 应用定义 with paging
    /// </summary>
    public async Task<PageList<ApplicationItemDto>> FilterAsync(ApplicationFilterDto filter)
    {
        Queryable = Queryable
            .AsNoTracking()
            .WhereNotNull(filter.Name, q => q.Name == filter.Name)
            .WhereNotNull(filter.AccessKey, q => q.AccessKey == filter.AccessKey)
            .WhereNotNull(filter.IsEnabled, q => q.IsEnabled == filter.IsEnabled);

        return await PageListAsync<ApplicationFilterDto, ApplicationItemDto>(filter);
    }

    /// <summary>
    /// Add 应用定义
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<Application> AddAsync(ApplicationAddDto dto)
    {
        var entity = dto.MapTo<Application>();

        await InsertAsync(entity);
        return entity;
    }

    /// <summary>
    /// edit 应用定义
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<int> EditAsync(Guid id, ApplicationUpdateDto dto)
    {
        if (await HasPermissionAsync(id))
        {
            return await UpdateAsync(id, dto);
        }
        throw new BusinessException(Localizer.NoPermission);
    }


    /// <summary>
    /// Get 应用定义 detail
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<ApplicationDetailDto?> GetAsync(Guid id)
    {
        if (await HasPermissionAsync(id))
        {
            return await FindAsync<ApplicationDetailDto>(q => q.Id == id);
        }
        throw new BusinessException(Localizer.NoPermission);
    }

    /// <summary>
    /// Delete  应用定义
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