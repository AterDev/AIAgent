namespace ModelMod.Models.ApplicationToolPermissionDtos;

/// <summary>
/// 应用工具权限 DetailDto
/// </summary>
/// <see cref="Entity.ModelMod.ApplicationToolPermission"/>
public class ApplicationToolPermissionDetailDto
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }

    [MaxLength(100)]
    public string? ToolName { get; set; }

    public bool IsEnabled { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
    public DateTimeOffset UpdatedTime { get; set; }
    public Guid TenantId { get; set; }
}
