using System.Text.Json.Serialization;

namespace Entity.SystemMod;

/// <summary>
/// 系统日志
/// </summary>
[Index(nameof(ActionType), nameof(CreatedTime))]
[Index(nameof(ActionUserName), nameof(CreatedTime))]
[Index(nameof(CreatedTime))]
public class SystemLogs : EntityBase
{
    /// <summary>
    /// 操作人名称
    /// </summary>
    [MaxLength(100)]
    public required string ActionUserName { get; set; }

    /// <summary>
    /// 操作对象名称
    /// </summary>
    [MaxLength(100)]
    public string? TargetName { get; set; }

    [NotMapped]
    [JsonIgnore]
    public object? Data { get; set; }

    /// <summary>
    /// 操作路由
    /// </summary>
    [MaxLength(200)]
    public required string Route { get; set; } = string.Empty;

    /// <summary>
    /// 操作类型
    /// </summary>
    public required UserActionType ActionType { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    [ForeignKey(nameof(SystemUserId))]
    public SystemUser SystemUser { get; set; } = null!;

    public Guid SystemUserId { get; set; } = default!;

    public static SystemLogs NewLog(
        Guid tenantId,
        string userName,
        Guid userId,
        object entity,
        UserActionType actionType,
        string? route = null,
        string? description = null
    )
    {
        return new SystemLogs
        {
            TenantId = tenantId,
            SystemUserId = userId,
            ActionUserName = userName,
            Data = entity,
            Route = route ?? string.Empty,
            ActionType = actionType,
            Description = description,
        };
    }
}
