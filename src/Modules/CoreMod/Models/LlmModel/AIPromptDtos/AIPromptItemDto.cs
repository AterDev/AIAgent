namespace CoreMod.Models.AIPromptDtos;
/// <summary>
/// 提示词ItemDto
/// </summary>
/// <see cref="AIPrompt"/>
public class AIPromptItemDto
{
    /// <summary>
    /// 提示词名称
    /// </summary>
    [MaxLength(100)]
    public string Name { get; set; } = default!;
    /// <summary>
    /// 提示词分组
    /// </summary>
    [MaxLength(100)]
    public string GroupName { get; set; } = default!;
    [Key]
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public DateTimeOffset CreatedTime { get; set; } = DateTimeOffset.UtcNow;
    
}
