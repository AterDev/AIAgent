namespace Entity.CoreMod;

/// <summary>
/// 提示词
/// </summary>
[Index(nameof(Name))]
[Index(nameof(GroupName))]
[Index(nameof(Name), nameof(GroupName), IsUnique = true)]
public class AIPrompt : EntityBase
{
    /// <summary>
    /// 提示词名称
    /// </summary>
    [MaxLength(100)]
    public required string Name { get; set; }

    /// <summary>
    /// 提示词描述
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// 提示词内容
    /// </summary>
    [MaxLength(5000)]
    public required string Content { get; set; }

    /// <summary>
    /// 提示词分组
    /// </summary>
    [MaxLength(100)]
    public required string GroupName { get; set; }
}
