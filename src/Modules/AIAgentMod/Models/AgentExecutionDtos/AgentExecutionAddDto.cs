namespace AIAgentMod.Models.AgentExecutionDtos;

/// <summary>
/// Agent 执行 AddDto
/// </summary>
/// <see cref="Entity.AIAgentMod.AgentExecution"/>
public class AgentExecutionAddDto
{
    public Guid AgentId { get; set; }

    [MaxLength(4000)]
    public string? InputJson { get; set; }

    public AgentExecutionStatus Status { get; set; }
}
