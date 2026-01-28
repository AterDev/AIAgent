using KnowledgeBaseMod.Models.RagChunkDtos;

namespace KnowledgeBaseMod.Managers;

/// <summary>
/// 分块管理
/// </summary>
public class RagChunkManager(
    TenantDbFactory dbContextFactory,
    ILogger<RagChunkManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, RagChunk>(dbContextFactory, userContext, logger)
{
    public async Task<PageList<RagChunkItemDto>> FilterAsync(RagChunkFilterDto filter)
    {
        Queryable = Queryable
            .AsNoTracking()
            .Where(q => q.TenantId == _userContext.TenantId)
            .WhereNotNull(filter.DocumentId, q => q.DocumentId == filter.DocumentId);

        return await PageListAsync<RagChunkFilterDto, RagChunkItemDto>(filter);
    }

    public async Task<RagChunk> AddAsync(RagChunkAddDto dto)
    {
        var entity = dto.MapTo<RagChunk>();
        await InsertAsync(entity);
        return entity;
    }

    public async Task<int> EditAsync(Guid id, RagChunkUpdateDto dto)
    {
        return await UpdateAsync(id, dto);
    }

    public async Task<RagChunkDetailDto?> GetAsync(Guid id)
    {
        return await FindAsync<RagChunkDetailDto>(q => q.Id == id && q.TenantId == _userContext.TenantId);
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
