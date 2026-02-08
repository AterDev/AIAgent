using CoreMod.Models;
using Share.Services;
using System.Text.Json;
using CoreMod.Services;

namespace AIAgentMod.Services;

/// <summary>
/// Agent 执行引擎（简化）
/// </summary>
public class AgentExecutionService(
    TenantDbFactory dbContextFactory,
    IUserContext userContext,
    IModelInvoker modelInvoker,
    IMcpToolExecutor mcpToolExecutor,
    ISystemConfigService systemConfigService,
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
            var response = await modelInvoker.ChatAsync(applicationId, new ModelInvokeRequest
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
                var toolCalls = ToolCallParser.ParseFromContent(response.Content);
                if (toolCalls.Count > 0)
                {
                    var firstToolCall = toolCalls[0];
                    var toolResult = await mcpToolExecutor.ExecuteAsync(new ToolExecutionRequest
                    {
                        ToolName = firstToolCall.Name,
                        ArgumentsJson = firstToolCall.ArgumentsJson,
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

        var template = await systemConfigService.GetValueAsync("Agent", agent.SystemPrompt, cancellationToken);
        if (!string.IsNullOrWhiteSpace(template))
        {
            systemPrompt = systemConfigService.RenderTemplate(template, new Dictionary<string, string>
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

}
