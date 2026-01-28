using McpMod.Models.ToolCallRecordDtos;

namespace McpMod.Managers;

/// <summary>
/// MCP 调用记录管理
/// </summary>
public class ToolCallRecordManager(
    TenantDbFactory dbContextFactory,
    ILogger<ToolCallRecordManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, ToolCallRecord>(dbContextFactory, userContext, logger)
{
    public async Task<PageList<ToolCallRecordItemDto>> FilterAsync(ToolCallRecordFilterDto filter)
    {
        Queryable = Queryable
            .Where(q => q.TenantId == _userContext.TenantId)
            .WhereNotNull(filter.ToolId, q => q.ToolId == filter.ToolId)
            .WhereNotNull(filter.Status, q => q.Status == filter.Status);

        return await PageListAsync<ToolCallRecordFilterDto, ToolCallRecordItemDto>(filter);
    }

    public async Task<ToolCallRecord> AddAsync(ToolCallRecordAddDto dto)
    {
        var entity = dto.MapTo<ToolCallRecord>();
        await InsertAsync(entity);
        return entity;
    }

    public async Task<int> EditAsync(Guid id, ToolCallRecordUpdateDto dto)
    {
        return await UpdateAsync(id, dto);
    }

    public async Task<ToolCallRecordDetailDto?> GetAsync(Guid id)
    {
        return await FindAsync<ToolCallRecordDetailDto>(q => q.Id == id && q.TenantId == _userContext.TenantId);
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
