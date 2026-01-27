using System.Runtime.CompilerServices;

namespace AIAgentMod.Services;

/// <summary>
/// 流式 Agent 执行器，支持实时响应和工具调用追踪
/// </summary>
public class StreamingAgentExecutor : IStreamingAgentExecutor
{
    private readonly AgUiCommunicationService _communicationService;
    private readonly ILogger<StreamingAgentExecutor> _logger;

    public StreamingAgentExecutor(
        AgUiCommunicationService communicationService,
        ILogger<StreamingAgentExecutor> logger)
    {
        _communicationService = communicationService;
        _logger = logger;
    }

    public async IAsyncEnumerable<AgentExecutionEvent> ExecuteStreamAsync(
        Guid agentId,
        string userMessage,
        Guid? threadId = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var effectiveThreadId = threadId ?? Guid.NewGuid();
        var threadIdStr = effectiveThreadId.ToString();

        _logger.LogInformation(
            "Starting streaming execution for agent {AgentId} in thread {ThreadId}",
            agentId,
            effectiveThreadId);

        // 发送开始事件
        yield return new AgentExecutionEvent
        {
            Type = "message_start",
            ThreadId = effectiveThreadId
        };

        await SendAgUiMessageAsync(threadIdStr, new AgUiMessage
        {
            Type = "message_start",
            ThreadId = threadIdStr
        }, cancellationToken);

        try
        {
            // TODO: 这里应该调用真实的 Agent 执行逻辑
            // 目前使用模拟响应来演示流式传输
            var response = GenerateMockResponse(userMessage);

            // 模拟流式响应：逐块发送内容
            foreach (var chunk in SplitIntoChunks(response, chunkSize: 5))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Execution cancelled for thread {ThreadId}", effectiveThreadId);
                    break;
                }

                yield return new AgentExecutionEvent
                {
                    Type = "content_block",
                    Content = chunk,
                    ThreadId = effectiveThreadId
                };

                await SendAgUiMessageAsync(threadIdStr, new AgUiMessage
                {
                    Type = "content_block",
                    Content = chunk,
                    ThreadId = threadIdStr
                }, cancellationToken);

                // 模拟流式延迟
                await Task.Delay(50, cancellationToken);
            }

            // 模拟工具调用
            if (userMessage.Contains("查询", StringComparison.OrdinalIgnoreCase) ||
                userMessage.Contains("search", StringComparison.OrdinalIgnoreCase))
            {
                yield return await SimulateToolCallAsync(threadIdStr, cancellationToken);
            }

            // 发送结束事件
            var usage = new AgUiTokenUsage
            {
                PromptTokens = EstimateTokens(userMessage),
                CompletionTokens = EstimateTokens(response),
                TotalTokens = EstimateTokens(userMessage) + EstimateTokens(response)
            };

            yield return new AgentExecutionEvent
            {
                Type = "message_end",
                ThreadId = effectiveThreadId,
                Usage = usage
            };

            await SendAgUiMessageAsync(threadIdStr, new AgUiMessage
            {
                Type = "message_end",
                ThreadId = threadIdStr,
                Usage = usage
            }, cancellationToken);

            _logger.LogInformation(
                "Streaming execution completed for thread {ThreadId}",
                effectiveThreadId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during streaming execution for thread {ThreadId}", effectiveThreadId);

            yield return new AgentExecutionEvent
            {
                Type = "error",
                ThreadId = effectiveThreadId,
                Content = ex.Message
            };

            await SendAgUiMessageAsync(threadIdStr, new AgUiMessage
            {
                Type = "error",
                ThreadId = threadIdStr,
                Content = ex.Message,
                Metadata = new Dictionary<string, object>
                {
                    ["error_type"] = ex.GetType().Name
                }
            }, cancellationToken);
        }
    }

    private async Task SendAgUiMessageAsync(string threadId, AgUiMessage message, CancellationToken cancellationToken)
    {
        try
        {
            if (_communicationService.IsConnected(threadId))
            {
                await _communicationService.SendMessageAsync(threadId, message, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send AG-UI message to thread {ThreadId}", threadId);
        }
    }

    private async Task<AgentExecutionEvent> SimulateToolCallAsync(string threadId, CancellationToken cancellationToken)
    {
        var toolCall = new AgUiToolCall
        {
            Name = "query_knowledge_base",
            Arguments = "{\"query\": \"测试查询\", \"topK\": 5}",
            DurationMs = 0
        };

        // 发送工具调用开始事件
        await SendAgUiMessageAsync(threadId, new AgUiMessage
        {
            Type = "tool_call_start",
            ThreadId = threadId,
            ToolCall = toolCall
        }, cancellationToken);

        // 模拟工具执行
        await Task.Delay(200, cancellationToken);

        // 设置结果
        toolCall = toolCall with
        {
            Result = "[{\"content\": \"相关文档内容\", \"score\": 0.95}]",
            DurationMs = 200
        };

        // 发送工具调用完成事件
        await SendAgUiMessageAsync(threadId, new AgUiMessage
        {
            Type = "tool_call_end",
            ThreadId = threadId,
            ToolCall = toolCall
        }, cancellationToken);

        return new AgentExecutionEvent
        {
            Type = "tool_call",
            ThreadId = Guid.Parse(threadId),
            ToolCall = toolCall
        };
    }

    private static string GenerateMockResponse(string userMessage)
    {
        return $"这是对您的消息「{userMessage}」的模拟响应。AG-UI 集成正常工作。";
    }

    private static IEnumerable<string> SplitIntoChunks(string text, int chunkSize)
    {
        for (int i = 0; i < text.Length; i += chunkSize)
        {
            yield return text.Substring(i, Math.Min(chunkSize, text.Length - i));
        }
    }

    private static int EstimateTokens(string text)
    {
        // 简单估算：中文按字符数，英文按空格分词数
        return text.Length / 2;
    }
}

/// <summary>
/// 流式 Agent 执行器接口
/// </summary>
public interface IStreamingAgentExecutor
{
    /// <summary>
    /// 流式执行 Agent
    /// </summary>
    /// <param name="agentId">Agent ID</param>
    /// <param name="userMessage">用户消息</param>
    /// <param name="threadId">线程ID（可选）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>执行事件流</returns>
    IAsyncEnumerable<AgentExecutionEvent> ExecuteStreamAsync(
        Guid agentId,
        string userMessage,
        Guid? threadId = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Agent 执行事件
/// </summary>
public record AgentExecutionEvent
{
    /// <summary>事件类型: message_start, content_block, tool_call, message_end, error</summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>线程ID</summary>
    public Guid? ThreadId { get; init; }

    /// <summary>内容（用于 content_block）</summary>
    public string? Content { get; init; }

    /// <summary>工具调用信息</summary>
    public AgUiToolCall? ToolCall { get; init; }

    /// <summary>Token 使用情况</summary>
    public AgUiTokenUsage? Usage { get; init; }

    /// <summary>时间戳</summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}
