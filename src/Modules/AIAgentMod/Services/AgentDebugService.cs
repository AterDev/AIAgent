using AIAgentMod.Models.AgentDebugDtos;
using CoreMod.Models;
using CoreMod.Services;
using Share.Services;
using System.Text.Json;
using SystemMod.Services;

namespace AIAgentMod.Services;

public class AgentDebugService(
    TenantDbFactory dbContextFactory,
    IUserContext userContext,
    IModelInvokeFacade modelInvokeFacade,
    ExtensionsAIModelClient modelClient,
    IMcpToolExecutorFacade mcpToolExecutorFacade,
    SystemConfigFacade systemConfigFacade,
    ILogger<AgentDebugService> logger
)
{
    private const int MaxIterations = 10;

    public async Task<string> ExecuteStreamAsync(
        AgentDebugRequest request,
        Func<AgentDebugStreamEvent, Task> onEvent,
        CancellationToken cancellationToken = default)
    {
        var requestId = string.IsNullOrWhiteSpace(request.RequestId)
            ? Guid.NewGuid().ToString("N")
            : request.RequestId;

        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var agent = await dbContext.AIAgents
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == request.AgentId && q.TenantId == userContext.TenantId, cancellationToken);

        if (agent is null)
        {
            await onEvent(new AgentDebugStreamEvent
            {
                Type = "error",
                RequestId = requestId,
                Error = "Agent not found",
            });
            return requestId;
        }

        if (!userContext.IsAdmin && !request.ApplicationId.HasValue)
        {
            await onEvent(new AgentDebugStreamEvent
            {
                Type = "error",
                RequestId = requestId,
                Error = "Application is required",
            });
            return requestId;
        }

        var effectiveApplicationId = userContext.IsAdmin ? null : request.ApplicationId;
        if (effectiveApplicationId.HasValue)
        {
            var application = await dbContext.Applications
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.Id == effectiveApplicationId && q.TenantId == userContext.TenantId && q.IsEnabled, cancellationToken);
            if (application is null)
            {
                await onEvent(new AgentDebugStreamEvent
                {
                    Type = "error",
                    RequestId = requestId,
                    Error = "Application not found",
                });
                return requestId;
            }
        }

        await onEvent(new AgentDebugStreamEvent
        {
            Type = "meta",
            RequestId = requestId,
            Message = new AgentDebugMessage
            {
                Role = "system",
                Content = $"Agent: {agent.Name}",
            }
        });

        var stopwatch = Stopwatch.StartNew();
        var messages = new List<ModelInvokeMessage>();
        var toolResults = new List<object>();
        var metrics = new AgentDebugMetrics();
        var enabledTools = request.EnabledTools.Count > 0 ? request.EnabledTools : agent.Tools;

        var (initialMessages, promptText) = await BuildMessagesAsync(agent, request.SystemPrompt, request.UserMessage, cancellationToken);
        messages.AddRange(initialMessages);

        foreach (var message in initialMessages)
        {
            await onEvent(new AgentDebugStreamEvent
            {
                Type = "message",
                RequestId = requestId,
                Message = new AgentDebugMessage
                {
                    Role = message.Role,
                    Content = message.Content,
                    Timestamp = DateTimeOffset.UtcNow,
                }
            });
        }

        for (var iteration = 0; iteration < MaxIterations; iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var response = await InvokeModelAsync(effectiveApplicationId, agent, messages, promptText, request, iteration, cancellationToken);
            if (!response.Success)
            {
                await onEvent(new AgentDebugStreamEvent
                {
                    Type = "error",
                    RequestId = requestId,
                    Error = response.ErrorMessage ?? "Model invocation failed",
                });
                return requestId;
            }

            metrics.PromptTokens += response.Usage.PromptTokens;
            metrics.CompletionTokens += response.Usage.CompletionTokens;
            metrics.TotalTokens += response.Usage.TotalTokens;

            var assistantContent = response.Content ?? string.Empty;
            messages.Add(new ModelInvokeMessage { Role = "assistant", Content = assistantContent });

            await onEvent(new AgentDebugStreamEvent
            {
                Type = "message",
                RequestId = requestId,
                Message = new AgentDebugMessage
                {
                    Role = "assistant",
                    Content = assistantContent,
                    Timestamp = DateTimeOffset.UtcNow,
                }
            });

            var toolCalls = ParseToolCalls(assistantContent);
            if (toolCalls.Count == 0)
            {
                stopwatch.Stop();
                metrics.DurationMs = (int)stopwatch.ElapsedMilliseconds;
                metrics.ToolCallCount = toolResults.Count;

                await onEvent(new AgentDebugStreamEvent
                {
                    Type = "done",
                    RequestId = requestId,
                    Metrics = metrics,
                });
                return requestId;
            }

            foreach (var toolCall in toolCalls)
            {
                if (enabledTools.Count > 0 && !enabledTools.Contains(toolCall.ToolName, StringComparer.OrdinalIgnoreCase))
                {
                    var denied = new ToolExecutionResult
                    {
                        Success = false,
                        ErrorMessage = "Tool is not enabled for this session",
                    };

                    toolResults.Add(new
                    {
                        tool = toolCall.ToolName,
                        arguments = toolCall.ArgumentsJson,
                        result = denied
                    });

                    await onEvent(new AgentDebugStreamEvent
                    {
                        Type = "tool",
                        RequestId = requestId,
                        ToolCall = new AgentDebugToolCall
                        {
                            Name = toolCall.ToolName,
                            Input = toolCall.ArgumentsJson,
                            Output = denied,
                            Timestamp = DateTimeOffset.UtcNow,
                        }
                    });

                    messages.Add(new ModelInvokeMessage
                    {
                        Role = "tool",
                        Content = JsonSerializer.Serialize(denied)
                    });

                    continue;
                }

                ToolExecutionResult toolResult;
                try
                {
                    toolResult = await mcpToolExecutorFacade.ExecuteAsync(new ToolExecutionRequest
                    {
                        ToolName = toolCall.ToolName,
                        ArgumentsJson = toolCall.ArgumentsJson,
                        ApplicationId = effectiveApplicationId,
                        AgentId = agent.Id,
                    }, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Tool execution failed: {ToolName}", toolCall.ToolName);
                    toolResult = new ToolExecutionResult
                    {
                        Success = false,
                        ErrorMessage = ex.Message,
                    };
                }

                toolResults.Add(new
                {
                    tool = toolCall.ToolName,
                    arguments = toolCall.ArgumentsJson,
                    result = toolResult
                });

                if (request.EnableToolCallLogging)
                {
                    await onEvent(new AgentDebugStreamEvent
                    {
                        Type = "tool",
                        RequestId = requestId,
                        ToolCall = new AgentDebugToolCall
                        {
                            Name = toolCall.ToolName,
                            Input = toolCall.ArgumentsJson,
                            Output = toolResult,
                            Timestamp = DateTimeOffset.UtcNow,
                        }
                    });
                }

                messages.Add(new ModelInvokeMessage
                {
                    Role = "tool",
                    Content = JsonSerializer.Serialize(toolResult)
                });
            }
        }

        stopwatch.Stop();
        metrics.DurationMs = (int)stopwatch.ElapsedMilliseconds;
        metrics.ToolCallCount = toolResults.Count;

        await onEvent(new AgentDebugStreamEvent
        {
            Type = "error",
            RequestId = requestId,
            Error = $"Agent execution exceeded maximum iterations ({MaxIterations})",
            Metrics = metrics,
        });

        return requestId;
    }

    private async Task<ModelInvokeResponse> InvokeModelAsync(
        Guid? applicationId,
        AIAgent agent,
        List<ModelInvokeMessage> messages,
        string promptText,
        AgentDebugRequest request,
        int iteration,
        CancellationToken cancellationToken)
    {
        var metadata = new Dictionary<string, string>
        {
            ["prompt"] = promptText,
            ["iteration"] = iteration.ToString(),
        };

        if (request.Temperature.HasValue)
        {
            metadata["temperature"] = request.Temperature.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        if (request.MaxTokens.HasValue)
        {
            metadata["max_tokens"] = request.MaxTokens.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        var invokeRequest = new ModelInvokeRequest
        {
            Model = agent.ModelId,
            Scene = agent.Name,
            Messages = messages,
            Metadata = metadata,
        };

        if (!applicationId.HasValue)
        {
            // 从数据库获取完整的模型信息（包括 Provider）
            await using var dbContext = await dbContextFactory.CreateDbContextAsync();
            
            // 支持通过 ID 或 Name 查询模型
            var isGuid = Guid.TryParse(agent.ModelId, out var modelId);
            
            var modelInfo = await dbContext.AIModelInfos
                .AsNoTracking()
                .Include(m => m.Provider)
                .FirstOrDefaultAsync(
                    m => (isGuid ? m.Id == modelId : m.Name == agent.ModelId)
                        && m.TenantId == userContext.TenantId
                        && m.IsEnabled,
                    cancellationToken
                );

            if (modelInfo?.Provider is null)
            {
                return new ModelInvokeResponse
                {
                    Success = false,
                    ErrorMessage = $"Model or provider not found for: {agent.ModelId}",
                    Usage = new Share.Services.UsageStats()
                };
            }

            var modelRequest = new ModelRequest
            {
                Model = modelInfo.Name,  // 使用模型名称而不是 ID
                Provider = modelInfo.Provider.Name,  // 设置 Provider
                Scene = invokeRequest.Scene,
                Messages = invokeRequest.Messages.Select(m => new ModelMessage { Role = m.Role, Content = m.Content }).ToList(),
                Metadata = invokeRequest.Metadata,
            };

            var response = await modelClient.ChatAsync(modelRequest, cancellationToken);
            return new ModelInvokeResponse
            {
                Success = response.Success,
                Content = response.Content,
                ErrorMessage = response.ErrorMessage,
                Usage = new Share.Services.UsageStats
                {
                    PromptTokens = response.Usage.PromptTokens,
                    CompletionTokens = response.Usage.CompletionTokens,
                    TotalTokens = response.Usage.TotalTokens,
                }
            };
        }

        return await modelInvokeFacade.ChatAsync(applicationId.Value, invokeRequest, cancellationToken);
    }

    private async Task<(List<ModelInvokeMessage> messages, string promptText)> BuildMessagesAsync(
        AIAgent agent,
        string? systemPromptOverride,
        string? userMessage,
        CancellationToken cancellationToken)
    {
        var prompt = userMessage ?? string.Empty;
        var systemPrompt = systemPromptOverride ?? agent.SystemPrompt;

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

    private static List<ToolCallPayload> ParseToolCalls(string? content)
    {
        var toolCalls = new List<ToolCallPayload>();

        if (string.IsNullOrWhiteSpace(content))
        {
            return toolCalls;
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
                        toolCalls.Add(new ToolCallPayload(name, arguments));
                    }
                }

                if (root.TryGetProperty("tool_calls", out var toolCallsArray) && toolCallsArray.ValueKind == JsonValueKind.Array)
                {
                    foreach (var toolCallElement in toolCallsArray.EnumerateArray())
                    {
                        if (toolCallElement.ValueKind == JsonValueKind.Object)
                        {
                            var name = toolCallElement.TryGetProperty("name", out var nameElement)
                                ? nameElement.GetString()
                                : null;

                            var args = toolCallElement.TryGetProperty("arguments", out var argsElement)
                                ? argsElement.GetRawText()
                                : null;

                            if (!string.IsNullOrWhiteSpace(name))
                            {
                                toolCalls.Add(new ToolCallPayload(name, args));
                            }
                        }
                    }
                }
            }
        }
        catch (JsonException)
        {
            return [];
        }

        return toolCalls;
    }

    private sealed record ToolCallPayload(string ToolName, string? ArgumentsJson);
}
