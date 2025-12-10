using Entity.AIAgentMod;
namespace AIAgentMod.Models.AIAgentDtos;
/// <summary>
/// agentDetailDto
/// </summary>
/// <see cref="Entity.AIAgentMod.AIAgent"/>
public class AIAgentDetailDto
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
    /// <summary>
    /// Agent 描述信息
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }
    /// <summary>
    /// is enabled
    /// </summary>
    public bool? Enable { get; set; }
    public bool? IsTemplate { get; set; }
    /// <summary>
    /// Agent 所使用的大模型名称（例如 "gpt-4", "qwen-max", "custom-llm"）
    /// </summary>
    public string? ModelId { get; set; }
    /// <summary>
    /// Agent 名称
    /// </summary>
    [MaxLength(100)]
    public string? Name { get; set; }
    /// <summary>
    /// Agent 的角色设定（System Prompt）
    /// </summary>
    public string? SystemPrompt { get; set; }
    public Guid TenantId { get; set; }
    /// <summary>
    /// Agent 可用的工具列表
    /// </summary>
    public List<string>? Tools { get; set; }
    public DateTimeOffset UpdatedTime { get; set; }
    public Guid? UserId { get; set; }
    
}
