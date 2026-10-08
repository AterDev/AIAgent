using AIAgentMod.Models.AIAgentDtos;
using Perigon.AspNetCore.Constants;
using Share.Exceptions;

namespace AIAgentMod.Managers;

/// <summary>
/// 应用侧 Agent 管理
/// </summary>
public class ApplicationAgentManager(
    TenantDbFactory dbContextFactory,
    ILogger<ApplicationAgentManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, ApplicationAgent>(dbContextFactory, userContext, logger)
{
    public async Task<PageList<AIAgentItemDto>> FilterAsync(AIAgentFilterDto filter)
    {
        var applicationId = ResolveApplicationId(filter.ApplicationId, requireAdminApplicationId: false);

        Queryable = BuildScopedQuery(applicationId)
            .WhereNotNull(filter.Enable, q => q.Enable == filter.Enable)
            .WhereNotNull(filter.Name, q => q.Name == filter.Name)
            .WhereNotNull(filter.ModelId, q => q.ModelId == filter.ModelId);

        return await PageListAsync<AIAgentFilterDto, AIAgentItemDto>(filter);
    }

    public async Task<ApplicationAgent> AddAsync(AIAgentAddDto dto)
    {
        var applicationId = ResolveApplicationId(dto.ApplicationId, requireAdminApplicationId: true);
        await EnsureApplicationExistsAsync(applicationId);

        var entity = dto.MapTo<ApplicationAgent>();
        entity.ApplicationId = applicationId;
        entity.UserId = ResolveActorUserId();

        await InsertAsync(entity);
        return entity;
    }

    public async Task<ApplicationAgent> ClonePublicAsync(Guid publicAgentId, Guid? requestedApplicationId = null)
    {
        var applicationId = ResolveApplicationId(requestedApplicationId, requireAdminApplicationId: true);
        await EnsureApplicationExistsAsync(applicationId);

        var template = await _dbContext.AIAgents
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == publicAgentId
                && q.TenantId == _userContext.TenantId
                && q.IsPublic);

        if (template is null)
        {
            throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
        }

        var cloneName = await GenerateCloneNameAsync(applicationId, template.Name);
        var entity = new ApplicationAgent
        {
            Name = cloneName,
            Description = template.Description,
            ModelId = template.ModelId,
            SystemPrompt = template.SystemPrompt,
            Tools = [.. template.Tools],
            Enable = template.Enable,
            ApplicationId = applicationId,
            UserId = ResolveActorUserId(),
        };

        await InsertAsync(entity);
        return entity;
    }

    public async Task<int> EditAsync(Guid id, AIAgentUpdateDto dto)
    {
        if (!await HasPermissionAsync(id))
        {
            throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
        }

        dto.ApplicationId = null;
        dto.IsPublic = null;
        return await UpdateAsync(id, dto);
    }

    public async Task<AIAgentDetailDto?> GetAsync(Guid id)
    {
        if (!await HasPermissionAsync(id))
        {
            throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
        }

        return await FindAsync<AIAgentDetailDto>(q => q.Id == id);
    }

    public async Task<bool> DeleteAsync(List<Guid> ids, bool softDelete = true)
    {
        if (ids.Count == 0)
        {
            return false;
        }

        if (ids.Count == 1)
        {
            var id = ids[0];
            if (!await HasPermissionAsync(id))
            {
                throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
            }

            return await DeleteOrUpdateAsync(ids, !softDelete) > 0;
        }

        var ownedIds = await GetOwnedIdsAsync(ids);
        if (ownedIds.Count == 0)
        {
            throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
        }

        return await DeleteOrUpdateAsync(ownedIds, !softDelete) > 0;
    }

    public override async Task<bool> HasPermissionAsync(Guid id)
    {
        return await BuildScopedQuery()
            .Where(q => q.Id == id)
            .AnyAsync();
    }

    public async Task<List<Guid>> GetOwnedIdsAsync(IEnumerable<Guid> ids)
    {
        var idList = ids.ToList();
        if (idList.Count == 0)
        {
            return [];
        }

        return await BuildScopedQuery()
            .Where(q => idList.Contains(q.Id))
            .Select(q => q.Id)
            .ToListAsync();
    }

    private IQueryable<ApplicationAgent> BuildScopedQuery(Guid? requestedApplicationId = null)
    {
        var query = _dbSet.Where(q => q.TenantId == _userContext.TenantId);

        if (_userContext.IsRole(WebConst.Application))
        {
            return query.Where(q => q.ApplicationId == _userContext.UserId);
        }

        if (_userContext.IsAdmin)
        {
            if (requestedApplicationId.HasValue && requestedApplicationId != Guid.Empty)
            {
                return query.Where(q => q.ApplicationId == requestedApplicationId.Value);
            }

            return query;
        }

        return query.Where(q => false);
    }

    private Guid ResolveApplicationId(Guid? requestedApplicationId, bool requireAdminApplicationId)
    {
        if (_userContext.IsRole(WebConst.Application))
        {
            return _userContext.UserId;
        }

        if (_userContext.IsAdmin)
        {
            if (requestedApplicationId.HasValue && requestedApplicationId != Guid.Empty)
            {
                return requestedApplicationId.Value;
            }

            if (!requireAdminApplicationId)
            {
                return Guid.Empty;
            }

            throw new BusinessException("Application is required");
        }

        throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
    }

    private Guid? ResolveActorUserId()
    {
        return _userContext.IsRole(WebConst.Application)
            ? null
            : _userContext.UserId == Guid.Empty
                ? null
                : _userContext.UserId;
    }

    private async Task EnsureApplicationExistsAsync(Guid applicationId)
    {
        if (applicationId == Guid.Empty)
        {
            return;
        }

        var exists = await _dbContext.Applications.AnyAsync(q => q.Id == applicationId
            && q.TenantId == _userContext.TenantId
            && q.IsEnabled);

        if (!exists)
        {
            throw new BusinessException("Application not found", StatusCodes.Status404NotFound);
        }
    }

    private async Task<string> GenerateCloneNameAsync(Guid applicationId, string sourceName)
    {
        var baseName = sourceName;
        if (!await _dbSet.AnyAsync(q => q.TenantId == _userContext.TenantId
            && q.ApplicationId == applicationId
            && q.Name == baseName))
        {
            return baseName;
        }

        for (var i = 1; i < 1000; i++)
        {
            var candidate = $"{sourceName}-{i}";
            var exists = await _dbSet.AnyAsync(q => q.TenantId == _userContext.TenantId
                && q.ApplicationId == applicationId
                && q.Name == candidate);
            if (!exists)
            {
                return candidate;
            }
        }

        return $"{sourceName}-{Guid.NewGuid():N}"[..Math.Min(sourceName.Length + 9, 100)];
    }
}