using System.Text.Json;
using Share.Services;

namespace AIAgentMod.Services;

/// <summary>
/// Agent 执行引擎（简化）
/// </summary>
public class AgentExecutionService(
    TenantDbFactory dbContextFactory,
    IUserContext userContext,
    IModelInvokeFacade modelInvokeFacade,
    IMcpToolExecutorFacade mcpToolExecutorFacade,
    ISystemConfigFacade systemConfigFacade,
    ILogger<AgentExecutionService> logger
) : IAgentExecutionService
{
    public async Task<bool> ExecuteAsync(Guid executionId, Guid applicationId, string? inputJson, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var execution = await dbContext.AgentExecutions
            .FirstOrDefaultAsync(q => q.Id == executionId && q.TenantId == userContext.TenantId, cancellationToken);

        if (execution is null)
        {
            return false;
        }

        var agent = await dbContext.AIAgents
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == execution.AgentId && q.TenantId == userContext.TenantId, cancellationToken);

        if (agent is null)
        {
            execution.Status = AgentExecutionStatus.Failed;
            execution.ErrorMessage = "Agent not found";
            await dbContext.SaveChangesAsync(cancellationToken);
            return false;
        }

        execution.Status = AgentExecutionStatus.Running;
        execution.ErrorMessage = null;
        execution.InputJson = inputJson ?? execution.InputJson;
        await dbContext.SaveChangesAsync(cancellationToken);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var (messages, promptText) = await BuildMessagesAsync(agent, inputJson, cancellationToken);
            var response = await modelInvokeFacade.ChatAsync(applicationId, new ModelInvokeRequest
            {
                Model = agent.ModelId,
                Scene = agent.Name,
                Messages = messages,
                Metadata = new Dictionary<string, string>
                {
                    ["prompt"] = promptText,
                },
            }, cancellationToken);

            var context = new Dictionary<string, object?>
            {
                ["model"] = response,
            };

            if (response.Success)
            {
                var toolCall = TryParseToolCall(response.Content);
                if (toolCall is not null)
                {
                    var toolResult = await mcpToolExecutorFacade.ExecuteAsync(new ToolExecutionRequest
                    {
                        ToolName = toolCall.ToolName,
                        ArgumentsJson = toolCall.ArgumentsJson,
                        ApplicationId = applicationId,
                        AgentId = agent.Id,
                    }, cancellationToken);

                    context["tool"] = toolResult;
                }
            }

            execution.Status = response.Success ? AgentExecutionStatus.Completed : AgentExecutionStatus.Failed;
            execution.OutputJson = JsonSerializer.Serialize(context);
            execution.CompletedTime = DateTimeOffset.UtcNow;
            execution.DurationMs = (int)stopwatch.ElapsedMilliseconds;
            execution.ErrorMessage = response.ErrorMessage;
            await dbContext.SaveChangesAsync(cancellationToken);
            return response.Success;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Agent execution failed {ExecutionId}", executionId);
            execution.Status = AgentExecutionStatus.Failed;
            execution.ErrorMessage = ex.Message;
            execution.CompletedTime = DateTimeOffset.UtcNow;
            execution.DurationMs = (int)stopwatch.ElapsedMilliseconds;
            await dbContext.SaveChangesAsync(cancellationToken);
            return false;
        }
    }

    private async Task<(List<ModelInvokeMessage> messages, string promptText)> BuildMessagesAsync(AIAgent agent, string? inputJson, CancellationToken cancellationToken)
    {
        var prompt = ExtractPrompt(inputJson);
        var systemPrompt = agent.SystemPrompt;

        var template = await systemConfigFacade.GetValueAsync("Agent", agent.SystemPrompt, cancellationToken);
        if (!string.IsNullOrWhiteSpace(template))
        {
            systemPrompt = systemConfigFacade.RenderTemplate(template, new Dictionary<string, string>
            {
                ["input"] = prompt,
            });
        }

        var messages = new List<ModelInvokeMessage>();
        if (!string.IsNullOrWhiteSpace(systemPrompt))
        {
            messages.Add(new ModelInvokeMessage { Role = "system", Content = systemPrompt });
        }

        if (!string.IsNullOrWhiteSpace(prompt))
        {
            messages.Add(new ModelInvokeMessage { Role = "user", Content = prompt });
        }

        return (messages, prompt);
    }

    private static string ExtractPrompt(string? inputJson)
    {
        if (string.IsNullOrWhiteSpace(inputJson))
        {
            return string.Empty;
        }

        try
        {
            using var doc = JsonDocument.Parse(inputJson);
            var root = doc.RootElement;
            if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("prompt", out var prompt))
                {
                    return prompt.GetString() ?? string.Empty;
                }

                if (root.TryGetProperty("message", out var message))
                {
                    return message.GetString() ?? string.Empty;
                }
            }
        }
        catch (JsonException)
        {
            return inputJson;
        }

        return inputJson;
    }

    private static ToolCallPayload? TryParseToolCall(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;
            if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("tool", out var tool) || root.TryGetProperty("toolName", out tool))
                {
                    var name = tool.GetString();
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        var arguments = root.TryGetProperty("arguments", out var args)
                            ? args.GetRawText()
                            : null;
                        return new ToolCallPayload(name, arguments);
                    }
                }

                if (root.TryGetProperty("tool_calls", out var toolCalls) && toolCalls.ValueKind == JsonValueKind.Array)
                {
                    var first = toolCalls.EnumerateArray().FirstOrDefault();
                    if (first.ValueKind == JsonValueKind.Object && first.TryGetProperty("name", out var toolName))
                    {
                        var name = toolName.GetString();
                        var args = first.TryGetProperty("arguments", out var argsElement) ? argsElement.GetRawText() : null;
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            return new ToolCallPayload(name, args);
                        }
                    }
                }
            }
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }

    private sealed record ToolCallPayload(string ToolName, string? ArgumentsJson);
}
