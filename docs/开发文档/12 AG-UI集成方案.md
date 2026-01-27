# AG-UI 集成方案

> **文档版本**: v1.0  
> **创建日期**: 2025-01-27  
> **目的**: 为 AIAgent 项目集成 AG-UI (Agent UI Protocol) 以提供 AI Agent 调试和交互界面

---

## 目录

1. [AG-UI 简介](#ag-ui-简介)
2. [集成目标](#集成目标)
3. [技术架构](#技术架构)
4. [实施方案](#实施方案)
5. [后端实现](#后端实现)
6. [前端实现](#前端实现)
7. [测试与验证](#测试与验证)
8. [参考资源](#参考资源)

---

## AG-UI 简介

### 什么是 AG-UI

AG-UI (Agent UI Protocol) 是一个开放标准协议，用于创建基于 Web 的 AI Agent 应用程序。它提供：

- **实时流式通信**: 客户端与 Agent 之间的实时消息流
- **标准化通信格式**: 统一的消息和状态管理协议
- **工具调用可视化**: 展示 Agent 使用的工具和调用过程
- **会话管理**: 线程（Thread）管理和上下文保持
- **自定义 UI 渲染**: 支持工具调用的自定义 UI 组件

### AG-UI 核心概念

```
┌─────────────┐     AG-UI Protocol    ┌─────────────┐
│   Client    │ ◄──────────────────► │   Server    │
│  (Browser)  │   WebSocket/SSE      │  (Backend)  │
└─────────────┘                       └─────────────┘
       │                                      │
       │                                      │
       ▼                                      ▼
  ┌──────────┐                         ┌──────────┐
  │ UI Layer │                         │  Agent   │
  └──────────┘                         │ Runtime  │
                                       └──────────┘
```

### 关键特性

1. **消息流式传输**
   - Agent 响应的实时流式传输
   - 逐字符或逐 Token 显示
   - 支持中断和取消

2. **状态同步**
   - 线程（Thread）状态管理
   - 消息历史同步
   - 工具调用状态追踪

3. **工具调用可视化**
   - 展示工具调用参数
   - 显示工具执行结果
   - 错误和警告提示

4. **交互式调试**
   - 查看 Agent 推理过程
   - 检查中间步骤
   - 重放执行历史

---

## 集成目标

### 业务目标

1. **开发者调试**: 提供直观的界面调试 Agent 行为
2. **功能演示**: 展示 Agent 能力和工作流程
3. **问题诊断**: 快速定位和解决 Agent 执行问题
4. **性能监控**: 实时查看 Agent 执行性能指标

### 技术目标

1. **实现 AG-UI 协议**: 完整支持 AG-UI 标准
2. **WebSocket 通信**: 建立双向实时通信通道
3. **流式响应**: 实现 Agent 响应的流式传输
4. **可视化界面**: 提供清晰的调试和监控界面

---

## 技术架构

### 整体架构

```
┌──────────────────────────────────────────────────────────────┐
│                       Angular Frontend                       │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐         │
│  │ Agent Chat  │  │ Tool Viewer │  │ Trace Panel │         │
│  └─────────────┘  └─────────────┘  └─────────────┘         │
└────────────────────────┬─────────────────────────────────────┘
                         │ WebSocket / SSE
                         ▼
┌──────────────────────────────────────────────────────────────┐
│                    ASP.NET Core Backend                      │
│  ┌──────────────────────────────────────────────────────┐   │
│  │              AG-UI Communication Layer               │   │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  │   │
│  │  │ WS Handler  │  │ SSE Handler │  │ HTTP API    │  │   │
│  │  └─────────────┘  └─────────────┘  └─────────────┘  │   │
│  └──────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────┐   │
│  │              Agent Execution Service                 │   │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  │   │
│  │  │ Agent Core  │  │ Tool Caller │  │ RAG Query   │  │   │
│  │  └─────────────┘  └─────────────┘  └─────────────┘  │   │
│  └──────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────┐   │
│  │                OpenTelemetry Tracing                 │   │
│  └──────────────────────────────────────────────────────┘   │
└──────────────────────────────────────────────────────────────┘
```

### 协议栈

| 层级 | 技术 | 说明 |
|------|------|------|
| 传输层 | WebSocket / SSE | 实时双向通信 |
| 协议层 | AG-UI Protocol | 标准化消息格式 |
| 应用层 | Agent API | 业务逻辑处理 |
| 持久层 | PostgreSQL + Redis | 数据存储和缓存 |

---

## 实施方案

### Phase 1: 后端基础设施 (Week 1)

#### 1.1 添加必要的 NuGet 包

```xml
<!-- Agent Framework 相关 -->
<PackageReference Include="Microsoft.Extensions.AI" Version="1.0.0-preview" />
<PackageReference Include="Microsoft.Extensions.AI.Abstractions" Version="1.0.0-preview" />

<!-- WebSocket 和流式传输 -->
<PackageReference Include="System.Threading.Channels" Version="8.0.0" />
```

#### 1.2 实现 AG-UI 通信层

**核心组件**:
- `AgentStreamingService`: 流式 Agent 执行服务
- `AgUiWebSocketHandler`: WebSocket 处理器
- `AgUiMessageFormatter`: 消息格式化器
- `ThreadManager`: 线程（会话）管理器

#### 1.3 集成 OpenTelemetry

- 配置分布式追踪
- 添加 Agent 执行指标
- 实现调用链追踪

### Phase 2: Agent 流式执行 (Week 2)

#### 2.1 改造 Agent 执行引擎

**当前问题**:
- `AgentExecutionService` 不支持流式响应
- 没有中间状态回调
- 缺少工具调用进度通知

**改进方案**:
```csharp
// 支持流式响应的 Agent 执行接口
public interface IStreamingAgentExecutor
{
    IAsyncEnumerable<AgentExecutionEvent> ExecuteStreamAsync(
        Guid agentId,
        string userMessage,
        Guid? threadId = null,
        CancellationToken cancellationToken = default
    );
}

// Agent 执行事件
public record AgentExecutionEvent
{
    public string Type { get; init; } // message_start, content_block, tool_call, message_end
    public string? Content { get; init; }
    public ToolCall? ToolCall { get; init; }
    public TokenUsage? Usage { get; init; }
}
```

#### 2.2 实现工具调用追踪

```csharp
public interface IToolCallTracker
{
    Task TrackToolCallStartAsync(string toolName, string arguments);
    Task TrackToolCallCompleteAsync(string toolName, string result);
    Task TrackToolCallErrorAsync(string toolName, string error);
}
```

### Phase 3: 前端界面 (Week 2-3)

#### 3.1 创建 AG-UI 组件

**核心组件**:
1. **AgentChatComponent**: Agent 对话界面
2. **ToolCallViewerComponent**: 工具调用可视化
3. **ExecutionTraceComponent**: 执行轨迹面板
4. **ThreadHistoryComponent**: 会话历史

#### 3.2 实现 WebSocket 客户端

```typescript
// Angular Service
@Injectable()
export class AgUiWebSocketService {
  private socket$: WebSocket;
  
  connect(threadId: string): Observable<AgUiMessage> {
    // WebSocket 连接逻辑
  }
  
  sendMessage(content: string): void {
    // 发送用户消息
  }
  
  disconnect(): void {
    // 关闭连接
  }
}
```

#### 3.3 实现流式消息渲染

- 逐字符显示 Agent 响应
- Markdown 实时渲染
- 代码块语法高亮
- 工具调用卡片展示

### Phase 4: OpenTelemetry 集成 (Week 3)

#### 4.1 配置追踪

```csharp
// Program.cs
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSource("AIAgent.*")
            .AddOtlpExporter();
    });
```

#### 4.2 添加自定义追踪

```csharp
using var activity = ActivitySource.StartActivity("AgentExecution");
activity?.SetTag("agent.id", agentId);
activity?.SetTag("agent.name", agentName);
activity?.SetTag("tool.name", toolName);
```

#### 4.3 集成仪表板

- Azure Application Insights
- Jaeger UI
- 自定义监控面板

---

## 后端实现

### 1. 创建 AG-UI 通信服务

**文件**: `src/Modules/AIAgentMod/Services/AgUiCommunicationService.cs`

```csharp
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
    private readonly Dictionary<string, WebSocket> _connections = new();
    private readonly Dictionary<string, Channel<AgUiMessage>> _messageChannels = new();

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

        // 启动消息发送任务
        var sendTask = SendMessagesAsync(webSocket, channel.Reader, cancellationToken);

        // 接收客户端消息
        var receiveTask = ReceiveMessagesAsync(webSocket, threadId, cancellationToken);

        await Task.WhenAny(sendTask, receiveTask);

        // 清理
        _connections.Remove(threadId);
        _messageChannels.Remove(threadId);
    }

    /// <summary>
    /// 发送消息到客户端
    /// </summary>
    public async Task SendMessageAsync(string threadId, AgUiMessage message)
    {
        if (_messageChannels.TryGetValue(threadId, out var channel))
        {
            await channel.Writer.WriteAsync(message);
        }
    }

    private async Task SendMessagesAsync(
        WebSocket webSocket,
        ChannelReader<AgUiMessage> reader,
        CancellationToken cancellationToken)
    {
        await foreach (var message in reader.ReadAllAsync(cancellationToken))
        {
            var json = JsonSerializer.Serialize(message);
            var bytes = Encoding.UTF8.GetBytes(json);
            await webSocket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                endOfMessage: true,
                cancellationToken);
        }
    }

    private async Task ReceiveMessagesAsync(
        WebSocket webSocket,
        string threadId,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[4096];
        while (webSocket.State == WebSocketState.Open)
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

            // 处理接收到的消息
            var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
            _logger.LogInformation("Received message from thread {ThreadId}: {Message}", 
                threadId, json);
        }
    }
}

/// <summary>
/// AG-UI 消息格式
/// </summary>
public record AgUiMessage
{
    public string Type { get; init; } = string.Empty;
    public string? ThreadId { get; init; }
    public string? Content { get; init; }
    public AgUiToolCall? ToolCall { get; init; }
    public AgUiTokenUsage? Usage { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
}

public record AgUiToolCall
{
    public string Name { get; init; } = string.Empty;
    public string Arguments { get; init; } = string.Empty;
    public string? Result { get; init; }
    public string? Error { get; init; }
}

public record AgUiTokenUsage
{
    public int PromptTokens { get; init; }
    public int CompletionTokens { get; init; }
    public int TotalTokens { get; init; }
}
```

### 2. 创建流式 Agent 执行器

**文件**: `src/Modules/AIAgentMod/Services/StreamingAgentExecutor.cs`

```csharp
namespace AIAgentMod.Services;

/// <summary>
/// 流式 Agent 执行器，支持实时响应和工具调用追踪
/// </summary>
public class StreamingAgentExecutor : IStreamingAgentExecutor
{
    private readonly IAgentExecutionService _executionService;
    private readonly AgUiCommunicationService _communicationService;
    private readonly ILogger<StreamingAgentExecutor> _logger;

    public StreamingAgentExecutor(
        IAgentExecutionService executionService,
        AgUiCommunicationService communicationService,
        ILogger<StreamingAgentExecutor> logger)
    {
        _executionService = executionService;
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

        // 发送开始事件
        yield return new AgentExecutionEvent
        {
            Type = "message_start",
            ThreadId = effectiveThreadId
        };

        await _communicationService.SendMessageAsync(
            effectiveThreadId.ToString(),
            new AgUiMessage
            {
                Type = "message_start",
                ThreadId = effectiveThreadId.ToString()
            });

        // 执行 Agent（这里需要改造 AgentExecutionService 支持流式）
        // 目前简化为模拟流式响应
        var response = "这是一个模拟的流式响应示例。";
        
        foreach (var chunk in SplitIntoChunks(response, chunkSize: 5))
        {
            yield return new AgentExecutionEvent
            {
                Type = "content_block",
                Content = chunk,
                ThreadId = effectiveThreadId
            };

            await _communicationService.SendMessageAsync(
                effectiveThreadId.ToString(),
                new AgUiMessage
                {
                    Type = "content_block",
                    Content = chunk,
                    ThreadId = effectiveThreadId.ToString()
                });

            await Task.Delay(100, cancellationToken); // 模拟流式延迟
        }

        // 发送结束事件
        yield return new AgentExecutionEvent
        {
            Type = "message_end",
            ThreadId = effectiveThreadId,
            Usage = new AgUiTokenUsage
            {
                PromptTokens = 100,
                CompletionTokens = 50,
                TotalTokens = 150
            }
        };

        await _communicationService.SendMessageAsync(
            effectiveThreadId.ToString(),
            new AgUiMessage
            {
                Type = "message_end",
                ThreadId = effectiveThreadId.ToString(),
                Usage = new AgUiTokenUsage
                {
                    PromptTokens = 100,
                    CompletionTokens = 50,
                    TotalTokens = 150
                }
            });
    }

    private static IEnumerable<string> SplitIntoChunks(string text, int chunkSize)
    {
        for (int i = 0; i < text.Length; i += chunkSize)
        {
            yield return text.Substring(i, Math.Min(chunkSize, text.Length - i));
        }
    }
}

public interface IStreamingAgentExecutor
{
    IAsyncEnumerable<AgentExecutionEvent> ExecuteStreamAsync(
        Guid agentId,
        string userMessage,
        Guid? threadId = null,
        CancellationToken cancellationToken = default);
}

public record AgentExecutionEvent
{
    public string Type { get; init; } = string.Empty;
    public Guid? ThreadId { get; init; }
    public string? Content { get; init; }
    public AgUiToolCall? ToolCall { get; init; }
    public AgUiTokenUsage? Usage { get; init; }
}
```

### 3. 添加 WebSocket Controller

**文件**: `src/Services/ApiService/Controllers/AgUiController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using AIAgentMod.Services;

namespace ApiService.Controllers;

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
        }
    }

    /// <summary>
    /// HTTP 端点：发送消息到 Agent（用于测试）
    /// </summary>
    [HttpPost("message")]
    public async Task<IActionResult> SendMessage(
        [FromBody] AgentMessageRequest request,
        CancellationToken cancellationToken)
    {
        var events = new List<AgentExecutionEvent>();

        await foreach (var evt in _streamingExecutor.ExecuteStreamAsync(
            request.AgentId,
            request.Message,
            request.ThreadId,
            cancellationToken))
        {
            events.Add(evt);
        }

        return Ok(new { events });
    }
}

public record AgentMessageRequest
{
    public Guid AgentId { get; init; }
    public string Message { get; init; } = string.Empty;
    public Guid? ThreadId { get; init; }
}
```

### 4. 配置 WebSocket 中间件

**文件**: `src/Services/ApiService/Program.cs` (添加配置)

```csharp
// 在 Program.cs 中添加

// 启用 WebSocket
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
});

// 注册 AG-UI 服务
builder.Services.AddSingleton<AgUiCommunicationService>();
builder.Services.AddScoped<IStreamingAgentExecutor, StreamingAgentExecutor>();
```

---

## 前端实现

### 1. 创建 WebSocket 服务

**文件**: `src/ClientApp/WebApp/src/app/services/ag-ui-websocket.service.ts`

```typescript
import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';

export interface AgUiMessage {
  type: string;
  threadId?: string;
  content?: string;
  toolCall?: {
    name: string;
    arguments: string;
    result?: string;
    error?: string;
  };
  usage?: {
    promptTokens: number;
    completionTokens: number;
    totalTokens: number;
  };
  metadata?: Record<string, any>;
}

@Injectable({
  providedIn: 'root'
})
export class AgUiWebSocketService {
  private socket?: WebSocket;
  private messageSubject = new Subject<AgUiMessage>();

  connect(threadId: string): Observable<AgUiMessage> {
    const protocol = window.location.protocol === 'https:' ? 'wss:' : 'ws:';
    const wsUrl = `${protocol}//${window.location.host}/api/agui/ws/${threadId}`;

    this.socket = new WebSocket(wsUrl);

    this.socket.onopen = () => {
      console.log('WebSocket connected');
    };

    this.socket.onmessage = (event) => {
      const message: AgUiMessage = JSON.parse(event.data);
      this.messageSubject.next(message);
    };

    this.socket.onerror = (error) => {
      console.error('WebSocket error:', error);
    };

    this.socket.onclose = () => {
      console.log('WebSocket closed');
    };

    return this.messageSubject.asObservable();
  }

  sendMessage(content: string): void {
    if (this.socket && this.socket.readyState === WebSocket.OPEN) {
      this.socket.send(JSON.stringify({ type: 'user_message', content }));
    }
  }

  disconnect(): void {
    if (this.socket) {
      this.socket.close();
    }
  }
}
```

### 2. 创建 Agent 聊天组件

**文件**: `src/ClientApp/WebApp/src/app/components/agent-chat/agent-chat.component.ts`

```typescript
import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { AgUiWebSocketService, AgUiMessage } from '../../services/ag-ui-websocket.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-agent-chat',
  standalone: true,
  templateUrl: './agent-chat.component.html',
  styleUrls: ['./agent-chat.component.scss']
})
export class AgentChatComponent implements OnInit, OnDestroy {
  threadId = signal<string>(crypto.randomUUID());
  messages = signal<ChatMessage[]>([]);
  isConnected = signal<boolean>(false);
  currentResponse = signal<string>('');

  private subscription?: Subscription;

  constructor(private wsService: AgUiWebSocketService) {}

  ngOnInit(): void {
    this.connectWebSocket();
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
    this.wsService.disconnect();
  }

  private connectWebSocket(): void {
    this.subscription = this.wsService.connect(this.threadId()).subscribe({
      next: (message: AgUiMessage) => {
        this.handleMessage(message);
      },
      error: (error) => {
        console.error('WebSocket error:', error);
        this.isConnected.set(false);
      }
    });
    this.isConnected.set(true);
  }

  private handleMessage(message: AgUiMessage): void {
    switch (message.type) {
      case 'message_start':
        this.currentResponse.set('');
        break;
      case 'content_block':
        this.currentResponse.update(current => current + (message.content || ''));
        break;
      case 'message_end':
        this.messages.update(msgs => [
          ...msgs,
          {
            role: 'assistant',
            content: this.currentResponse(),
            usage: message.usage
          }
        ]);
        this.currentResponse.set('');
        break;
      case 'tool_call':
        // 处理工具调用
        console.log('Tool called:', message.toolCall);
        break;
    }
  }

  sendMessage(content: string): void {
    if (!content.trim()) return;

    // 添加用户消息到界面
    this.messages.update(msgs => [...msgs, { role: 'user', content }]);

    // 通过 WebSocket 发送
    this.wsService.sendMessage(content);
  }
}

interface ChatMessage {
  role: 'user' | 'assistant';
  content: string;
  usage?: {
    promptTokens: number;
    completionTokens: number;
    totalTokens: number;
  };
}
```

### 3. 创建聊天界面模板

**文件**: `src/ClientApp/WebApp/src/app/components/agent-chat/agent-chat.component.html`

```html
<div class="agent-chat-container">
  <div class="chat-header">
    <h2>Agent 调试界面</h2>
    <span [class.connected]="isConnected()" [class.disconnected]="!isConnected()">
      {{ isConnected() ? '已连接' : '未连接' }}
    </span>
  </div>

  <div class="messages-container">
    @for (message of messages(); track $index) {
      <div [class]="'message ' + message.role">
        <div class="message-role">
          {{ message.role === 'user' ? '用户' : 'Agent' }}
        </div>
        <div class="message-content" [innerHTML]="message.content | markdown"></div>
        @if (message.usage) {
          <div class="message-usage">
            Tokens: {{ message.usage.totalTokens }} 
            (Prompt: {{ message.usage.promptTokens }}, 
             Completion: {{ message.usage.completionTokens }})
          </div>
        }
      </div>
    }

    @if (currentResponse()) {
      <div class="message assistant streaming">
        <div class="message-role">Agent</div>
        <div class="message-content">{{ currentResponse() }}<span class="cursor">|</span></div>
      </div>
    }
  </div>

  <div class="input-container">
    <textarea 
      #messageInput
      placeholder="输入消息..." 
      (keydown.enter)="sendMessage(messageInput.value); messageInput.value = ''">
    </textarea>
    <button (click)="sendMessage(messageInput.value); messageInput.value = ''">
      发送
    </button>
  </div>
</div>
```

---

## 测试与验证

### 1. 单元测试

```csharp
[Fact]
public async Task StreamingAgentExecutor_Should_Produce_Events()
{
    // Arrange
    var executor = new StreamingAgentExecutor(/* dependencies */);
    var agentId = Guid.NewGuid();
    var message = "Hello, Agent!";

    // Act
    var events = new List<AgentExecutionEvent>();
    await foreach (var evt in executor.ExecuteStreamAsync(agentId, message))
    {
        events.Add(evt);
    }

    // Assert
    Assert.Contains(events, e => e.Type == "message_start");
    Assert.Contains(events, e => e.Type == "content_block");
    Assert.Contains(events, e => e.Type == "message_end");
}
```

### 2. 集成测试

- 启动 Aspire AppHost
- 打开浏览器访问 Agent 调试界面
- 发送测试消息
- 验证流式响应
- 检查工具调用可视化

### 3. 性能测试

- 并发连接数测试
- 消息吞吐量测试
- 延迟测试

---

## 参考资源

### 官方文档

1. **Microsoft Agent Framework**
   - GitHub: https://github.com/microsoft/agent-framework
   - 文档: https://learn.microsoft.com/en-us/agent-framework/

2. **AG-UI Protocol**
   - 规范: https://learn.microsoft.com/en-us/agent-framework/integrations/ag-ui/

3. **AutoGen Studio**
   - 文档: https://microsoft.github.io/autogen/stable/user-guide/autogenstudio-user-guide/

### 开源项目参考

1. **CopilotKit**: AG-UI UI 组件库
   - https://github.com/CopilotKit/CopilotKit

2. **AG-UI Dojo**: 示例应用
   - https://github.com/microsoft/ag-ui-dojo

### 技术博客

1. **Empowering Multi-Agent Solutions with Microsoft Agent Framework**
   - https://argonsys.com/microsoft-cloud/library/empowering-multi-agent-solutions-with-microsoft-agent-framework-code-migration-and-devui/

---

## 下一步行动

1. ✅ 完成 AG-UI 集成方案文档
2. ⬜ 实现后端 WebSocket 通信层
3. ⬜ 改造 Agent 执行器支持流式响应
4. ⬜ 创建前端调试界面
5. ⬜ 集成 OpenTelemetry 追踪
6. ⬜ 编写测试用例
7. ⬜ 更新用户文档

---

**文档维护**:
- 由开发团队共同维护
- 遇到问题或改进建议请提交 Issue
- 实施过程中持续更新本文档
