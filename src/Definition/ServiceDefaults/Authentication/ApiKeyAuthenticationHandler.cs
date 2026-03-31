using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Share.Implement;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace ServiceDefaults.Authentication;

/// <summary>
/// Bearer ApiKey 认证处理器
/// </summary>
public class ApiKeyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    ApiKeyService apiKeyAuthService
) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authorization = Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authorization)
            || !authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.NoResult();
        }

        var token = authorization["Bearer ".Length..].Trim();
        if (!ApiKeyService.IsWellFormedApiKey(token))
        {
            return AuthenticateResult.NoResult();
        }

        var application = await apiKeyAuthService.AuthenticateAsync(token, Context.RequestAborted);
        if (application is null)
        {
            return AuthenticateResult.Fail("Invalid application API key.");
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, application.ApplicationId.ToString()),
            new(ClaimTypes.Name, application.Name),
            new(ClaimTypes.Role, WebConst.Application),
            new(CustomClaimTypes.ApplicationId, application.ApplicationId.ToString()),
            new(CustomClaimTypes.TenantId, application.TenantId.ToString()),
            new(CustomClaimTypes.TenantType, application.TenantType),
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return AuthenticateResult.Success(ticket);
    }
}