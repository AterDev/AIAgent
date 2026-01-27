namespace Entity.ModelMod;

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

    [MaxLength(100)]
    public required string AccessKey { get; set; }

    [MaxLength(200)]
    public required string SecretKey { get; set; }

    public bool IsEnabled { get; set; } = true;
}
