using Entity.AIAgentMod;
namespace AIAgentMod.Models.AIAgentDtos;
/// <summary>
/// agentFilterDto
/// </summary>
/// <see cref="Entity.AIAgentMod.AIAgent"/>
public class AIAgentFilterDto : FilterBase
{
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
    public Guid? UserId { get; set; }
    
}
