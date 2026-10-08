using McpMod.Models;
using McpMod.Models.McpToolDtos;

namespace McpMod.Managers;

/// <summary>
/// MCP 工具管理
/// </summary>
public class McpToolManager(
    TenantDbFactory dbContextFactory,
    ILogger<McpToolManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, McpTool>(dbContextFactory, userContext, logger)
{
    public async Task<PageList<McpToolItemDto>> FilterAsync(McpToolFilterDto filter)
    {
        Queryable = Queryable
            .Where(q => q.TenantId == _userContext.TenantId)
            .WhereNotNull(filter.Name, q => q.Name == filter.Name)
            .WhereNotNull(filter.ToolType, q => q.ToolType == filter.ToolType)
            .WhereNotNull(filter.IsEnabled, q => q.IsEnabled == filter.IsEnabled);

        return await PageListAsync<McpToolFilterDto, McpToolItemDto>(filter);
    }

    public async Task<McpTool> AddAsync(McpToolAddDto dto)
    {
        var entity = dto.MapTo<McpTool>();
        await InsertAsync(entity);
        return entity;
    }

    public async Task<int> EditAsync(Guid id, McpToolUpdateDto dto)
    {
        return await UpdateAsync(id, dto);
    }

    public async Task<McpToolDetailDto?> GetAsync(Guid id)
    {
        return await FindAsync<McpToolDetailDto>(q => q.Id == id && q.TenantId == _userContext.TenantId);
    }

    public async Task<List<ToolDefinitionDto>> GetDefinitionsAsync()
    {
        return await _dbSet
            .AsNoTracking()
            .Where(q => q.TenantId == _userContext.TenantId && q.IsEnabled)
            .Select(q => new ToolDefinitionDto
            {
                Name = q.Name,
                Description = q.Description,
                SchemaJson = q.SchemaJson,
                Version = q.Version,
                ToolType = q.ToolType,
            })
            .ToListAsync();
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
