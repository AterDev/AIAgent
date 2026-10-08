using System.Security.Claims;
using Entity;
using Microsoft.AspNetCore.Authentication;
using Perigon.AspNetCore.Services;
using Share.Services;

namespace ServiceDefaults;

public class UserClaimsTransformation(TenantService tenantService, CacheService cache)
    : IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
        {
            return principal;
        }

        var tenant = await ResolveTenantAsync(identity);
        if (tenant is null)
        {
            // TenantId is mandatory for authenticated requests. Leave the
            // principal untouched here so the tenant middleware can reject it
            // with a 403 instead of silently selecting the default tenant.
            return principal;
        }

        ReplaceTenantClaims(identity, tenant);

        var userIdentity = FindUserIdentity(principal);
        if (!string.IsNullOrWhiteSpace(userIdentity)
            && !identity.HasClaim(claim => claim.Type == ClaimTypes.Role))
        {
            var cacheKey = $"local-user-info:{tenant.Id}:{userIdentity}";

            // the sample of get user roles from local system, you can replace this with your own implementation, such as query from database or call external service.
            var roles = await cache.GetOrCreateAsync(
                cacheKey,
                cancellation => new ValueTask<string[]>(
                    QueryRolesFromLocalSystemAsync(userIdentity, cancellation)
                )
            );

            identity.AddClaims(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        }

        return principal;
    }

    private async Task<Tenant?> ResolveTenantAsync(ClaimsIdentity identity)
    {
        var tenantIdValue = identity.FindFirst(CustomClaimTypes.TenantId)?.Value;
        return Guid.TryParse(tenantIdValue, out var tenantId) && tenantId != Guid.Empty
            ? await tenantService.GetByIdAsync(tenantId)
            : null;
    }

    private static void ReplaceTenantClaims(ClaimsIdentity identity, Tenant tenant)
    {
        foreach (var claimType in new[]
        {
            CustomClaimTypes.TenantId,
            CustomClaimTypes.TenantType,
            CustomClaimTypes.TenantName,
        })
        {
            foreach (var claim in identity.FindAll(claimType).ToArray())
            {
                identity.RemoveClaim(claim);
            }
        }

        identity.AddClaim(new Claim(CustomClaimTypes.TenantId, tenant.Id.ToString()));
        identity.AddClaim(new Claim(CustomClaimTypes.TenantType, tenant.Type.ToString()));
        identity.AddClaim(new Claim(CustomClaimTypes.TenantName, tenant.Name));
    }

    /// <summary>
    /// get user identity from claims, use 
    /// </summary>
    private static string? FindUserIdentity(ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.Email)
            ?? principal.FindFirstValue(ClaimTypes.Name)
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    private static Task<string[]> QueryRolesFromLocalSystemAsync(
        string userIdentity,
        CancellationToken cancellation
    )
    {
        _ = cancellation;

        return Task.FromResult(
            userIdentity.Equals("admin", StringComparison.OrdinalIgnoreCase)
                ? new[] { WebConst.User, WebConst.AdminUser }
                : new[] { WebConst.User }
        );
    }

}
