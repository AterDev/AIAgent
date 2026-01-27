namespace AIAgentMod.Services;

public record AgentExecutionTask(Guid ExecutionId, Guid ApplicationId, string? InputJson);
