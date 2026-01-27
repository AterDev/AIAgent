using AIAgentMod.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiService.Controllers;

/// <summary>
/// AG-UI 集成控制器，提供 WebSocket 连接和流式 Agent 执行
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AgUiController : ControllerBase
{
    private readonly AgUiCommunicationService _communicationService;
    private readonly IStreamingAgentExecutor _streamingExecutor;
    private readonly ILogger<AgUiController> _logger;

    public AgUiController(
        AgUiCommunicationService communicationService,
        IStreamingAgentExecutor streamingExecutor,
        ILogger<AgUiController> logger)
    {
        _communicationService = communicationService;
        _streamingExecutor = streamingExecutor;
        _logger = logger;
    }

    /// <summary>
    /// WebSocket 端点：建立 AG-UI 连接
    /// </summary>
    /// <param name="threadId">线程ID，用于标识会话</param>
    [Route("ws/{threadId}")]
    public async Task WebSocket(string threadId)
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            _logger.LogInformation("WebSocket connection established for thread {ThreadId}", threadId);

            await _communicationService.HandleWebSocketAsync(
                webSocket,
                threadId,
                HttpContext.RequestAborted);

            _logger.LogInformation("WebSocket connection closed for thread {ThreadId}", threadId);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsync("WebSocket connection expected");
        }
    }

    /// <summary>
    /// HTTP 端点：发送消息到 Agent 并触发流式执行
    /// </summary>
    /// <param name="request">Agent 消息请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>执行事件列表</returns>
    [HttpPost("message")]
    [ProducesResponseType(typeof(AgentMessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendMessage(
        [FromBody] AgentMessageRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest("Message is required");
        }

        var events = new List<AgentExecutionEvent>();

        try
        {
            await foreach (var evt in _streamingExecutor.ExecuteStreamAsync(
                request.AgentId,
                request.Message,
                request.ThreadId,
                cancellationToken))
            {
                events.Add(evt);
            }

            return Ok(new AgentMessageResponse
            {
                ThreadId = request.ThreadId ?? events.FirstOrDefault()?.ThreadId ?? Guid.NewGuid(),
                Events = events,
                Success = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing agent for message: {Message}", request.Message);
            return BadRequest(new AgentMessageResponse
            {
                ThreadId = request.ThreadId ?? Guid.NewGuid(),
                Events = events,
                Success = false,
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// 获取活跃连接统计信息
    /// </summary>
    /// <returns>连接统计</returns>
    [HttpGet("status")]
    [ProducesResponseType(typeof(AgUiStatusResponse), StatusCodes.Status200OK)]
    public IActionResult GetStatus()
    {
        return Ok(new AgUiStatusResponse
        {
            ActiveConnections = _communicationService.GetActiveConnectionCount(),
            ServerTime = DateTimeOffset.UtcNow
        });
    }
}

/// <summary>
/// Agent 消息请求
/// </summary>
public record AgentMessageRequest
{
    /// <summary>Agent ID</summary>
    public Guid AgentId { get; init; }

    /// <summary>用户消息</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>线程ID（可选，用于维持会话上下文）</summary>
    public Guid? ThreadId { get; init; }
}

/// <summary>
/// Agent 消息响应
/// </summary>
public record AgentMessageResponse
{
    /// <summary>线程ID</summary>
    public Guid ThreadId { get; init; }

    /// <summary>执行事件列表</summary>
    public List<AgentExecutionEvent> Events { get; init; } = [];

    /// <summary>是否成功</summary>
    public bool Success { get; init; }

    /// <summary>错误信息</summary>
    public string? Error { get; init; }
}

/// <summary>
/// AG-UI 状态响应
/// </summary>
public record AgUiStatusResponse
{
    /// <summary>活跃连接数</summary>
    public int ActiveConnections { get; init; }

    /// <summary>服务器时间</summary>
    public DateTimeOffset ServerTime { get; init; }
}
