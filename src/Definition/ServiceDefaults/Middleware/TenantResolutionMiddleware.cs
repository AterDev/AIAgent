using Microsoft.Extensions.Logging;
using Perigon.AspNetCore.Abstraction;
using Share.Services;

namespace ServiceDefaults.Middleware;

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
        TenantService tenantService
    )
    {
        try
        {
            if (userContext.TenantId == Guid.Empty)
            {
                if (context.User.Identity?.IsAuthenticated != true)
                {
                    await _next(context);
                    return;
                }

                _logger.LogWarning("Authenticated user has no TenantId claim");
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }

            var tenant = await tenantService.GetByIdAsync(
                userContext.TenantId,
                context.RequestAborted
            );

            if (tenant is not null && (!tenant.Disabled && !tenant.IsDeleted))
            {
                userContext.TenantType = tenant.Type.ToString();
            }
            else
            {
                _logger.LogWarning(
                    "Tenant {TenantId} not found; rejecting the authenticated request",
                    userContext.TenantId
                );
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving tenant");
            throw;
        }

        await _next(context);
    }
}
