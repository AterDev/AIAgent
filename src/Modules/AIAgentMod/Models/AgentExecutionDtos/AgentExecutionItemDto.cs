namespace AIAgentMod.Models.AgentExecutionDtos;

/// <summary>
/// Agent 执行 ItemDto
/// </summary>
/// <see cref="AgentExecution"/>
public class AgentExecutionItemDto
{
    public Guid Id { get; set; }
    public Guid AgentId { get; set; }
    public AgentExecutionStatus Status { get; set; }
    public int DurationMs { get; set; }
}
