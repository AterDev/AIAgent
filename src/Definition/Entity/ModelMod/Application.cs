namespace Entity.ModelMod;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// 应用定义
/// </summary>
[Index(nameof(Name), IsUnique = true)]
public class Application : EntityBase
{
    [MaxLength(100)]
    public required string Name { get; set; }

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;
}
