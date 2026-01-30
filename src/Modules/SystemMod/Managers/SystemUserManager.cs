using Perigon.AspNetCore.Constants;
using Perigon.AspNetCore.Services;
using Share.Models.Auth;
using SystemMod.Models.SystemUserDtos;

namespace SystemMod.Managers;
/// <summary>
/// 系统用户
/// </summary>
public class SystemUserManager(
    TenantDbFactory dbContextFactory,
    ILogger<SystemUserManager> logger,
    IUserContext userContext,
    JwtService jwtService
) : ManagerBase<DefaultDbContext, SystemUser>(dbContextFactory, userContext, logger)
{
    /// <summary>
    /// Filter 系统用户 with paging
    /// </summary>
    public async Task<PageList<SystemUserItemDto>> FilterAsync(SystemUserFilterDto filter)
    {
        Queryable = Queryable
            .WhereNotNull(filter.UserName, q => q.UserName == filter.UserName)
            .WhereNotNull(filter.Email, q => q.Email == filter.Email)
            .WhereNotNull(filter.Enabled, q => q.Enabled == filter.Enabled);

        return await PageListAsync<SystemUserFilterDto, SystemUserItemDto>(filter);
    }

    /// <summary>
    /// Add 系统用户
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<SystemUser> AddAsync(SystemUserAddDto dto)
    {
        // 检查用户名是否已存在
        if (await _dbSet.AnyAsync(u => u.UserName == dto.UserName))
        {
            throw new BusinessException(Localizer.UserNameAlreadyExists, arguments: [dto.UserName]);
        }

        // 检查邮箱是否已存在
        if (await _dbSet.AnyAsync(u => u.Email == dto.Email))
        {
            throw new BusinessException(Localizer.EmailAlreadyExists, arguments: [dto.Email]);
        }

        var entity = dto.MapTo<SystemUser>();

        // 处理密码
        entity.PasswordSalt = HashCrypto.BuildSalt();
        entity.PasswordHash = HashCrypto.GeneratePwd(dto.Password, entity.PasswordSalt);

        await InsertAsync(entity);
        return entity;
    }

    /// <summary>
    /// edit 系统用户
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<int> EditAsync(Guid id, SystemUserUpdateDto dto)
    {
        if (await HasPermissionAsync(id))
        {
            return await UpdateAsync(id, dto);
        }
        throw new BusinessException(Localizer.NoPermission);
    }


    /// <summary>
    /// Get 系统用户 detail
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<SystemUserDetailDto?> GetAsync(Guid id)
    {
        if (await HasPermissionAsync(id))
        {
            return await FindAsync<SystemUserDetailDto>(q => q.Id == id);
        }
        throw new BusinessException(Localizer.NoPermission);
    }

    /// <summary>
    /// Delete  系统用户
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="softDelete"></param>
    /// <returns></returns>
    public async Task<bool?> DeleteAsync(List<Guid> ids, bool softDelete = true)
    {
        if (!ids.Any())
        {
            return false;
        }
        if (ids.Count() == 1)
        {
            Guid id = ids.First();
            if (await HasPermissionAsync(id))
            {
                return await DeleteOrUpdateAsync(ids, !softDelete) > 0;
            }
            throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
        }
        else
        {
            var ownedIds = await GetOwnedIdsAsync(ids);
            if (ownedIds.Any())
            {
                return await DeleteOrUpdateAsync(ownedIds, !softDelete) > 0;
            }
            throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
        }
    }

    public override async Task<bool> HasPermissionAsync(Guid id)
    {
        var query = _dbSet
            .Where(q => q.Id == id);
        return await query.AnyAsync();
    }

    public async Task<List<Guid>> GetOwnedIdsAsync(IEnumerable<Guid> ids)
    {
        if (!ids.Any())
        {
            return [];
        }
        var query = _dbSet
            .Where(q => ids.Contains(q.Id))
            .Select(q => q.Id);
        return await query.ToListAsync();
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    /// <param name="dto">登录信息</param>
    /// <returns>访问令牌</returns>
    public async Task<AccessTokenDto> LoginAsync(LoginDto dto)
    {
        var user = await _dbSet
            .Where(u => u.UserName == dto.UserName)
            .FirstOrDefaultAsync()
            ?? throw new BusinessException(Localizer.InvalidUserOrPassword);

        // 验证密码
        if (!HashCrypto.Validate(dto.Password, user.PasswordSalt, user.PasswordHash))
        {
            throw new BusinessException(Localizer.InvalidUserOrPassword);
        }

        // 检查用户是否启用
        if (!user.Enabled)
        {
            throw new BusinessException(Localizer.UserDisabled);
        }

        // 更新最后登录时间
        user.LastLoginTime = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync();

        // 生成JWT Token
        var roles = string.IsNullOrEmpty(user.Roles)
            ? [WebConst.SuperAdmin]
            : user.Roles.Split(',', StringSplitOptions.RemoveEmptyEntries);

        var token = jwtService.GetToken(user.Id.ToString(), roles);
        var refreshToken = JwtService.GetRefreshToken();

        return new AccessTokenDto
        {
            AccessToken = token,
            RefreshToken = refreshToken,
            ExpiresIn = jwtService.ExpiredSecond,
            RefreshExpiresIn = jwtService.RefreshExpiredSecond
        };
    }

    /// <summary>
    /// 修改密码
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="dto">密码信息</param>
    /// <returns></returns>
    public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
    {
        var user = await _dbSet.Where(u => u.Id == userId).FirstOrDefaultAsync()
            ?? throw new BusinessException(Localizer.UserNotFound);

        // 验证旧密码
        if (!HashCrypto.Validate(dto.OldPassword, user.PasswordSalt, user.PasswordHash))
        {
            throw new BusinessException(Localizer.PasswordInvalid);
        }

        // 更新密码
        user.PasswordSalt = HashCrypto.BuildSalt();
        user.PasswordHash = HashCrypto.GeneratePwd(dto.NewPassword, user.PasswordSalt);
        user.UpdatedTime = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// 获取当前用户信息
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns></returns>
    public async Task<UserInfoDto?> GetCurrentUserInfoAsync(Guid userId)
    {
        var user = await _dbSet
            .Where(u => u.Id == userId)
            .Select(u => new UserInfoDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                RealName = u.RealName,
                Avatar = u.Avatar,
                Roles = u.Roles
            })
            .FirstOrDefaultAsync();

        return user;
    }

    /// <summary>
    /// 检查用户名是否存在
    /// </summary>
    /// <param name="userName">用户名</param>
    /// <returns></returns>
    public async Task<bool> ExistsUserNameAsync(string userName)
    {
        return await _dbSet.AnyAsync(u => u.UserName == userName);
    }
}