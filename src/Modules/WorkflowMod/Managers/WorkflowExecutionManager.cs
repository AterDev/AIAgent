using WorkflowMod.Models.WorkflowExecutionDtos;

namespace WorkflowMod.Managers;

/// <summary>
/// 工作流执行管理
/// </summary>
public class WorkflowExecutionManager(
    TenantDbFactory dbContextFactory,
    ILogger<WorkflowExecutionManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, WorkflowExecution>(dbContextFactory, userContext, logger)
{
    public async Task<PageList<WorkflowExecutionItemDto>> FilterAsync(WorkflowExecutionFilterDto filter)
    {
        Queryable = Queryable
            .AsNoTracking()
            .Where(q => q.TenantId == _userContext.TenantId)
            .WhereNotNull(filter.WorkflowId, q => q.WorkflowId == filter.WorkflowId)
            .WhereNotNull(filter.Status, q => q.Status == filter.Status);

        return await PageListAsync<WorkflowExecutionFilterDto, WorkflowExecutionItemDto>(filter);
    }

    public async Task<WorkflowExecution> AddAsync(WorkflowExecutionAddDto dto)
    {
        var entity = dto.MapTo<WorkflowExecution>();
        await InsertAsync(entity);
        return entity;
    }

    public async Task<int> EditAsync(Guid id, WorkflowExecutionUpdateDto dto)
    {
        return await UpdateAsync(id, dto);
    }

    public async Task<WorkflowExecutionDetailDto?> GetAsync(Guid id)
    {
        return await FindAsync<WorkflowExecutionDetailDto>(q => q.Id == id && q.TenantId == _userContext.TenantId);
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
