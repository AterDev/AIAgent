using KnowledgeBaseMod.Models.RagCollectionDtos;

namespace KnowledgeBaseMod.Managers;

/// <summary>
/// 知识库管理
/// </summary>
public class RagCollectionManager(
    TenantDbFactory dbContextFactory,
    ILogger<RagCollectionManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, RagCollection>(dbContextFactory, userContext, logger)
{
    public async Task<PageList<RagCollectionItemDto>> FilterAsync(RagCollectionFilterDto filter)
    {
        Queryable = Queryable
            .AsNoTracking()
            .Where(q => q.TenantId == _userContext.TenantId)
            .WhereNotNull(filter.Name, q => q.Name == filter.Name)
            .WhereNotNull(filter.IsPublic, q => q.IsPublic == filter.IsPublic)
            .WhereNotNull(filter.IsEnabled, q => q.IsEnabled == filter.IsEnabled);

        return await PageListAsync<RagCollectionFilterDto, RagCollectionItemDto>(filter);
    }

    public async Task<RagCollection> AddAsync(RagCollectionAddDto dto)
    {
        var entity = dto.MapTo<RagCollection>();
        await InsertAsync(entity);
        return entity;
    }

    public async Task<int> EditAsync(Guid id, RagCollectionUpdateDto dto)
    {
        return await UpdateAsync(id, dto);
    }

    public async Task<RagCollectionDetailDto?> GetAsync(Guid id)
    {
        return await FindAsync<RagCollectionDetailDto>(q => q.Id == id && q.TenantId == _userContext.TenantId);
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
