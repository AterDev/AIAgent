using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace AIAgentMod.Services;

/// <summary>
/// AG-UI 通信服务，处理 WebSocket 连接和消息流
/// </summary>
public class AgUiCommunicationService
{
    private readonly ILogger<AgUiCommunicationService> _logger;
    private readonly ConcurrentDictionary<string, WebSocket> _connections = new();
    private readonly ConcurrentDictionary<string, Channel<AgUiMessage>> _messageChannels = new();

    public AgUiCommunicationService(ILogger<AgUiCommunicationService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 处理 WebSocket 连接
    /// </summary>
    public async Task HandleWebSocketAsync(
        WebSocket webSocket,
        string threadId,
        CancellationToken cancellationToken)
    {
        _connections[threadId] = webSocket;
        var channel = Channel.CreateUnbounded<AgUiMessage>();
        _messageChannels[threadId] = channel;

        _logger.LogInformation("WebSocket connection established for thread {ThreadId}", threadId);

        try
        {
            // 启动消息发送任务
            var sendTask = SendMessagesAsync(webSocket, channel.Reader, cancellationToken);

            // 接收客户端消息
            var receiveTask = ReceiveMessagesAsync(webSocket, threadId, cancellationToken);

            await Task.WhenAny(sendTask, receiveTask);
        }
        finally
        {
            // 清理
            _connections.TryRemove(threadId, out _);
            _messageChannels.TryRemove(threadId, out _);
            _logger.LogInformation("WebSocket connection closed for thread {ThreadId}", threadId);
        }
    }

    /// <summary>
    /// 发送消息到客户端
    /// </summary>
    public async Task SendMessageAsync(string threadId, AgUiMessage message, CancellationToken cancellationToken = default)
    {
        if (_messageChannels.TryGetValue(threadId, out var channel))
        {
            await channel.Writer.WriteAsync(message, cancellationToken);
        }
    }

    /// <summary>
    /// 检查连接是否活跃
    /// </summary>
    public bool IsConnected(string threadId)
    {
        return _connections.TryGetValue(threadId, out var ws) && ws.State == WebSocketState.Open;
    }

    /// <summary>
    /// 获取活跃连接数
    /// </summary>
    public int GetActiveConnectionCount()
    {
        return _connections.Count(kvp => kvp.Value.State == WebSocketState.Open);
    }

    private async Task SendMessagesAsync(
        WebSocket webSocket,
        ChannelReader<AgUiMessage> reader,
        CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var message in reader.ReadAllAsync(cancellationToken))
            {
                if (webSocket.State != WebSocketState.Open)
                {
                    break;
                }

                var json = JsonSerializer.Serialize(message, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                var bytes = Encoding.UTF8.GetBytes(json);

                await webSocket.SendAsync(
                    new ArraySegment<byte>(bytes),
                    WebSocketMessageType.Text,
                    endOfMessage: true,
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending messages through WebSocket");
        }
    }

    private async Task ReceiveMessagesAsync(
        WebSocket webSocket,
        string threadId,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[4096];
        var messageBuffer = new List<byte>();

        try
        {
            while (webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                var result = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Closed by client",
                        cancellationToken);
                    break;
                }

                messageBuffer.AddRange(buffer.Take(result.Count));

                if (result.EndOfMessage)
                {
                    var json = Encoding.UTF8.GetString(messageBuffer.ToArray());
                    _logger.LogDebug("Received message from thread {ThreadId}: {Message}", threadId, json);

                    // 可以在这里处理客户端发来的消息，比如取消请求、暂停等
                    // 目前只记录日志

                    messageBuffer.Clear();
                }
            }
        }
        catch (WebSocketException ex)
        {
            _logger.LogWarning(ex, "WebSocket error for thread {ThreadId}", threadId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error receiving messages from WebSocket for thread {ThreadId}", threadId);
        }
    }
}

/// <summary>
/// AG-UI 消息格式
/// </summary>
public record AgUiMessage
{
    /// <summary>消息类型: message_start, content_block, tool_call_start, tool_call_end, message_end</summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>线程ID</summary>
    public string? ThreadId { get; init; }

    /// <summary>消息内容（用于 content_block）</summary>
    public string? Content { get; init; }

    /// <summary>工具调用信息</summary>
    public AgUiToolCall? ToolCall { get; init; }

    /// <summary>Token 使用情况</summary>
    public AgUiTokenUsage? Usage { get; init; }

    /// <summary>额外元数据</summary>
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>时间戳</summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// 工具调用信息
/// </summary>
public record AgUiToolCall
{
    /// <summary>工具名称</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>工具参数（JSON格式）</summary>
    public string Arguments { get; init; } = string.Empty;

    /// <summary>工具执行结果</summary>
    public string? Result { get; init; }

    /// <summary>错误信息</summary>
    public string? Error { get; init; }

    /// <summary>执行耗时（毫秒）</summary>
    public long? DurationMs { get; init; }
}

/// <summary>
/// Token 使用情况
/// </summary>
public record AgUiTokenUsage
{
    /// <summary>Prompt Token 数</summary>
    public int PromptTokens { get; init; }

    /// <summary>Completion Token 数</summary>
    public int CompletionTokens { get; init; }

    /// <summary>总 Token 数</summary>
    public int TotalTokens { get; init; }
}
