namespace Entity.ModelMod;

/// <summary>
/// 应用模型权限
/// </summary>
[Index(nameof(ApplicationId), nameof(ModelProfileId), IsUnique = true)]
public class ApplicationModelPermission : EntityBase
{
    public Guid ApplicationId { get; set; }

    [ForeignKey(nameof(ApplicationId))]
    public Application? Application { get; set; }

    public Guid ModelProfileId { get; set; }

    [ForeignKey(nameof(ModelProfileId))]
    public ModelProfile? ModelProfile { get; set; }

    public bool IsEnabled { get; set; } = true;
}
