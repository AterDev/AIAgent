namespace ModelMod.Models.ApplicationToolPermissionDtos;

/// <summary>
/// 应用工具权限 AddDto
/// </summary>
/// <see cref="Entity.ModelMod.ApplicationToolPermission"/>
public class ApplicationToolPermissionAddDto
{
    public Guid ApplicationId { get; set; }

    [MaxLength(100)]
    public string ToolName { get; set; } = default!;

    public bool IsEnabled { get; set; } = true;
}
