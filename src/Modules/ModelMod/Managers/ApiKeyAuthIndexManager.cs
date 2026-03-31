using ModelMod.Models.ApplicationApiKeyDtos;
using Perigon.AspNetCore.Constants;

namespace ModelMod.Managers;

/// <summary>
/// 应用 ApiKey 管理
/// </summary>
public class ApiKeyAuthIndexManager(
    DefaultDbContext dbContext,
    ApiKeyService apiKeyService,
    ILogger<ApiKeyAuthIndexManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext>(dbContext, logger)
{
    private readonly IUserContext _userContext = userContext;
    private DbSet<ApiKeyAuthIndex> ApiKeyDbSet => _dbContext.Set<ApiKeyAuthIndex>();

    public async Task<List<ApplicationApiKeyItemDto>> ListAsync(
        Guid applicationId,
        CancellationToken cancellationToken = default
    )
    {
        return await ApiKeyDbSet
            .AsNoTracking()
            .Where(q => q.ApplicationId == applicationId && q.TenantId == _userContext.TenantId)
            .OrderByDescending(q => q.CreatedTime)
            .Select(q => new ApplicationApiKeyItemDto
            {
                Id = q.Id,
                ApplicationId = q.ApplicationId,
                Name = q.Name,
                KeyUpdatedTime = q.KeyUpdatedTime,
                KeyExpiresAt = q.KeyExpiresAt,
                IsExpired = q.KeyExpiresAt <= DateTimeOffset.UtcNow,
                CreatedTime = q.CreatedTime,
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ApplicationApiKeyCredentialResultDto> AddAsync(
        Application application,
        ApplicationApiKeyAddDto dto,
        CancellationToken cancellationToken = default
    )
    {
        var credential = await GenerateApiKeyAsync(dto.ApiKeyExpiresInMonths, cancellationToken);
        var entity = new ApiKeyAuthIndex
        {
            ApplicationId = application.Id,
            ApplicationName = application.Name,
            Name = dto.Name,
            TenantId = application.TenantId,
            KeyFingerprint = credential.KeyFingerprint,
            KeyHash = credential.KeyHash,
            KeySalt = credential.KeySalt,
            KeyUpdatedTime = credential.KeyUpdatedTime,
            KeyExpiresAt = credential.KeyExpiresAt,
        };

        ApiKeyDbSet.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await apiKeyService.RefreshAsync(entity, cancellationToken);

        return new ApplicationApiKeyCredentialResultDto
        {
            Id = entity.Id,
            ApplicationId = entity.ApplicationId,
            ApplicationName = entity.ApplicationName,
            Name = entity.Name,
            ApiKey = credential.ApiKey,
            KeyUpdatedTime = entity.KeyUpdatedTime,
            KeyExpiresAt = entity.KeyExpiresAt,
        };
    }

    public async Task<bool> DeleteAsync(
        Guid applicationId,
        Guid apiKeyId,
        CancellationToken cancellationToken = default
    )
    {
        var entity = await ApiKeyDbSet
            .FirstOrDefaultAsync(
                q => q.Id == apiKeyId && q.ApplicationId == applicationId && q.TenantId == _userContext.TenantId,
                cancellationToken);

        if (entity is null)
        {
            throw new BusinessException(Localizer.ApplicationApiKeyNotFound, StatusCodes.Status404NotFound);
        }

        entity.IsDeleted = true;
        entity.UpdatedTime = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        await apiKeyService.RemoveAsync(entity.KeyFingerprint);
        return true;
    }

    public async Task<int> DeleteByApplicationIdAsync(
        Guid applicationId,
        CancellationToken cancellationToken = default
    )
    {
        var fingerprints = await ApiKeyDbSet
            .AsNoTracking()
            .Where(q => q.ApplicationId == applicationId && q.TenantId == _userContext.TenantId)
            .Select(q => q.KeyFingerprint)
            .ToListAsync(cancellationToken);

        var rows = await ApiKeyDbSet
            .Where(q => q.ApplicationId == applicationId && q.TenantId == _userContext.TenantId)
            .ExecuteUpdateAsync(
                s => s
                    .SetProperty(q => q.IsDeleted, true)
                    .SetProperty(q => q.UpdatedTime, DateTimeOffset.UtcNow),
                cancellationToken);

        foreach (var fingerprint in fingerprints)
        {
            await apiKeyService.RemoveAsync(fingerprint);
        }

        return rows;
    }

    public async Task SyncApplicationAsync(
        Application application,
        CancellationToken cancellationToken = default
    )
    {
        var entities = await ApiKeyDbSet
            .Where(q => q.ApplicationId == application.Id && q.TenantId == application.TenantId)
            .ToListAsync(cancellationToken);

        if (entities.Count == 0)
        {
            return;
        }

        foreach (var entity in entities)
        {
            entity.ApplicationName = application.Name;
            entity.TenantId = application.TenantId;
            entity.UpdatedTime = DateTimeOffset.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        foreach (var entity in entities)
        {
            await apiKeyService.RefreshAsync(entity, cancellationToken);
        }
    }

    private async Task<ApplicationApiKeyCredential> GenerateApiKeyAsync(
        int expiresInMonths,
        CancellationToken cancellationToken = default
    )
    {
        var keyExpiresAt = ApiKeyService.BuildExpiresAt(expiresInMonths);
        string apiKey;
        string keyFingerprint;
        do
        {
            apiKey = WebConst.ApiKeyPrefix + Guid.CreateVersion7().ToString("N");
            keyFingerprint = ApiKeyService.BuildFingerprint(apiKey);
        }
        while (await ApiKeyDbSet.IgnoreQueryFilters().AnyAsync(q => q.KeyFingerprint == keyFingerprint, cancellationToken));

        var keySalt = HashCrypto.BuildSalt();
        return new ApplicationApiKeyCredential(
            apiKey,
            keyFingerprint,
            HashCrypto.GeneratePwd(apiKey, keySalt),
            keySalt,
            DateTimeOffset.UtcNow,
            keyExpiresAt);
    }

    private sealed record ApplicationApiKeyCredential(
        string ApiKey,
        string KeyFingerprint,
        string KeyHash,
        string KeySalt,
        DateTimeOffset KeyUpdatedTime,
        DateTimeOffset KeyExpiresAt);
}