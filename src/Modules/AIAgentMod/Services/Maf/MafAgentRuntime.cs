using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using AgentEntity = Entity.AIAgentMod.AIAgent;
using ChatMessageEntity = Entity.AIAgentMod.ChatMessage;

namespace AIAgentMod.Services.Maf;

/// <summary>
/// 基于 Microsoft Agent Framework 1.1 的 Agent 运行时封装。
/// 输入 <see cref="AgentEntity"/> 配置 + 可选 tool 列表，产出可直接运行的 <see cref="ChatClientAgent"/>。
/// 返回的 <see cref="MafAgentBundle"/> 包含 ChatClientAgent 以及每次 RunAsync 应传入的 ChatOptions
/// （Temperature/TopP/ResponseFormat 等），调用方可直接透传到 <c>agent.RunAsync(..., options: bundle.ChatOptions)</c>。
/// </summary>
public class MafAgentRuntime(
    ExtensionsAIModelClient modelClient,
    ILogger<MafAgentRuntime> logger
)
{
    /// <summary>
    /// 构造基于指定 AIAgent 配置的 ChatClientAgent。
    /// </summary>
    public async Task<MafAgentBundle> BuildAgentAsync(
        AgentEntity agent,
        IReadOnlyList<AITool>? tools = null,
        CancellationToken cancellationToken = default)
    {
        var (chatClient, _) = await modelClient.GetChatClientAsync(agent.ModelId, cancellationToken: cancellationToken);

        var chatOptions = BuildChatOptions(agent, tools);

        var chatAgent = new ChatClientAgent(
            chatClient,
            instructions: string.IsNullOrWhiteSpace(agent.SystemPrompt) ? null : agent.SystemPrompt,
            name: agent.Name,
            description: agent.Description,
            tools: tools is { Count: > 0 } ? [.. tools] : null);

        logger.LogDebug("Built MAF ChatClientAgent {AgentName} (model={Model}, tools={ToolCount})",
            agent.Name, agent.ModelId, tools?.Count ?? 0);

        return new MafAgentBundle(chatAgent, chatOptions);
    }

    /// <summary>
    /// 将会话历史裁剪成 MAF 可直接消费的 <see cref="Microsoft.Extensions.AI.ChatMessage"/> 列表，
    /// 按照 <see cref="AgentEntity.MemoryMode"/> 执行窗口/摘要策略。
    /// </summary>
    public IReadOnlyList<Microsoft.Extensions.AI.ChatMessage> PrepareHistory(AgentEntity agent, IReadOnlyList<ChatMessageEntity> history)
    {
        if (history.Count == 0)
        {
            return [];
        }

        var window = agent.ContextWindow <= 0 ? 20 : agent.ContextWindow;

        IEnumerable<ChatMessageEntity> chosen = agent.MemoryMode switch
        {
            AgentMemoryMode.None => [],
            AgentMemoryMode.Window => history.TakeLast(window),
            AgentMemoryMode.Summary => WithSummary(history, window),
            _ => history.TakeLast(window),
        };

        var result = new List<Microsoft.Extensions.AI.ChatMessage>();
        foreach (var msg in chosen)
        {
            var role = MapRole(msg.Role);
            var contents = BuildContents(msg);
            result.Add(new Microsoft.Extensions.AI.ChatMessage(role, contents));
        }
        return result;
    }

    private static ChatOptions BuildChatOptions(AgentEntity agent, IReadOnlyList<AITool>? tools)
    {
        var options = new ChatOptions();

        if (agent.Temperature.HasValue) options.Temperature = agent.Temperature;
        if (agent.TopP.HasValue) options.TopP = agent.TopP;
        if (agent.MaxOutputTokens.HasValue) options.MaxOutputTokens = agent.MaxOutputTokens;
        if (agent.FrequencyPenalty.HasValue) options.FrequencyPenalty = agent.FrequencyPenalty;
        if (agent.PresencePenalty.HasValue) options.PresencePenalty = agent.PresencePenalty;

        if (tools is { Count: > 0 })
        {
            options.Tools = [.. tools];
        }

        if (!string.IsNullOrWhiteSpace(agent.ResponseSchemaJson))
        {
            try
            {
                using var doc = JsonDocument.Parse(agent.ResponseSchemaJson);
                options.ResponseFormat = ChatResponseFormat.ForJsonSchema(
                    schema: doc.RootElement.Clone(),
                    schemaName: $"{agent.Name}_Response",
                    schemaDescription: $"Structured output for agent {agent.Name}");
            }
            catch (JsonException)
            {
                // 非法 schema 则降级为 free-form
            }
        }

        return options;
    }

    private static IEnumerable<ChatMessageEntity> WithSummary(
        IReadOnlyList<ChatMessageEntity> history,
        int window)
    {
        if (history.Count <= window)
        {
            return history;
        }
        var recent = history.TakeLast(window).ToList();
        var older = history.Take(history.Count - window).ToList();
        if (older.Count == 0)
        {
            return recent;
        }

        var summaryText = string.Join("\n", older.TakeLast(20).Select(m => $"[{m.Role}] {Truncate(m.Content, 200)}"));
        var pseudoSystem = new ChatMessageEntity
        {
            Role = ChatMessageRole.System,
            Content = $"[Conversation Summary]\n{summaryText}",
            ContentType = ChatMessageType.Text,
            ConversationId = history[0].ConversationId,
        };

        var result = new List<ChatMessageEntity> { pseudoSystem };
        result.AddRange(recent);
        return result;
    }

    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max] + "...";

    private static ChatRole MapRole(ChatMessageRole role) => role switch
    {
        ChatMessageRole.System => ChatRole.System,
        ChatMessageRole.AI => ChatRole.Assistant,
        ChatMessageRole.Tool => ChatRole.Tool,
        _ => ChatRole.User,
    };

    private static List<AIContent> BuildContents(ChatMessageEntity msg)
    {
        var contents = new List<AIContent>();
        if (!string.IsNullOrEmpty(msg.Content))
        {
            contents.Add(new TextContent(msg.Content));
        }

        if (msg.ContentType != ChatMessageType.Text
            && !string.IsNullOrWhiteSpace(msg.AttachmentUrl)
            && !msg.AttachmentUrl.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            var mime = msg.AttachmentMime ?? GuessMime(msg.ContentType);
            if (Uri.TryCreate(msg.AttachmentUrl, UriKind.Absolute, out var uri))
            {
                contents.Add(new UriContent(uri, mime));
            }
        }

        if (contents.Count == 0)
        {
            contents.Add(new TextContent(string.Empty));
        }

        return contents;
    }

    private static string GuessMime(ChatMessageType type) => type switch
    {
        ChatMessageType.Image => "image/png",
        ChatMessageType.File => "application/octet-stream",
        _ => "text/plain",
    };
}

/// <summary>
/// <see cref="MafAgentRuntime"/> 返回的 Agent + 默认 ChatOptions 组合。
/// </summary>
public sealed record MafAgentBundle(ChatClientAgent Agent, ChatOptions ChatOptions);

