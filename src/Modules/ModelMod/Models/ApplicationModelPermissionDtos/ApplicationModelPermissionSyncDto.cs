namespace ModelMod.Models.ApplicationModelPermissionDtos;

/// <summary>
/// 批量同步应用模型权限
/// </summary>
public class ApplicationModelPermissionSyncDto
{
    public Guid ApplicationId { get; set; }

    public List<Guid> AIModelInfoIds { get; set; } = [];
}