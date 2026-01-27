using Entity.AIAgentMod;
namespace AIAgentMod.Models.AgentExecutionDtos;

/// <summary>
/// Agent 执行 UpdateDto
/// </summary>
/// <see cref="Entity.AIAgentMod.AgentExecution"/>
public class AgentExecutionUpdateDto
{
    [MaxLength(4000)]
    public string? OutputJson { get; set; }

    public DateTimeOffset? CompletedTime { get; set; }

    public int? DurationMs { get; set; }

    public AgentExecutionStatus? Status { get; set; }

    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }
}
