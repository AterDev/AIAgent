using Share.Models.Auth;
using SystemMod.Models.SystemUserDtos;

namespace AdminService.Controllers.SystemMod;

/// <summary>
/// 系统用户
/// </summary>
public class SystemUserController(
    Localizer localizer,
    IUserContext user,
    ILogger<SystemUserController> logger,
    SystemUserManager manager
    ) : RestControllerBase<SystemUserManager>(localizer, manager, user, logger)
{
    /// <summary>
    /// list 系统用户 with page ✍️
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<SystemUserItemDto>>> ListAsync(SystemUserFilterDto filter)
    {
        return await _manager.FilterAsync(filter);
    }

    /// <summary>
    /// Add 系统用户 ✍️
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<SystemUser>> AddAsync(SystemUserAddDto dto)
    {

        var entity = await _manager.AddAsync(dto);
        return CreatedAtRoute(null, new { id = entity.Id }, entity);
    }

    /// <summary>
    /// Update 系统用户 ✍️
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPatch("{id}")]
    public async Task<bool> UpdateAsync([FromRoute] Guid id, SystemUserUpdateDto dto)
    {
        return await _manager.EditAsync(id, dto) == 1;
    }

    /// <summary>
    /// Get 系统用户 Detail ✍️
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<SystemUserDetailDto?> DetailAsync([FromRoute] Guid id)
    {
        return await _manager.GetAsync(id);
    }

    /// <summary>
    /// Delete 系统用户 ✍️
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteAsync([FromRoute] Guid id)
    {
        return await _manager.DeleteAsync([id], false);
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    /// <param name="dto">登录信息</param>
    /// <returns>访问令牌</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<AccessTokenDto> LoginAsync([FromBody] LoginDto dto)
    {
        var result = await _manager.LoginAsync(dto);
        return result;
    }

    /// <summary>
    /// 修改密码
    /// </summary>
    /// <param name="dto">密码信息</param>
    /// <returns></returns>
    [HttpPost("change-password")]
    public async Task<ActionResult<bool>> ChangePasswordAsync([FromBody] ChangePasswordDto dto)
    {
        var result = await _manager.ChangePasswordAsync(_user.UserId, dto);
        return Ok(result);
    }

    /// <summary>
    /// 获取当前用户信息
    /// </summary>
    /// <returns>用户信息</returns>
    [HttpGet("current")]
    public async Task<ActionResult<UserInfoDto>> GetCurrentUserInfoAsync()
    {
        var result = await _manager.GetCurrentUserInfoAsync(_user.UserId);
        if (result == null)
        {
            return NotFound();
        }
        return Ok(result);
    }
}