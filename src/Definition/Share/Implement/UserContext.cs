using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Share.Implement;

public class UserContext : IUserContext
{
    public Guid UserId { get; init; }

    public Guid? GroupId { get; init; }

    public Guid TenantId { get; set; }
    public string? TenantType { get; set; }

    public string? UserName { get; init; }
    public string? Email { get; set; }

    public bool IsAdmin { get; init; }
    public string? CurrentRole { get; set; }
    public List<string>? Roles { get; set; }
    public IReadOnlyList<Guid> RoleIds { get; private set; } = [];
    IReadOnlyList<string>? IUserContext.Roles => Roles;

    public HttpContext? HttpContext { get; set; }

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        HttpContext = httpContextAccessor!.HttpContext;
        if (Guid.TryParse(FindClaimValue(ClaimTypes.NameIdentifier, JwtRegisteredClaimNames.Sub), out Guid userId)
            && userId != Guid.Empty
        )
        {
            UserId = userId;
        }
        if (Guid.TryParse(FindClaimValue(ClaimTypes.GroupSid), out Guid groupSid)
            && groupSid != Guid.Empty
        )
        {
            GroupId = groupSid;
        }

        if (Guid.TryParse(FindClaimValue(CustomClaimTypes.TenantId), out Guid tenantId)
            && tenantId != Guid.Empty
        )
        {
            TenantId = tenantId;
            TenantType = FindClaimValue(CustomClaimTypes.TenantType)
                ?? nameof(Entity.TenantType.Normal);
        }

        UserName = FindClaimValue(ClaimTypes.Name, JwtRegisteredClaimNames.Name);
        Email = FindClaimValue(ClaimTypes.Email, JwtRegisteredClaimNames.Email);

        CurrentRole = FindClaimValue(ClaimTypes.Role);

        Roles = HttpContext?.User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        RoleIds = HttpContext?.User?.FindAll(CustomClaimTypes.RoleId)
            .Select(claim => Guid.TryParse(claim.Value, out Guid roleId) ? roleId : Guid.Empty)
            .Where(roleId => roleId != Guid.Empty)
            .Distinct()
            .ToList() ?? [];
        if (Roles != null)
        {
            IsAdmin = Roles.Any(r => r.Equals(WebConst.AdminUser) || r.Equals(WebConst.SuperAdmin));
        }
    }

    protected string? FindClaimValue(params string[] claimTypes)
    {
        foreach (var claimType in claimTypes)
        {
            var value = HttpContext?.User?.FindFirstValue(claimType);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }
        return null;
    }

    /// <summary>
    /// 判断当前角色
    /// </summary>
    /// <param name="roleName"></param>
    /// <returns></returns>
    public bool IsRole(string roleName)
    {
        return Roles != null && Roles.Any(r => r == roleName);
    }
}
