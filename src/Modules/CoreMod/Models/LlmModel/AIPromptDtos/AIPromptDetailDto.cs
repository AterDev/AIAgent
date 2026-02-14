namespace CoreMod.Models.AIPromptDtos;
/// <summary>
/// 提示词DetailDto
/// </summary>
/// <see cref="AIPrompt"/>
public class AIPromptDetailDto
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
    [Key]
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public DateTimeOffset CreatedTime { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedTime { get; set; } = DateTimeOffset.UtcNow;
    public Guid TenantId { get; set; }
    
}
