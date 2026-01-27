using AIAgentMod.Models.AgentExecutionDtos;

namespace AIAgentMod.Managers;

/// <summary>
/// Agent 执行管理
/// </summary>
public class AgentExecutionManager(
    TenantDbFactory dbContextFactory,
    ILogger<AgentExecutionManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, AgentExecution>(dbContextFactory, userContext, logger)
{
    public async Task<PageList<AgentExecutionItemDto>> FilterAsync(AgentExecutionFilterDto filter)
    {
        Queryable = Queryable.Where(q => q.TenantId == _userContext.TenantId)
            .WhereNotNull(filter.AgentId, q => q.AgentId == filter.AgentId)
            .WhereNotNull(filter.Status, q => q.Status == filter.Status);

        return await PageListAsync<AgentExecutionFilterDto, AgentExecutionItemDto>(filter);
    }

    public async Task<AgentExecution> AddAsync(AgentExecutionAddDto dto)
    {
        var entity = dto.MapTo<AgentExecution>();
        await InsertAsync(entity);
        return entity;
    }

    public async Task<int> EditAsync(Guid id, AgentExecutionUpdateDto dto)
    {
        return await UpdateAsync(id, dto);
    }

    public async Task<AgentExecutionDetailDto?> GetAsync(Guid id)
    {
        return await FindAsync<AgentExecutionDetailDto>(q => q.Id == id && q.TenantId == _userContext.TenantId);
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
