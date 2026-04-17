namespace Entity.AIAgentMod;

using System.ComponentModel;

/// <summary>
/// Agent 能力标志（按位组合）
/// </summary>
[Flags]
public enum AgentCapabilities
{
    None = 0,

    /// <summary>支持 function calling / tools</summary>
    Tools = 1 << 0,

    /// <summary>支持流式响应</summary>
    Streaming = 1 << 1,

    /// <summary>支持结构化输出 (ResponseFormat.Json)</summary>
    StructuredOutput = 1 << 2,

    /// <summary>支持多模态（图片/音频/文件）</summary>
    Multimodal = 1 << 3,

    /// <summary>支持向其他 Agent 进行 Handoff</summary>
    Handoff = 1 << 4,

    /// <summary>支持 Human-in-the-loop 审批</summary>
    HumanInTheLoop = 1 << 5,

    /// <summary>支持 RAG 知识检索</summary>
    Rag = 1 << 6,

    /// <summary>支持 MCP 工具</summary>
    Mcp = 1 << 7,
}

/// <summary>
/// Agent 记忆模式
/// </summary>
public enum AgentMemoryMode
{
    /// <summary>不保留历史，每次调用完全无状态</summary>
    [Description("None")]
    None = 0,

    /// <summary>滑动窗口：保留最近 N 条消息</summary>
    [Description("Window")]
    Window = 1,

    /// <summary>滑动窗口 + 旧消息摘要</summary>
    [Description("Summary")]
    Summary = 2,
}
