using Entity.AIAgentMod;
namespace AIAgentMod.Models.AIAgentDtos;
/// <summary>
/// agentItemDto
/// </summary>
/// <see cref="Entity.AIAgentMod.AIAgent"/>
public class AIAgentItemDto
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
    /// <summary>
    /// is enabled
    /// </summary>
    public bool? Enable { get; set; }
    public bool? IsTemplate { get; set; }
    /// <summary>
    /// Agent 名称
    /// </summary>
    [MaxLength(100)]
    public string? Name { get; set; }
    /// <summary>
    /// Agent 的角色设定（System Prompt）
    /// </summary>
    public string? SystemPrompt { get; set; }
    
}
