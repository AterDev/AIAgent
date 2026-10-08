using AIAgentMod.Models.AIAgentDtos;
using Share.Exceptions;

namespace AIAgentMod.Managers;
/// <summary>
/// agent
/// </summary>
public class AIAgentManager(
    TenantDbFactory dbContextFactory,
    ILogger<AIAgentManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, AIAgent>(dbContextFactory, userContext, logger)
{
    /// <summary>
    /// Filter agent with paging
    /// </summary>
    public async Task<PageList<AIAgentItemDto>> FilterAsync(AIAgentFilterDto filter)
    {
        EnsureAdminAccess();

        Queryable = _dbSet
            .Where(q => q.TenantId == _userContext.TenantId)
            .WhereNotNull(filter.Enable, q => q.Enable == filter.Enable)
            .WhereNotNull(filter.IsPublic, q => q.IsPublic == filter.IsPublic)
            .WhereNotNull(filter.Name, q => q.Name == filter.Name)
            .WhereNotNull(filter.ModelId, q => q.ModelId == filter.ModelId);

        return await PageListAsync<AIAgentFilterDto, AIAgentItemDto>(filter);
    }

    public async Task<PageList<AIAgentItemDto>> FilterPublicTemplatesAsync(AIAgentFilterDto filter)
    {
        Queryable = _dbSet
            .Where(q => q.TenantId == _userContext.TenantId)
            .Where(q => q.IsPublic)
            .WhereNotNull(filter.Enable, q => q.Enable == filter.Enable)
            .WhereNotNull(filter.Name, q => q.Name == filter.Name)
            .WhereNotNull(filter.ModelId, q => q.ModelId == filter.ModelId);

        return await PageListAsync<AIAgentFilterDto, AIAgentItemDto>(filter);
    }

    /// <summary>
    /// Add agent
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<AIAgent> AddAsync(AIAgentAddDto dto)
    {
        EnsureAdminAccess();

        var entity = dto.MapTo<AIAgent>();
        entity.IsPublic = dto.IsPublic;
        await InsertAsync(entity);

        return entity;
    }

    /// <summary>
    /// edit agent
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<int> EditAsync(Guid id, AIAgentUpdateDto dto)
    {
        EnsureAdminAccess();

        if (await HasPermissionAsync(id))
        {
            dto.ApplicationId = null;
            var result = await UpdateAsync(id, dto);
            return result;
        }
        throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
    }


    /// <summary>
    /// Get agent detail
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<AIAgentDetailDto?> GetAsync(Guid id)
    {
        EnsureAdminAccess();

        if (!await HasPermissionAsync(id))
        {
            throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
        }

        var result = await FindAsync<AIAgentDetailDto>(q => q.Id == id);
        return result;
    }

    /// <summary>
    /// Delete  agent
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="softDelete"></param>
    /// <returns></returns>
    public async Task<bool> DeleteAsync(List<Guid> ids, bool softDelete = true)
    {
        EnsureAdminAccess();

        if (ids.Count == 0)
        {
            return false;
        }
        if (ids.Count == 1)
        {
            Guid id = ids.First();
            if (await HasPermissionAsync(id))
            {
                var result = await DeleteOrUpdateAsync(ids, !softDelete) > 0;
                return result;
            }
            throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
        }
        else
        {
            var ownedIds = await GetOwnedIdsAsync(ids);
            if (ownedIds.Count != 0)
            {
                var result = await DeleteOrUpdateAsync(ownedIds, !softDelete) > 0;
                return result;
            }
            throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
        }
    }

    public override async Task<bool> HasPermissionAsync(Guid id)
    {
        var query = _dbSet
            .Where(q => q.TenantId == _userContext.TenantId)
            .Where(q => q.Id == id);
        return _userContext.IsAdmin && await query.AnyAsync();
    }

    public async Task<List<Guid>> GetOwnedIdsAsync(IEnumerable<Guid> ids)
    {
        if (!ids.Any())
        {
            return [];
        }
        var query = _dbSet
            .Where(q => q.TenantId == _userContext.TenantId)
            .Where(q => ids.Contains(q.Id))
            .Select(q => q.Id);
        return _userContext.IsAdmin
            ? await query.ToListAsync()
            : [];
    }

    private void EnsureAdminAccess()
    {
        if (!_userContext.IsAdmin)
        {
            throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
        }
    }
}