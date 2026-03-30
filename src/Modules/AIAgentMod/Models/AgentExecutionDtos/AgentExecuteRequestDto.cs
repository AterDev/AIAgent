namespace AIAgentMod.Models.AgentExecutionDtos;

/// <summary>
/// Agent 执行请求
/// </summary>
public class AgentExecuteRequestDto
{
    public Guid? ApplicationId { get; set; }

    [MaxLength(4000)]
    public string? InputJson { get; set; }
}
