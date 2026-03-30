using ModelMod.Models.ApplicationDtos;

namespace ModelMod.Managers;
/// <summary>
/// 应用定义
/// </summary>
public class ApplicationManager(
    TenantDbFactory dbContextFactory,
    ILogger<ApplicationManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, Application>(dbContextFactory, userContext, logger)
{
    private const string ClientIdPrefix = "app_";

    /// <summary>
    /// Filter 应用定义 with paging
    /// </summary>
    public async Task<PageList<ApplicationItemDto>> FilterAsync(ApplicationFilterDto filter)
    {
        Queryable = Queryable
            .Where(q => q.TenantId == _userContext.TenantId)
            .WhereNotNull(filter.Name, q => q.Name == filter.Name)
            .WhereNotNull(filter.ClientId, q => q.ClientId == filter.ClientId)
            .WhereNotNull(filter.IsEnabled, q => q.IsEnabled == filter.IsEnabled);

        return await PageListAsync<ApplicationFilterDto, ApplicationItemDto>(filter);
    }

    /// <summary>
    /// Add 应用定义
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<ApplicationCredentialResultDto> AddAsync(ApplicationAddDto dto)
    {
        var clientId = await GenerateClientIdAsync();
        var clientSecret = GenerateClientSecret();
        var secretSalt = HashCrypto.BuildSalt();
        var entity = new Application
        {
            Name = dto.Name,
            Description = dto.Description,
            ClientId = clientId,
            SecretSalt = secretSalt,
            SecretHash = HashCrypto.GeneratePwd(clientSecret, secretSalt),
            SecretUpdatedTime = DateTimeOffset.UtcNow,
            IsEnabled = dto.IsEnabled,
        };

        await InsertAsync(entity);
        return ToCredentialResult(entity, clientSecret);
    }

    /// <summary>
    /// edit 应用定义
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<int> EditAsync(Guid id, ApplicationUpdateDto dto)
    {
        if (await HasPermissionAsync(id))
        {
            return await UpdateAsync(id, dto);
        }
        throw new BusinessException(Localizer.NoPermission);
    }


    /// <summary>
    /// Get 应用定义 detail
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<ApplicationDetailDto?> GetAsync(Guid id)
    {
        if (await HasPermissionAsync(id))
        {
            return await FindAsync<ApplicationDetailDto>(q => q.Id == id && q.TenantId == _userContext.TenantId);
        }
        throw new BusinessException(Localizer.NoPermission);
    }

    /// <summary>
    /// 获取列表项
    /// </summary>
    public async Task<ApplicationItemDto?> GetItemAsync(Guid id)
    {
        return await FindAsync<ApplicationItemDto>(q => q.Id == id && q.TenantId == _userContext.TenantId);
    }

    /// <summary>
    /// 根据 Id 获取应用实体
    /// </summary>
    public async Task<Application?> GetEntityAsync(Guid id)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(q => q.Id == id && q.TenantId == _userContext.TenantId);
    }

    /// <summary>
    /// 重置应用密钥
    /// </summary>
    public async Task<ApplicationCredentialResultDto> ResetSecretAsync(Guid id)
    {
        if (!await HasPermissionAsync(id))
        {
            throw new BusinessException(Localizer.NoPermission);
        }

        var entity = await _dbSet.FirstOrDefaultAsync(q => q.Id == id && q.TenantId == _userContext.TenantId)
            ?? throw new BusinessException("Application not found");

        var clientSecret = GenerateClientSecret();
        entity.SecretSalt = HashCrypto.BuildSalt();
        entity.SecretHash = HashCrypto.GeneratePwd(clientSecret, entity.SecretSalt);
        entity.SecretUpdatedTime = DateTimeOffset.UtcNow;

        await Db.SaveChangesAsync();
        return ToCredentialResult(entity, clientSecret);
    }

    /// <summary>
    /// 校验应用凭证
    /// </summary>
    public async Task<Application?> AuthenticateAsync(string clientId, string clientSecret)
    {
        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
        {
            return null;
        }

        var entity = await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.ClientId == clientId && q.IsEnabled);

        if (entity is null)
        {
            return null;
        }

        return HashCrypto.Validate(clientSecret, entity.SecretSalt, entity.SecretHash)
            ? entity
            : null;
    }

    /// <summary>
    /// Delete  应用定义
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="softDelete"></param>
    /// <returns></returns>
    public async Task<bool?> DeleteAsync(List<Guid> ids, bool softDelete = true)
    {
        if (!ids.Any())
        {
            return false;
        }
        if (ids.Count() == 1)
        {
            Guid id = ids.First();
            if (await HasPermissionAsync(id))
            {
                return await DeleteOrUpdateAsync(ids, !softDelete) > 0;
            }
            throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
        }
        else
        {
            var ownedIds = await GetOwnedIdsAsync(ids);
            if (ownedIds.Any())
            {
                return await DeleteOrUpdateAsync(ownedIds, !softDelete) > 0;
            }
            throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
        }
    }

    public override async Task<bool> HasPermissionAsync(Guid id)
    {
        var query = _dbSet
            .Where(q => q.Id == id && q.TenantId == _userContext.TenantId);
        return await query.AnyAsync();
    }

    public async Task<List<Guid>> GetOwnedIdsAsync(IEnumerable<Guid> ids)
    {
        if (!ids.Any())
        {
            return [];
        }
        var query = _dbSet
            .Where(q => ids.Contains(q.Id) && q.TenantId == _userContext.TenantId)
            .Select(q => q.Id);
        return await query.ToListAsync();
    }

    private async Task<string> GenerateClientIdAsync()
    {
        string clientId;
        do
        {
            clientId = ClientIdPrefix + HashCrypto.GetRandom(16, useNum: true, useLow: true, useUpp: false);
        } while (await _dbSet.AnyAsync(q => q.ClientId == clientId));

        return clientId;
    }

    private static string GenerateClientSecret()
    {
        return HashCrypto.GetRandom(40, useNum: true, useLow: true, useUpp: true);
    }

    private static ApplicationCredentialResultDto ToCredentialResult(Application entity, string clientSecret)
    {
        return new ApplicationCredentialResultDto
        {
            Id = entity.Id,
            Name = entity.Name,
            ClientId = entity.ClientId,
            ClientSecret = clientSecret,
            IsEnabled = entity.IsEnabled,
            SecretUpdatedTime = entity.SecretUpdatedTime,
        };
    }
}