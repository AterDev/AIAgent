using ModelMod.Models.ApplicationDtos;
using Perigon.AspNetCore.Constants;
using Perigon.AspNetCore.Services;
using System.Security.Claims;

namespace ApiService.Controllers.OpenPlatform;

/// <summary>
/// Open platform apps
/// </summary>
[ApiController]
[Route("api/v1/apps")]
public class AppsController(
    ApplicationManager manager,
    IUserContext user,
    JwtService jwtService,
    ILogger<AppsController> logger
) : OpenApiControllerBase<ApplicationManager>(manager, user, logger)
{
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<ApplicationItemDto>>> ListAsync(ApplicationFilterDto filter)
    {
        if (_user.IsRole(WebConst.Application))
        {
            var currentApp = await _manager.GetItemAsync(_user.UserId);
            if (currentApp is null)
            {
                return Unauthorized();
            }

            return Ok(new PageList<ApplicationItemDto>
            {
                Count = 1,
                Data = [currentApp],
                PageIndex = filter.PageIndex ?? 1,
            });
        }

        return await _manager.FilterAsync(filter);
    }

    [HttpGet("{id}")]
    public async Task<ApplicationDetailDto?> DetailAsync([FromRoute] Guid id)
    {
        if (_user.IsRole(WebConst.Application) && _user.UserId != id)
        {
            throw new BusinessException("No permission", StatusCodes.Status403Forbidden);
        }

        return await _manager.GetAsync(id);
    }

    /// <summary>
    /// 使用应用凭证换取访问令牌
    /// </summary>
    [AllowAnonymous]
    [HttpPost("token")]
    public async Task<ActionResult<ApplicationTokenResponseDto>> TokenAsync(ApplicationTokenRequestDto dto)
    {
        var application = await _manager.AuthenticateAsync(dto.ClientId, dto.ClientSecret);
        if (application is null)
        {
            return Unauthorized();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, application.Id.ToString()),
            new(ClaimTypes.Name, application.Name),
            new(ClaimTypes.Role, WebConst.User),
            new(ClaimTypes.Role, WebConst.Application),
            new(CustomClaimTypes.ApplicationId, application.Id.ToString()),
            new(CustomClaimTypes.TenantId, application.TenantId.ToString()),
            new(CustomClaimTypes.TenantType, nameof(Entity.TenantType.Normal)),
        };

        var accessToken = jwtService.GetToken(claims, jwtService.ExpiredSecond);
        return Ok(new ApplicationTokenResponseDto
        {
            ApplicationId = application.Id,
            Name = application.Name,
            ClientId = application.ClientId,
            AccessToken = accessToken,
            ExpiresIn = jwtService.ExpiredSecond,
        });
    }
}
