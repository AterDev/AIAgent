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
            .Where(q => q.TenantId == _userContext.TenantId)
            .WhereNotNull(filter.Name, q => q.Name == filter.Name)
            .WhereNotNull(filter.IsEnabled, q => q.IsEnabled == filter.IsEnabled);

        return await PageListAsync<ApplicationFilterDto, ApplicationItemDto>(filter);
    }

    /// <summary>
    /// Add 应用定义
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<ApplicationDetailDto> AddAsync(ApplicationAddDto dto)
    {
        var entity = new Application
        {
            Name = dto.Name,
            Description = dto.Description,
            IsEnabled = dto.IsEnabled,
        };

        await InsertAsync(entity);
        return ToDetailDto(entity);
    }

    /// <summary>
    /// edit 应用定义
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<int> EditAsync(Guid id, ApplicationUpdateDto dto)
    {
        if (!await HasPermissionAsync(id))
        {
            throw new BusinessException(Localizer.NoPermission);
        }

        var entity = await _dbSet.FirstOrDefaultAsync(q => q.Id == id && q.TenantId == _userContext.TenantId)
            ?? throw new BusinessException(Localizer.ApplicationNotFound, StatusCodes.Status404NotFound);

        entity.Name = dto.Name ?? entity.Name;
        entity.Description = dto.Description ?? entity.Description;
        entity.IsEnabled = dto.IsEnabled ?? entity.IsEnabled;
        entity.UpdatedTime = DateTimeOffset.UtcNow;

        return await _dbContext.SaveChangesAsync();
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
            var entity = await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.Id == id && q.TenantId == _userContext.TenantId);
            if (entity is null)
            {
                return null;
            }

            return new ApplicationDetailDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                IsEnabled = entity.IsEnabled,
                CreatedTime = entity.CreatedTime,
                UpdatedTime = entity.UpdatedTime,
                TenantId = entity.TenantId,
            };
        }
        throw new BusinessException(Localizer.NoPermission);
    }

    /// <summary>
    /// 获取列表项
    /// </summary>
    public async Task<ApplicationItemDto?> GetItemAsync(Guid id)
    {
        return await FindAsync<ApplicationItemDto>(q => q.Id == id && q.TenantId == _userContext.TenantId);
    }

    /// <summary>
    /// 根据 Id 获取应用实体
    /// </summary>
    public async Task<Application?> GetEntityAsync(Guid id)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(q => q.Id == id && q.TenantId == _userContext.TenantId);
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
            .Where(q => q.Id == id && q.TenantId == _userContext.TenantId);
        return await query.AnyAsync();
    }

    public async Task<List<Guid>> GetOwnedIdsAsync(IEnumerable<Guid> ids)
    {
        if (!ids.Any())
        {
            return [];
        }
        var query = _dbSet
            .Where(q => ids.Contains(q.Id) && q.TenantId == _userContext.TenantId)
            .Select(q => q.Id);
        return await query.ToListAsync();
    }

    private static ApplicationDetailDto ToDetailDto(Application entity)
    {
        return new ApplicationDetailDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            IsEnabled = entity.IsEnabled,
            CreatedTime = entity.CreatedTime,
            UpdatedTime = entity.UpdatedTime,
            TenantId = entity.TenantId,
        };
    }
}