namespace Entity.ModelMod;

/// <summary>
/// 应用模型权限
/// </summary>
[Index(nameof(ApplicationId), nameof(AIModelInfoId), IsUnique = true)]
public class ApplicationModelPermission : EntityBase
{
    public Guid ApplicationId { get; set; }

    [ForeignKey(nameof(ApplicationId))]
    public Application? Application { get; set; }

    public Guid AIModelInfoId { get; set; }

    [ForeignKey(nameof(AIModelInfoId))]
    public AIModelInfo? AIModelInfo { get; set; }

    public bool IsEnabled { get; set; } = true;
}
