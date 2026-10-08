using Microsoft.AspNetCore.Http;

namespace Perigon.AspNetCore.Abstraction;

public interface IUserContext
{
    /// <summary>
    /// 用户ID
    /// </summary>
    Guid UserId { get; }

    /// <summary>
    /// 组织ID
    /// </summary>
    Guid? GroupId { get; }

    Guid TenantId { get; set; }

    string? TenantType { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// 邮箱
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// 是否为管理员
    /// </summary>
    bool IsAdmin { get; }

    /// <summary>
    /// 当前角色
    /// </summary>
    string? CurrentRole { get; }

    /// <summary>
    /// 所有角色
    /// </summary>
    IReadOnlyList<string>? Roles { get; }

    /// <summary>
    /// 当前用户拥有的角色 ID；角色名称与角色 ID 分开传递，供模块按不透明 ID 执行授权。
    /// </summary>
    IReadOnlyList<Guid> RoleIds { get; }

    public HttpContext? HttpContext { get; set; }

    /// <summary>
    /// 判断当前用户是否属于指定角色
    /// </summary>
    bool IsRole(string roleName);
}
