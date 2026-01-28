using WorkflowMod.Models.WorkflowDtos;
using WorkflowMod.Services;

namespace WorkflowMod.Managers;

/// <summary>
/// 工作流管理
/// </summary>
public class WorkflowManager(
    TenantDbFactory dbContextFactory,
    ILogger<WorkflowManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, Workflow>(dbContextFactory, userContext, logger)
{
    public async Task<PageList<WorkflowItemDto>> FilterAsync(WorkflowFilterDto filter)
    {
        Queryable = Queryable
            .AsNoTracking()
            .Where(q => q.TenantId == _userContext.TenantId)
            .WhereNotNull(filter.Name, q => q.Name == filter.Name)
            .WhereNotNull(filter.IsPublished, q => q.IsPublished == filter.IsPublished);

        return await PageListAsync<WorkflowFilterDto, WorkflowItemDto>(filter);
    }

    public async Task<Workflow> AddAsync(WorkflowAddDto dto)
    {
        WorkflowDefinitionValidator.ValidateOrThrow(dto.DefinitionJson);
        var entity = dto.MapTo<Workflow>();
        await InsertAsync(entity);
        return entity;
    }

    public async Task<int> EditAsync(Guid id, WorkflowUpdateDto dto)
    {
        if (dto.DefinitionJson is not null)
        {
            WorkflowDefinitionValidator.ValidateOrThrow(dto.DefinitionJson);
        }
        return await UpdateAsync(id, dto);
    }

    public async Task<WorkflowDetailDto?> GetAsync(Guid id)
    {
        return await FindAsync<WorkflowDetailDto>(q => q.Id == id && q.TenantId == _userContext.TenantId);
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
