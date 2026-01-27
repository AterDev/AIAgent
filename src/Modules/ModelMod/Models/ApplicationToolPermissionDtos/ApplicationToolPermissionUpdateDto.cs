using Entity.ModelMod;
namespace ModelMod.Models.ApplicationToolPermissionDtos;

/// <summary>
/// 应用工具权限 UpdateDto
/// </summary>
/// <see cref="Entity.ModelMod.ApplicationToolPermission"/>
public class ApplicationToolPermissionUpdateDto
{
    [MaxLength(100)]
    public string? ToolName { get; set; }

    public bool? IsEnabled { get; set; }
}
