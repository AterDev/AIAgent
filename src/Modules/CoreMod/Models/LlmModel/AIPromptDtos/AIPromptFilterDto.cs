namespace CoreMod.Models.AIPromptDtos;
/// <summary>
/// 提示词FilterDto
/// </summary>
/// <see cref="AIPrompt"/>
public class AIPromptFilterDto : FilterBase
{
    /// <summary>
    /// 提示词名称
    /// </summary>
    [MaxLength(100)]
    public string? Name { get; set; }
    /// <summary>
    /// 提示词分组
    /// </summary>
    [MaxLength(100)]
    public string? GroupName { get; set; }
    
}
