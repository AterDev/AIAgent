using ModelMod.Models.ApplicationDtos;
using Perigon.AspNetCore.Constants;
using Share.Exceptions;

namespace ApiService.Controllers.OpenPlatform;

/// <summary>
/// Open platform apps
/// </summary>
[ApiController]
[Route("api/v1/apps")]
public class AppsController(
    ApplicationManager manager,
    IUserContext user,
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
                PageIndex = filter.PageIndex,
            });
        }

        return await _manager.FilterAsync(filter);
    }

    [HttpGet("{id}")]
    public async Task<ApplicationDetailDto?> DetailAsync([FromRoute] Guid id)
    {
        if (_user.IsRole(WebConst.Application) && _user.UserId != id)
        {
            throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
        }

        return await _manager.GetAsync(id);
    }
}
