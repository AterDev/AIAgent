using Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Perigon.AspNetCore.Abstraction;
using Perigon.AspNetCore.Services;

namespace Services.Middleware;

/// <summary>
/// Middleware to resolve tenant metadata and cache it in memory.
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    public TenantResolutionMiddleware(
        RequestDelegate next,
        ILogger<TenantResolutionMiddleware> logger
    )
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IUserContext userContext,
        DefaultDbContext dbContext,
        CacheService cache
    )
    {
        try
        {
            if (userContext.TenantId == Guid.Empty)
            {
                _logger.LogDebug("Skip tenant resolve because TenantId is empty");
                await _next(context);
                return;
            }

            var cacheKey = $"{WebConst.TenantId}__{userContext.TenantId}";
            var tenant = cache.GetMemory<Tenant>(cacheKey);

            if (tenant is null)
            {
                tenant = await dbContext.Tenants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.TenantId == userContext.TenantId);

                if (tenant is not null)
                {
                    cache.SetMemory(cacheKey, tenant, TimeSpan.FromDays(1));
                    _logger.LogInformation(
                        "Tenant {TenantId} loaded from database and cached",
                        userContext.TenantId
                    );
                }
            }
            else
            {
                _logger.LogDebug("Tenant {TenantId} loaded from memory cache", userContext.TenantId);
            }

            if (tenant is not null)
            {
                userContext.TenantType = tenant.Type.ToString();
            }
            else
            {
                _logger.LogWarning(
                    "Tenant {TenantId} not found; fallback to default connection strings",
                    userContext.TenantId
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving tenant connection strings");
            throw;
        }

        await _next(context);
    }
}