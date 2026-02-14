namespace ModelMod.Models.ApplicationToolPermissionDtos;

/// <summary>
/// 应用工具权限 FilterDto
/// </summary>
/// <see cref="ApplicationToolPermission"/>
public class ApplicationToolPermissionFilterDto : FilterBase
{
    public Guid? ApplicationId { get; set; }

    [MaxLength(100)]
    public string? ToolName { get; set; }

    public bool? IsEnabled { get; set; }
}
