using System.Text.Json;
using CoreMod.Models;

namespace AIAgentMod.Services;

/// <summary>
/// 增强的 Agent 执行引擎，支持多轮对话和工具调用链路
/// 使用结构化 tool calling API（OpenAI function calling）而非从文本解析
/// </summary>
public class EnhancedAgentExecutionService(
    TenantDbFactory dbContextFactory,
    IUserContext userContext,
    IModelInvoker modelInvoker,
    IMcpToolExecutor mcpToolExecutor,
    ISystemConfigService systemConfigService,
    ILogger<EnhancedAgentExecutionService> logger
) : IAgentExecutionService
{
    private const int MaxIterations = 10;

    public async Task<bool> ExecuteAsync(Guid executionId, Guid applicationId, string? inputJson, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var execution = await dbContext.AgentExecutions
            .FirstOrDefaultAsync(q => q.Id == executionId && q.TenantId == userContext.TenantId, cancellationToken);

        if (execution is null)
        {
            return false;
        }

        var agent = execution.IsApplicationAgent
            ? await dbContext.ApplicationAgents
                .AsNoTracking()
                .Where(q => q.Id == execution.AgentId && q.TenantId == userContext.TenantId)
                .Select(q => new AgentExecutionDefinition(q.Id, q.Name, q.ModelId, q.SystemPrompt, q.Tools))
                .FirstOrDefaultAsync(cancellationToken)
            : await dbContext.AIAgents
                .AsNoTracking()
                .Where(q => q.Id == execution.AgentId && q.TenantId == userContext.TenantId)
                .Select(q => new AgentExecutionDefinition(q.Id, q.Name, q.ModelId, q.SystemPrompt, q.Tools))
                .FirstOrDefaultAsync(cancellationToken);

        if (agent is null)
        {
            execution.Status = AgentExecutionStatus.Failed;
            execution.ErrorMessage = "Agent not found";
            await dbContext.SaveChangesAsync(cancellationToken);
            return false;
        }

        // Load tool definitions from DB
        var toolDefinitions = await ToolCallParser.LoadToolDefinitionsAsync(dbContext, agent.Tools, cancellationToken);

        execution.Status = AgentExecutionStatus.Running;
        execution.ErrorMessage = null;
        execution.InputJson = inputJson ?? execution.InputJson;
        await dbContext.SaveChangesAsync(cancellationToken);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = await ExecuteAgentLoopAsync(agent, applicationId, inputJson, toolDefinitions, cancellationToken);
            
            execution.Status = result.Success ? AgentExecutionStatus.Completed : AgentExecutionStatus.Failed;
            execution.OutputJson = JsonSerializer.Serialize(result.Context);
            execution.CompletedTime = DateTimeOffset.UtcNow;
            execution.DurationMs = (int)stopwatch.ElapsedMilliseconds;
            execution.ErrorMessage = result.ErrorMessage;
            await dbContext.SaveChangesAsync(cancellationToken);
            
            return result.Success;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Agent execution failed {ExecutionId}", executionId);
            execution.Status = AgentExecutionStatus.Failed;
            execution.ErrorMessage = ex.Message;
            execution.CompletedTime = DateTimeOffset.UtcNow;
            execution.DurationMs = (int)stopwatch.ElapsedMilliseconds;
            await dbContext.SaveChangesAsync(cancellationToken);
            return false;
        }
    }

    private async Task<ExecutionResult> ExecuteAgentLoopAsync(
        AgentExecutionDefinition agent,
        Guid applicationId,
        string? inputJson,
        List<ModelToolDefinition> toolDefinitions,
        CancellationToken cancellationToken)
    {
        var messages = new List<ModelInvokeMessage>();
        var toolResults = new List<object>();
        var context = new Dictionary<string, object?>();

        // 1. Build initial messages
        var (initialMessages, promptText) = await BuildMessagesAsync(agent, inputJson, cancellationToken);
        messages.AddRange(initialMessages);

        // 2. Multi-round conversation loop
        for (var iteration = 0; iteration < MaxIterations; iteration++)
        {
            logger.LogInformation(
                "Agent {AgentName} iteration {Iteration}/{Max}",
                agent.Name, iteration + 1, MaxIterations
            );

            // Call model with tool definitions
            var response = await modelInvoker.ChatAsync(applicationId, new ModelInvokeRequest
            {
                Model = agent.ModelId,
                Scene = agent.Name,
                Messages = messages,
                ToolDefinitions = toolDefinitions,
                Metadata = new Dictionary<string, string>
                {
                    ["prompt"] = promptText,
                    ["iteration"] = iteration.ToString(),
                },
            }, cancellationToken);

            if (!response.Success)
            {
                return new ExecutionResult
                {
                    Success = false,
                    ErrorMessage = response.ErrorMessage,
                    Context = context
                };
            }

            // Check for structured tool calls from API response first,
            // then fall back to parsing from content text
            var toolCalls = response.ToolCalls.Count > 0
                ? response.ToolCalls
                : ToolCallParser.ParseFromContent(response.Content);

            EnsureToolCallIds(toolCalls);

            // Add assistant response to message history
            messages.Add(new ModelInvokeMessage
            {
                Role = "assistant",
                Content = response.Content ?? string.Empty,
                ToolCalls = toolCalls
                    .Select(toolCall => new ToolCall
                    {
                        Id = toolCall.Id,
                        Name = toolCall.Name,
                        ArgumentsJson = toolCall.ArgumentsJson,
                    })
                    .ToList(),
            });

            if (toolCalls.Count == 0)
            {
                // No tool calls, return final result
                context["final_response"] = response.Content;
                context["iterations"] = iteration + 1;
                context["tool_results"] = toolResults;
                
                return new ExecutionResult
                {
                    Success = true,
                    Context = context
                };
            }

            // Execute all tool calls
            foreach (var toolCall in toolCalls)
            {
                logger.LogInformation(
                    "Executing tool {ToolName} with args: {Args}",
                    toolCall.Name,
                    toolCall.ArgumentsJson
                );

                try
                {
                    var toolResult = await mcpToolExecutor.ExecuteAsync(new ToolExecutionRequest
                    {
                        ToolName = toolCall.Name,
                        ArgumentsJson = toolCall.ArgumentsJson,
                        ApplicationId = applicationId,
                        AgentId = agent.Id,
                    }, cancellationToken);

                    toolResults.Add(new
                    {
                        tool = toolCall.Name,
                        arguments = toolCall.ArgumentsJson,
                        result = toolResult
                    });

                    // Feed the model the raw tool payload instead of the audit envelope so it can
                    // reason over the actual result structure.
                    messages.Add(new ModelInvokeMessage
                    {
                        Role = "tool",
                        Content = BuildToolMessageContent(toolResult),
                        ToolCallId = toolCall.Id,
                    });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Tool execution failed: {ToolName}", toolCall.Name);

                    var failedResult = new ToolExecutionResult
                    {
                        Success = false,
                        ErrorMessage = ex.Message,
                    };

                    messages.Add(new ModelInvokeMessage
                    {
                        Role = "tool",
                        Content = BuildToolMessageContent(failedResult),
                        ToolCallId = toolCall.Id,
                    });
                }
            }
        }

        // Reached max iterations
        logger.LogWarning("Agent {AgentName} reached max iterations", agent.Name);
        context["iterations"] = MaxIterations;
        context["tool_results"] = toolResults;
        context["warning"] = "Reached maximum iteration count";
        
        return new ExecutionResult
        {
            Success = false,
            ErrorMessage = $"Agent execution exceeded maximum iterations ({MaxIterations})",
            Context = context
        };
    }

    private async Task<(List<ModelInvokeMessage> messages, string promptText)> BuildMessagesAsync(
        AgentExecutionDefinition agent,
        string? inputJson,
        CancellationToken cancellationToken)
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

    private static void EnsureToolCallIds(List<ToolCall> toolCalls)
    {
        foreach (var toolCall in toolCalls.Where(toolCall => string.IsNullOrWhiteSpace(toolCall.Id)))
        {
            toolCall.Id = $"call_{Guid.NewGuid():N}";
        }
    }

    private static string BuildToolMessageContent(ToolExecutionResult result)
    {
        if (result.Success)
        {
            if (string.IsNullOrWhiteSpace(result.OutputJson))
            {
                return "{}";
            }

            return result.OutputJson;
        }

        return JsonSerializer.Serialize(new
        {
            success = false,
            error = result.ErrorMessage ?? "Tool execution failed",
        });
    }

    private sealed record ExecutionResult
    {
        public bool Success { get; init; }
        public string? ErrorMessage { get; init; }
        public Dictionary<string, object?> Context { get; init; } = new();
    }
}
