namespace AIAgentMod.Models.AgentDebugDtos;

public sealed class AgentDebugRequest
{
    public Guid? ApplicationId { get; set; }

    public Guid AgentId { get; set; }

    public string? SystemPrompt { get; set; }

    public string UserMessage { get; set; } = string.Empty;

    public double? Temperature { get; set; }

    public int? MaxTokens { get; set; }

    public List<string> EnabledTools { get; set; } = [];

    public bool EnableToolCallLogging { get; set; } = true;

    public string? RequestId { get; set; }
}
