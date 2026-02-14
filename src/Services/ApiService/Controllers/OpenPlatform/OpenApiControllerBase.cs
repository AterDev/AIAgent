namespace ApiService.Controllers.OpenPlatform;

[ApiExplorerSettings(GroupName = "v1")]
[Authorize(Policy = WebConst.User)]
public abstract class OpenApiControllerBase<TManager>(
    TManager manager,
    IUserContext user,
    ILogger logger
) : ControllerBase
    where TManager : class
{
    protected readonly TManager _manager = manager;
    protected readonly IUserContext _user = user;
    protected readonly ILogger _logger = logger;
}
