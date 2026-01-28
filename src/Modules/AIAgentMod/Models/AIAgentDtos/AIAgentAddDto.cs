namespace AIAgentMod.Models.AIAgentDtos;
/// <summary>
/// agentAddDto
/// </summary>
/// <see cref="Entity.AIAgentMod.AIAgent"/>
public class AIAgentAddDto
{
    /// <summary>
    /// Agent 名称
    /// </summary>
    [Required(ErrorMessage = "Agent名称不能为空")]
    [MaxLength(100, ErrorMessage = "Agent名称长度不能超过100个字符")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Agent 描述信息
    /// </summary>
    [MaxLength(500, ErrorMessage = "描述信息长度不能超过500个字符")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Agent 所使用的大模型名称（例如 "gpt-4", "qwen-max", "custom-llm"）
    /// </summary>
    [Required(ErrorMessage = "模型ID不能为空")]
    [MaxLength(100, ErrorMessage = "模型ID长度不能超过100个字符")]
    public string ModelId { get; set; } = default!;

    /// <summary>
    /// Agent 的角色设定（System Prompt）
    /// </summary>
    [MaxLength(4000, ErrorMessage = "系统提示词长度不能超过4000个字符")]
    public string SystemPrompt { get; set; } = string.Empty;

    /// <summary>
    /// Agent 可用的工具列表
    /// </summary>
    public List<string> Tools { get; set; } = [];

    /// <summary>
    /// is enabled
    /// </summary>
    public bool Enable { get; set; }

    public bool IsTemplate { get; set; }

    public Guid? UserId { get; set; }
}
