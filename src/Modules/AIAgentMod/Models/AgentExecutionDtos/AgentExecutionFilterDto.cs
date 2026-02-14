namespace AIAgentMod.Models.AgentExecutionDtos;

/// <summary>
/// Agent 执行 FilterDto
/// </summary>
/// <see cref="AgentExecution"/>
public class AgentExecutionFilterDto : FilterBase
{
    public Guid? AgentId { get; set; }
    public AgentExecutionStatus? Status { get; set; }
}
