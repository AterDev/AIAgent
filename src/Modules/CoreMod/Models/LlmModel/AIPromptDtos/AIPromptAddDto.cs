namespace CoreMod.Models.AIPromptDtos;
/// <summary>
/// 提示词AddDto
/// </summary>
/// <see cref="AIPrompt"/>
public class AIPromptAddDto
{
    /// <summary>
    /// 提示词名称
    /// </summary>
    [MaxLength(100)]
    public string Name { get; set; } = default!;
    /// <summary>
    /// 提示词描述
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }
    /// <summary>
    /// 提示词内容
    /// </summary>
    [MaxLength(5000)]
    public string Content { get; set; } = default!;
    /// <summary>
    /// 提示词分组
    /// </summary>
    [MaxLength(100)]
    public string GroupName { get; set; } = default!;
    
}
