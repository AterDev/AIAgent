using AIAgentMod.Models.MCPServerInfoDtos;

namespace AIAgentMod.Managers;

/// <summary>
/// MCP server 管理
/// </summary>
public class MCPServerInfoManager(
    TenantDbFactory dbContextFactory,
    ILogger<MCPServerInfoManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, MCPServerInfo>(dbContextFactory, userContext, logger)
{
    public async Task<PageList<MCPServerInfoItemDto>> FilterAsync(MCPServerInfoFilterDto filter)
    {
        Queryable = Queryable
            .AsNoTracking()
            .Where(q => q.TenantId == _userContext.TenantId)
            .WhereNotNull(filter.AuthType, q => q.AuthType == filter.AuthType)
            .WhereNotNull(filter.TransportType, q => q.TransportType == filter.TransportType)
            .WhereNotNull(filter.DisplayName, q => q.DisplayName == filter.DisplayName)
            .WhereNotNull(filter.IdentityName, q => q.IdentityName == filter.IdentityName);

        return await PageListAsync<MCPServerInfoFilterDto, MCPServerInfoItemDto>(filter);
    }

    public async Task<MCPServerInfo> AddAsync(MCPServerInfoAddDto dto)
    {
        var entity = dto.MapTo<MCPServerInfo>();
        await InsertAsync(entity);
        return entity;
    }

    public async Task<int> EditAsync(Guid id, MCPServerInfoUpdateDto dto)
    {
        return await UpdateAsync(id, dto);
    }

    public async Task<MCPServerInfoDetailDto?> GetAsync(Guid id)
    {
        return await FindAsync<MCPServerInfoDetailDto>(q => q.Id == id && q.TenantId == _userContext.TenantId);
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
