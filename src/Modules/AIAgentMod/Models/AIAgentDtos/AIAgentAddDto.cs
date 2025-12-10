using Entity.AIAgentMod;
namespace AIAgentMod.Models.AIAgentDtos;
/// <summary>
/// agentAddDto
/// </summary>
/// <see cref="Entity.AIAgentMod.AIAgent"/>
public class AIAgentAddDto
{
    /// <summary>
    /// Agent 描述信息
    /// </summary>
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    /// <summary>
    /// is enabled
    /// </summary>
    public bool Enable { get; set; }
    public bool IsTemplate { get; set; }
    /// <summary>
    /// Agent 所使用的大模型名称（例如 "gpt-4", "qwen-max", "custom-llm"）
    /// </summary>
    public string ModelId { get; set; } = default!;
    /// <summary>
    /// Agent 名称
    /// </summary>
    [MaxLength(100)]
    public string Name { get; set; } = default!;
    /// <summary>
    /// Agent 的角色设定（System Prompt）
    /// </summary>
    public string SystemPrompt { get; set; } = string.Empty;
    /// <summary>
    /// Agent 可用的工具列表
    /// </summary>
    public List<string> Tools { get; set; } = [];
    public Guid? UserId { get; set; }
    
}
