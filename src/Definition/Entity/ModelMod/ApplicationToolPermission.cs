namespace Entity.ModelMod;

/// <summary>
/// 应用 MCP 工具权限
/// </summary>
[Index(nameof(ApplicationId), nameof(ToolName), IsUnique = true)]
public class ApplicationToolPermission : EntityBase
{
    public Guid ApplicationId { get; set; }

    [ForeignKey(nameof(ApplicationId))]
    public Application? Application { get; set; }

    [MaxLength(100)]
    public required string ToolName { get; set; }

    public bool IsEnabled { get; set; } = true;
}
