using Entity.ModelMod;
namespace ModelMod.Models.ApplicationToolPermissionDtos;

/// <summary>
/// 应用工具权限 ItemDto
/// </summary>
/// <see cref="Entity.ModelMod.ApplicationToolPermission"/>
public class ApplicationToolPermissionItemDto
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }

    [MaxLength(100)]
    public string? ToolName { get; set; }

    public bool IsEnabled { get; set; }
}
