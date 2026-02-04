using KnowledgeBaseMod.Models.RagAgentConfigDtos;

namespace KnowledgeBaseMod.Managers;
/// <summary>
/// RAG 模型配置
/// </summary>
public class RagAgentConfigManager(
    TenantDbFactory dbContextFactory, 
    ILogger<RagAgentConfigManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, RagAgentConfig>(dbContextFactory, userContext, logger)
{
    /// <summary>
    /// Filter RAG 模型配置 with paging
    /// </summary>
    public async Task<PageList<RagAgentConfigItemDto>> FilterAsync(RagAgentConfigFilterDto filter)
    {        
        Queryable = Queryable
            .WhereNotNull(filter.Key, q => q.Key == filter.Key);

        return await PageListAsync<RagAgentConfigFilterDto, RagAgentConfigItemDto>(filter);
    }

    /// <summary>
    /// Add RAG 模型配置
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<RagAgentConfig> AddAsync(RagAgentConfigAddDto dto)
    {
        var entity = dto.MapTo<RagAgentConfig>();
        
        await InsertAsync(entity);
        return entity;
    }

    /// <summary>
    /// edit RAG 模型配置
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<int> EditAsync(Guid id, RagAgentConfigUpdateDto dto)
    {
        if (await HasPermissionAsync(id))
        {
            return await UpdateAsync(id, dto);
        }
        throw new BusinessException(Localizer.NoPermission);
    }


    /// <summary>
    /// Get RAG 模型配置 detail
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<RagAgentConfigDetailDto?> GetAsync(Guid id)
    {
        if (await HasPermissionAsync(id))
        {
            return await FindAsync<RagAgentConfigDetailDto>(q => q.Id == id);
        }
        throw new BusinessException(Localizer.NoPermission);
    }

    /// <summary>
    /// Delete  RAG 模型配置
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