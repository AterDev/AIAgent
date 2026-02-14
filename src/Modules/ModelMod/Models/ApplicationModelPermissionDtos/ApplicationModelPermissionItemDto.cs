namespace ModelMod.Models.ApplicationModelPermissionDtos;

/// <summary>
/// 应用模型权限 ItemDto
/// </summary>
/// <see cref="ApplicationModelPermission"/>
public class ApplicationModelPermissionItemDto
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public Guid AIModelInfoId { get; set; }
    public bool IsEnabled { get; set; }
}
