using KnowledgeBaseMod.Models.RagCollectionDtos;
using Perigon.AspNetCore.Constants;
using Entity.ModelMod;
using EFCore.BulkExtensions;

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
        Queryable = BuildScopedQuery(filter.ApplicationId)
            .WhereNotNull(filter.Name, q => q.Name == filter.Name)
            .WhereNotNull(filter.IsPublic, q => q.IsPublic == filter.IsPublic)
            .WhereNotNull(filter.IsEnabled, q => q.IsEnabled == filter.IsEnabled);

        return await PageListAsync<RagCollectionFilterDto, RagCollectionItemDto>(filter);
    }

    public async Task<RagCollection> AddAsync(RagCollectionAddDto dto)
    {
        var applicationId = _userContext.IsRole(WebConst.Application)
            ? _userContext.UserId
            : dto.ApplicationId;

        var entity = dto.MapTo<RagCollection>();
        await ExecuteInTransactionAsync(async () =>
        {
            await InsertAsync(entity);

            if (applicationId.HasValue && applicationId != Guid.Empty)
            {
                var link = new ApplicationRagCollectionPermission
                {
                    ApplicationId = applicationId.Value,
                    RagCollectionId = entity.Id,
                    IsEnabled = true,
                };

                if (_isMultiTenant)
                {
                    link.TenantId = _userContext.TenantId;
                }

                await _dbContext.BulkInsertAsync([link]);
            }
        });

        return entity;
    }

    public async Task<int> EditAsync(Guid id, RagCollectionUpdateDto dto)
    {
        if (!await HasPermissionAsync(id))
        {
            throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
        }

        return await UpdateAsync(id, dto);
    }

    public async Task<RagCollectionDetailDto?> GetAsync(Guid id)
    {
        if (!await HasPermissionAsync(id))
        {
            throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
        }

        return await FindAsync<RagCollectionDetailDto>(q => q.Id == id && q.TenantId == _userContext.TenantId);
    }

    public async Task<bool?> DeleteAsync(List<Guid> ids, bool softDelete = true)
    {
        if (!ids.Any())
        {
            return false;
        }

        var ownedIds = await BuildScopedQuery()
            .Where(q => ids.Contains(q.Id))
            .Select(q => q.Id)
            .ToListAsync();

        if (!ownedIds.Any())
        {
            throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
        }

        return await DeleteOrUpdateAsync(ownedIds, !softDelete) > 0;
    }

    public override async Task<bool> HasPermissionAsync(Guid id)
    {
        return await BuildScopedQuery().AnyAsync(q => q.Id == id);
    }

    private IQueryable<RagCollection> BuildScopedQuery(Guid? requestedApplicationId = null)
    {
        var query = _dbSet.Where(q => q.TenantId == _userContext.TenantId);

        if (_userContext.IsRole(WebConst.Application))
        {
            return query.Where(q => _dbContext.ApplicationRagCollectionPermissions
                .Any(link => link.TenantId == _userContext.TenantId
                    && link.IsEnabled
                    && link.ApplicationId == _userContext.UserId
                    && link.RagCollectionId == q.Id));
        }

        var applicationId = requestedApplicationId;
        if (applicationId.HasValue && applicationId != Guid.Empty)
        {
            return query.Where(q => _dbContext.ApplicationRagCollectionPermissions
                .Any(link => link.TenantId == _userContext.TenantId
                    && link.IsEnabled
                    && link.ApplicationId == applicationId
                    && link.RagCollectionId == q.Id));
        }

        return query;
    }
}
