using Share.Services;
using System.Text.Json;
using SystemMod.Services;

namespace AIAgentMod.Services;

/// <summary>
/// 增强的 Agent 执行引擎，支持多轮对话和工具调用链路
/// </summary>
public class EnhancedAgentExecutionService(
    TenantDbFactory dbContextFactory,
    IUserContext userContext,
    IModelInvokeFacade modelInvokeFacade,
    IMcpToolExecutorFacade mcpToolExecutorFacade,
    SystemConfigFacade systemConfigFacade,
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
            var result = await ExecuteAgentLoopAsync(agent, applicationId, inputJson, cancellationToken);
            
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
        AIAgent agent,
        Guid applicationId,
        string? inputJson,
        CancellationToken cancellationToken)
    {
        var messages = new List<ModelInvokeMessage>();
        var toolResults = new List<object>();
        var context = new Dictionary<string, object?>();

        // 1. 构建初始消息
        var (initialMessages, promptText) = await BuildMessagesAsync(agent, inputJson, cancellationToken);
        messages.AddRange(initialMessages);

        // 2. 多轮对话循环
        for (int iteration = 0; iteration < MaxIterations; iteration++)
        {
            logger.LogInformation(
                "Agent {AgentName} iteration {Iteration}/{Max}",
                agent.Name, iteration + 1, MaxIterations
            );

            // 调用模型
            var response = await modelInvokeFacade.ChatAsync(applicationId, new ModelInvokeRequest
            {
                Model = agent.ModelId,
                Scene = agent.Name,
                Messages = messages,
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

            // 添加助手响应到消息历史
            messages.Add(new ModelInvokeMessage
            {
                Role = "assistant",
                Content = response.Content ?? string.Empty
            });

            // 检查是否有工具调用
            var toolCalls = ParseToolCalls(response.Content);
            
            if (toolCalls.Count == 0)
            {
                // 没有工具调用，返回最终结果
                context["final_response"] = response.Content;
                context["iterations"] = iteration + 1;
                context["tool_results"] = toolResults;
                
                return new ExecutionResult
                {
                    Success = true,
                    Context = context
                };
            }

            // 执行所有工具调用
            foreach (var toolCall in toolCalls)
            {
                logger.LogInformation(
                    "Executing tool {ToolName} with args: {Args}",
                    toolCall.ToolName,
                    toolCall.ArgumentsJson
                );

                try
                {
                    var toolResult = await mcpToolExecutorFacade.ExecuteAsync(new ToolExecutionRequest
                    {
                        ToolName = toolCall.ToolName,
                        ArgumentsJson = toolCall.ArgumentsJson,
                        ApplicationId = applicationId,
                        AgentId = agent.Id,
                    }, cancellationToken);

                    toolResults.Add(new
                    {
                        tool = toolCall.ToolName,
                        arguments = toolCall.ArgumentsJson,
                        result = toolResult
                    });

                    // 将工具结果添加到消息历史
                    messages.Add(new ModelInvokeMessage
                    {
                        Role = "tool",
                        Content = JsonSerializer.Serialize(toolResult)
                    });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Tool execution failed: {ToolName}", toolCall.ToolName);
                    
                    // 添加工具错误到消息历史
                    messages.Add(new ModelInvokeMessage
                    {
                        Role = "tool",
                        Content = JsonSerializer.Serialize(new
                        {
                            error = ex.Message,
                            tool = toolCall.ToolName
                        })
                    });
                }
            }

            // 继续下一轮迭代
        }

        // 达到最大迭代次数
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
        AIAgent agent,
        string? inputJson,
        CancellationToken cancellationToken)
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
                // 单个工具调用
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

                // OpenAI 格式的工具调用数组
                if (root.TryGetProperty("tool_calls", out var toolCallsArray) && 
                    toolCallsArray.ValueKind == JsonValueKind.Array)
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
            // 如果无法解析 JSON，可能不是工具调用
            // 返回空列表
            return [];
        }

        return toolCalls;
    }

    private sealed record ToolCallPayload(string ToolName, string? ArgumentsJson);

    private sealed record ExecutionResult
    {
        public bool Success { get; init; }
        public string? ErrorMessage { get; init; }
        public Dictionary<string, object?> Context { get; init; } = new();
    }
}
