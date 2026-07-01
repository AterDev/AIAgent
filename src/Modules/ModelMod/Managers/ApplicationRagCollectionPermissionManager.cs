using ModelMod.Models.ApplicationRagCollectionPermissionDtos;

namespace ModelMod.Managers;

/// <summary>
/// 应用知识库关联管理
/// </summary>
public class ApplicationRagCollectionPermissionManager(
    TenantDbFactory dbContextFactory,
    ILogger<ApplicationRagCollectionPermissionManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, ApplicationRagCollectionPermission>(dbContextFactory, userContext, logger)
{
    public async Task<PageList<ApplicationRagCollectionPermissionItemDto>> FilterAsync(ApplicationRagCollectionPermissionFilterDto filter)
    {
        Queryable = Queryable
            .Where(q => q.TenantId == _userContext.TenantId)
            .WhereNotNull(filter.ApplicationId, q => q.ApplicationId == filter.ApplicationId)
            .WhereNotNull(filter.RagCollectionId, q => q.RagCollectionId == filter.RagCollectionId)
            .WhereNotNull(filter.IsEnabled, q => q.IsEnabled == filter.IsEnabled);

        return await PageListAsync<ApplicationRagCollectionPermissionFilterDto, ApplicationRagCollectionPermissionItemDto>(filter);
    }

    public async Task<ApplicationRagCollectionPermission> AddAsync(ApplicationRagCollectionPermissionAddDto dto)
    {
        var entity = dto.MapTo<ApplicationRagCollectionPermission>();
        await InsertAsync(entity);
        return entity;
    }

    public async Task<int> EditAsync(Guid id, ApplicationRagCollectionPermissionUpdateDto dto)
    {
        return await UpdateAsync(id, dto);
    }

    public async Task<ApplicationRagCollectionPermissionDetailDto?> GetAsync(Guid id)
    {
        return await FindAsync<ApplicationRagCollectionPermissionDetailDto>(q => q.Id == id && q.TenantId == _userContext.TenantId);
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