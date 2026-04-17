# Agents

## 构造

```csharp
var (chatClient, _) = await extensionsAIModelClient.GetChatClientAsync("deepseek-chat");
var agent = new ChatClientAgent(chatClient, new ChatClientAgentOptions
{
    Name = "翻译助手",
    Instructions = "你是一位专业翻译。",
    ChatOptions = new ChatOptions { Temperature = 0.3f, Tools = [ragTool] }
});

AgentThread thread = agent.GetNewThread();
var resp = await agent.RunAsync("Hello", thread);
```

## 通过本仓库 `MafAgentRuntime` 一步到位

```csharp
var mafAgent = await mafAgentRuntime.BuildAgentAsync(aiAgentEntity, tools: [ragTool]);
var thread = mafAgent.GetNewThread();
var reply = await mafAgent.RunAsync("用户输入", thread);
```

`MafAgentRuntime.BuildAgentAsync` 会：
- 调 `ExtensionsAIModelClient.GetChatClientAsync` 根据 `AIAgent.ModelId` 自动选 provider
- 把 `Temperature / TopP / MaxOutputTokens / FrequencyPenalty / PresencePenalty` 写入 `ChatOptions`
- 合并 `ResponseSchemaJson` → `ChatResponseFormat.ForJsonSchema`
- 添加 `tools` 列表

## 历史裁剪

通过 `MafAgentRuntime.PrepareHistory(agent, conversationMessages)` 得到 `IReadOnlyList<ChatMessage>`：
- `MemoryMode.None` → 空列表
- `MemoryMode.Window` → 末 N 条（N = `ContextWindow`）
- `MemoryMode.Summary` → 末 N 条 + 更旧消息的简易占位摘要（TODO：替换为 LLM 摘要）

将其作为 `RunAsync` 的 `chatHistory` 参数（或先添加到 `AgentThread`）。
