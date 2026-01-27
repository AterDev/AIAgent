namespace ModelMod.Models.ApplicationModelPermissionDtos;

/// <summary>
/// 应用模型权限 DetailDto
/// </summary>
/// <see cref="Entity.ModelMod.ApplicationModelPermission"/>
public class ApplicationModelPermissionDetailDto
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public Guid ModelProfileId { get; set; }
    public bool IsEnabled { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
    public DateTimeOffset UpdatedTime { get; set; }
    public Guid TenantId { get; set; }
}
