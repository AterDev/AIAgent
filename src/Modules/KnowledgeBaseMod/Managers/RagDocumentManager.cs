using KnowledgeBaseMod.Models.RagDocumentDtos;

namespace KnowledgeBaseMod.Managers;

/// <summary>
/// 文档管理
/// </summary>
public class RagDocumentManager(
    TenantDbFactory dbContextFactory,
    ILogger<RagDocumentManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, RagDocument>(dbContextFactory, userContext, logger)
{
    public async Task<PageList<RagDocumentItemDto>> FilterAsync(RagDocumentFilterDto filter)
    {
        Queryable = Queryable.Where(q => q.TenantId == _userContext.TenantId)
            .WhereNotNull(filter.CollectionId, q => q.CollectionId == filter.CollectionId)
            .WhereNotNull(filter.Name, q => q.Name == filter.Name)
            .WhereNotNull(filter.Status, q => q.Status == filter.Status);

        return await PageListAsync<RagDocumentFilterDto, RagDocumentItemDto>(filter);
    }

    public async Task<RagDocument> AddAsync(RagDocumentAddDto dto)
    {
        var entity = dto.MapTo<RagDocument>();
        await InsertAsync(entity);
        return entity;
    }

    public async Task<int> EditAsync(Guid id, RagDocumentUpdateDto dto)
    {
        return await UpdateAsync(id, dto);
    }

    public async Task<RagDocumentDetailDto?> GetAsync(Guid id)
    {
        return await FindAsync<RagDocumentDetailDto>(q => q.Id == id && q.TenantId == _userContext.TenantId);
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
