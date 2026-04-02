using AIAgentMod.Models.AIAgentDtos;
using Perigon.AspNetCore.Constants;
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
    private const string CacheKeyPrefix = "AIAgent:";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(15);

    /// <summary>
    /// Filter agent with paging
    /// </summary>
    public async Task<PageList<AIAgentItemDto>> FilterAsync(AIAgentFilterDto filter)
    {
        Queryable = BuildScopedQuery(filter.ApplicationId)
            .WhereNotNull(filter.Enable, q => q.Enable == filter.Enable)
            .WhereNotNull(filter.IsTemplate, q => q.IsTemplate == filter.IsTemplate)
            .WhereNotNull(filter.Name, q => q.Name == filter.Name)
            .WhereNotNull(filter.ModelId, q => q.ModelId == filter.ModelId)
            .WhereNotNull(filter.UserId, q => q.UserId == filter.UserId);

        return await PageListAsync<AIAgentFilterDto, AIAgentItemDto>(filter);
    }

    public async Task<PageList<AIAgentItemDto>> FilterPublicTemplatesAsync(AIAgentFilterDto filter)
    {
        Queryable = _dbSet
            .Where(q => q.TenantId == _userContext.TenantId)
            .Where(q => q.ApplicationId == null && q.IsTemplate)
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
        var entity = dto.MapTo<AIAgent>();
        ApplyOwnership(entity, dto.ApplicationId);
        await InsertAsync(entity);

        return entity;
    }

    public async Task<AIAgent> CloneTemplateAsync(Guid templateId, Guid applicationId)
    {
        if (_userContext.IsRole(WebConst.Application))
        {
            applicationId = _userContext.UserId;
        }
        else if (!_userContext.IsAdmin)
        {
            throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
        }

        var template = await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == templateId
                && q.TenantId == _userContext.TenantId
                && q.ApplicationId == null
                && q.IsTemplate);

        if (template is null)
        {
            throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
        }

        var entity = new AIAgent
        {
            Name = $"{template.Name}-{Guid.NewGuid().ToString()[..6]}",
            Description = template.Description,
            ModelId = template.ModelId,
            SystemPrompt = template.SystemPrompt,
            Tools = [.. template.Tools],
            Enable = template.Enable,
            IsTemplate = false,
            ApplicationId = applicationId,
            UserId = null,
        };

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
        if (await HasPermissionAsync(id))
        {
            if (_userContext.IsRole(WebConst.Application))
            {
                dto.ApplicationId = _userContext.UserId;
                dto.UserId = null;
            }

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
        if (!await HasPermissionAsync(id))
        {
            throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
        }

        // 从数据库查询
        var result = await FindAsync<AIAgentDetailDto>(q => q.Id == id);
        return result;
    }

    /// <summary>
    /// Delete  agent
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
                var result = await DeleteOrUpdateAsync(ids, !softDelete) > 0;
                return result;
            }
            throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
        }
        else
        {
            var ownedIds = await GetOwnedIdsAsync(ids);
            if (ownedIds.Any())
            {
                var result = await DeleteOrUpdateAsync(ownedIds, !softDelete) > 0;
                return result;
            }
            throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
        }
    }

    public override async Task<bool> HasPermissionAsync(Guid id)
    {
        var query = BuildScopedQuery()
            .Where(q => q.Id == id);
        return await query.AnyAsync();
    }

    public async Task<List<Guid>> GetOwnedIdsAsync(IEnumerable<Guid> ids)
    {
        if (!ids.Any())
        {
            return [];
        }
        var query = BuildScopedQuery()
            .Where(q => ids.Contains(q.Id))
            .Select(q => q.Id);
        return await query.ToListAsync();
    }

    private IQueryable<AIAgent> BuildScopedQuery(Guid? requestedApplicationId = null)
    {
        var query = _dbSet.Where(q => q.TenantId == _userContext.TenantId);

        if (_userContext.IsRole(WebConst.Application))
        {
            return query.Where(q => q.ApplicationId == _userContext.UserId);
        }

        var applicationId = requestedApplicationId;
        if (applicationId.HasValue && applicationId != Guid.Empty)
        {
            return query.Where(q => q.ApplicationId == applicationId);
        }

        if (_userContext.IsAdmin)
        {
            return query;
        }

        return query.Where(q => q.ApplicationId == null && q.UserId == _userContext.UserId);
    }

    private void ApplyOwnership(AIAgent entity, Guid? requestedApplicationId)
    {
        var applicationId = _userContext.IsRole(WebConst.Application)
            ? _userContext.UserId
            : requestedApplicationId;

        if (applicationId.HasValue && applicationId != Guid.Empty)
        {
            entity.ApplicationId = applicationId;
            entity.UserId = null;
            return;
        }

        if (entity.IsTemplate && _userContext.IsAdmin)
        {
            entity.ApplicationId = null;
            entity.UserId = null;
            return;
        }

        entity.ApplicationId = null;
        entity.UserId = _userContext.UserId;
    }
}