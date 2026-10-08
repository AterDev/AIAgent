using Entity;
using EntityFramework.AppDbContext;
using EntityFramework.AppDbFactory;
using Microsoft.EntityFrameworkCore;
using Perigon.AspNetCore.Constants;
using Perigon.AspNetCore.Services;

namespace Share.Services;

/// <summary>
/// Resolves tenant metadata and keeps it in the process cache.
/// </summary>
public sealed class TenantService(
    DefaultDbContext dbContext,
    CacheService cache,
    ILogger<TenantService> logger
) : ITenantResolver
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromDays(1);

    /// <summary>
    /// Gets tenant metadata synchronously for context factories that cannot await.
    /// </summary>
    public Tenant? GetById(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
        {
            return null;
        }

        var cachedTenant = GetCachedById(tenantId);
        if (cachedTenant is not null)
        {
            logger.LogDebug("Tenant {TenantId} loaded from memory cache", tenantId);
            return cachedTenant;
        }

        var tenant = dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefault(item => item.Id == tenantId);

        if (tenant is not null)
        {
            SetCache(tenant);
            logger.LogInformation("Tenant {TenantId} loaded from database and cached", tenantId);
        }

        return tenant;
    }

    /// <summary>
    /// Gets tenant metadata by id, using the shared tenant cache first.
    /// </summary>
    public async Task<Tenant?> GetByIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default
    )
    {
        if (tenantId == Guid.Empty)
        {
            return null;
        }

        var cachedTenant = GetCachedById(tenantId);
        if (cachedTenant is not null)
        {
            logger.LogDebug("Tenant {TenantId} loaded from memory cache", tenantId);
            return cachedTenant;
        }

        var tenant = await dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == tenantId, cancellationToken);

        if (tenant is not null)
        {
            SetCache(tenant);
            logger.LogInformation("Tenant {TenantId} loaded from database and cached", tenantId);
        }

        return tenant;
    }

    private Tenant? GetCachedById(Guid tenantId)
    {
        var cachedTenant = cache.GetMemory<Tenant>(GetCacheKey(tenantId));
        if (cachedTenant is null)
        {
            return null;
        }

        if (!cachedTenant.IsDeleted)
        {
            return cachedTenant;
        }

        RemoveCache(tenantId, cachedTenant.Domain);
        return null;
    }

    /// <summary>
    /// Gets tenant metadata by domain, using the shared tenant cache first.
    /// </summary>
    public async Task<Tenant?> GetByDomainAsync(
        string? domain,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(domain))
        {
            return null;
        }

        domain = domain.Trim();
        var cacheKey = GetDomainCacheKey(domain);
        var cachedTenant = cache.GetMemory<Tenant>(cacheKey);
        if (cachedTenant is not null)
        {
            if (!cachedTenant.IsDeleted)
            {
                logger.LogDebug(
                    "Tenant {TenantId} loaded from memory cache for domain {Domain}",
                    cachedTenant.Id,
                    domain
                );
                return cachedTenant;
            }

            RemoveCache(cachedTenant.Id, domain);
            return null;
        }

        var tenant = await dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Domain == domain, cancellationToken);

        if (tenant is not null)
        {
            SetCache(tenant);
            logger.LogInformation(
                "Tenant {TenantId} loaded from database and cached for domain {Domain}",
                tenant.Id,
                domain
            );
        }

        return tenant;
    }

    /// <summary>
    /// Stores tenant metadata in both id and domain cache entries.
    /// Deleted tenants are removed; disabled tenants remain available for
    /// connection selection and background processing.
    /// </summary>
    /// <remarks>Call this only after the tenant database transaction has committed.</remarks>
    public void SetCache(Tenant tenant)
    {
        ArgumentNullException.ThrowIfNull(tenant);

        if (tenant.Id == Guid.Empty)
        {
            throw new ArgumentException("Tenant id cannot be empty.", nameof(tenant));
        }

        if (tenant.IsDeleted)
        {
            RemoveCache(tenant.Id, tenant.Domain);
            return;
        }

        var previousTenant = cache.GetMemory<Tenant>(GetCacheKey(tenant.Id));
        if (previousTenant is not null
            && !string.Equals(previousTenant.Domain, tenant.Domain, StringComparison.Ordinal))
        {
            cache.RemoveMemory(GetDomainCacheKey(previousTenant.Domain));
        }

        cache.SetMemory(GetCacheKey(tenant.Id), tenant, CacheDuration);
        if (!string.IsNullOrWhiteSpace(tenant.Domain))
        {
            cache.SetMemory(GetDomainCacheKey(tenant.Domain), tenant, CacheDuration);
        }
    }

    /// <summary>
    /// Removes the tenant id and any related domain cache entries.
    /// </summary>
    public void RemoveCache(Guid tenantId, string? domain = null)
    {
        if (tenantId == Guid.Empty)
        {
            return;
        }

        var cachedTenant = cache.GetMemory<Tenant>(GetCacheKey(tenantId));
        cache.RemoveMemory(GetCacheKey(tenantId));

        var domains = new[] { domain, cachedTenant?.Domain }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .Distinct(StringComparer.Ordinal);

        foreach (var cachedDomain in domains)
        {
            cache.RemoveMemory(GetDomainCacheKey(cachedDomain));
        }
    }

    /// <summary>
    /// Reloads tenant metadata from the catalog and replaces its cache entries.
    /// Call this after a tenant metadata change has been committed.
    /// </summary>
    public async Task<Tenant?> RefreshCacheAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default
    )
    {
        if (tenantId == Guid.Empty)
        {
            return null;
        }

        var previousTenant = cache.GetMemory<Tenant>(GetCacheKey(tenantId));
        var tenant = await dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == tenantId, cancellationToken);

        RemoveCache(tenantId, previousTenant?.Domain);
        if (tenant is not null)
        {
            SetCache(tenant);
        }

        return tenant;
    }

    /// <summary>
    /// Returns the cache key used by AppDbFactory for synchronous connection selection.
    /// </summary>
    public static string GetCacheKey(Guid tenantId)
    {
        return $"{WebConst.TenantCachePrefix}{tenantId}";
    }

    private static string GetDomainCacheKey(string domain)
    {
        return $"{WebConst.TenantCachePrefix}Domain__{domain}";
    }
}
