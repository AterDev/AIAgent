using Entity.ModelMod;
using EntityFramework.AppDbContext;
using Microsoft.AspNetCore.Http;
using Perigon.AspNetCore.Services;
using Share.Exceptions;
using Share.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Share.Implement;

/// <summary>
/// 应用 ApiKey 鉴权服务
/// </summary>
public class ApiKeyService(
    DefaultDbContext dbContext,
    CacheService cacheService,
    ILogger<ApiKeyService> logger
)
{
    private static readonly int[] AllowedExpiryMonths = [1, 3, 6, 12];

    public static int DefaultExpiryMonths => 3;

    public static bool IsAllowedExpiryMonths(int months)
    {
        return AllowedExpiryMonths.Contains(months);
    }

    public static DateTimeOffset BuildExpiresAt(int months)
    {
        if (!IsAllowedExpiryMonths(months))
        {
            throw new BusinessException(Localizer.InvalidApplicationApiKeyExpiryMonths, StatusCodes.Status400BadRequest);
        }

        return DateTimeOffset.UtcNow.AddMonths(months);
    }

    public static string BuildFingerprint(string apiKey)
    {
        return HashCrypto.HashString(apiKey, HashType.SHA256);
    }

    public static bool IsWellFormedApiKey(string? apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey)
            || apiKey.Length != WebConst.ApiKeyLength
            || !apiKey.StartsWith(WebConst.ApiKeyPrefix, StringComparison.Ordinal))
        {
            return false;
        }

        for (var i = WebConst.ApiKeyPrefix.Length; i < apiKey.Length; i++)
        {
            var c = apiKey[i];
            if ((c < '0' || c > '9') && (c < 'a' || c > 'f'))
            {
                return false;
            }
        }

        return true;
    }

    public static string BuildCacheKey(string fingerprint)
    {
        return WebConst.ApplicationApiKeyCachePrefix + fingerprint;
    }

    public async Task<ApiKeyAuthInfo?> AuthenticateAsync(string apiKey, CancellationToken cancellationToken = default)
    {
        if (!IsWellFormedApiKey(apiKey))
        {
            return null;
        }

        var fingerprint = BuildFingerprint(apiKey);
        var cacheKey = BuildCacheKey(fingerprint);
        var cached = await cacheService.GetValueAsync<ApiKeyAuthInfo>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return Validate(apiKey, cached) ? cached : null;
        }

        var entity = await dbContext.ApiKeyAuthIndexes
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.KeyFingerprint == fingerprint, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        var authInfo = await ToAuthInfoAsync(entity, cancellationToken);
        if (!Validate(apiKey, authInfo))
        {
            return null;
        }

        await cacheService.SetValueAsync(cacheKey, authInfo);
        return authInfo;
    }

    public async Task RemoveAsync(string? fingerprint)
    {
        if (string.IsNullOrWhiteSpace(fingerprint))
        {
            return;
        }

        await cacheService.RemoveAsync(BuildCacheKey(fingerprint));
    }

    public async Task RefreshAsync(ApiKeyAuthIndex entity, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(entity.KeyFingerprint))
        {
            return;
        }

        await cacheService.SetValueAsync(
            BuildCacheKey(entity.KeyFingerprint),
            await ToAuthInfoAsync(entity, cancellationToken)
        );
    }

    private bool Validate(string apiKey, ApiKeyAuthInfo authInfo)
    {
        if (authInfo.KeyExpiresAt <= DateTimeOffset.UtcNow)
        {
            logger.LogInformation("Application ApiKey expired for application {ApplicationId}.", authInfo.ApplicationId);
            return false;
        }

        return HashCrypto.Validate(apiKey, authInfo.KeySalt, authInfo.KeyHash);
    }

    private async Task<ApiKeyAuthInfo> ToAuthInfoAsync(
        ApiKeyAuthIndex entity,
        CancellationToken cancellationToken = default
    )
    {
        var tenantType = await dbContext.Tenants
            .AsNoTracking()
            .Where(q => q.Id == entity.TenantId)
            .Select(q => (Entity.TenantType?)q.Type)
            .FirstOrDefaultAsync(cancellationToken);

        return new ApiKeyAuthInfo
        {
            ApiKeyId = entity.Id,
            ApplicationId = entity.ApplicationId,
            Name = entity.ApplicationName,
            ApiKeyName = entity.Name,
            TenantId = entity.TenantId,
            TenantType = (tenantType ?? Entity.TenantType.Normal).ToString(),
            KeyFingerprint = entity.KeyFingerprint,
            KeyHash = entity.KeyHash,
            KeySalt = entity.KeySalt,
            KeyExpiresAt = entity.KeyExpiresAt,
        };
    }
}