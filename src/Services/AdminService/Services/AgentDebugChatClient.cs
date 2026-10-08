using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Channels;
using AGUI.Abstractions;
using AGUI.Server;
using AIAgentMod.Models.AgentDebugDtos;
using AIAgentMod.Services;
using Microsoft.Extensions.AI;

namespace AdminService.Services;

/// <summary>Adapts the existing scoped debug business pipeline to the official AG-UI host.</summary>
public sealed class AgentDebugChatClient(IHttpContextAccessor contextAccessor) : IChatClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<Microsoft.Extensions.AI.ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (options is null || !options.TryGetRunAgentInput(out var input))
            throw new InvalidOperationException("AG-UI run input is required.");

        var request = JsonSerializer.Deserialize<AgentDebugRequest>(
            JsonSerializer.Serialize(input.ForwardedProperties), JsonOptions)
            ?? throw new InvalidOperationException("Debug configuration is required.");
        request.RequestId = input.RunId;
        // RunAgentInput messages are canonical; configuration cannot replace the user's message.
        request.UserMessage = messages.LastOrDefault(m => m.Role == ChatRole.User)?.Text ?? string.Empty;

        var context = contextAccessor.HttpContext
            ?? throw new InvalidOperationException("An authenticated HTTP request is required.");
        var service = context.RequestServices.GetRequiredService<AgentDebugService>();
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, context.RequestAborted);
        var channel = Channel.CreateBounded<ChatResponseUpdate>(new BoundedChannelOptions(16)
        {
            SingleReader = true, SingleWriter = true, FullMode = BoundedChannelFullMode.Wait
        });
        var producer = ProduceAsync();
        try
        {
            await foreach (var update in channel.Reader.ReadAllAsync(linked.Token))
                yield return update;
            await producer;
        }
        finally
        {
            await linked.CancelAsync();
            await producer;
        }

        async Task ProduceAsync()
        {
            try
            {
                await service.ExecuteStreamAsync(request, async evt =>
                {
                    foreach (var update in ConvertEvent(evt, request.EnableToolCallLogging))
                        await channel.Writer.WriteAsync(update, linked.Token);
                }, linked.Token);
                channel.Writer.TryComplete();
            }
            catch (Exception ex)
            {
                channel.Writer.TryComplete(ex);
            }
        }
    }

    public static IEnumerable<ChatResponseUpdate> ConvertEvent(AgentDebugStreamEvent evt, bool logTools)
    {
        if (evt.Type == "message" && evt.Message is { } message)
        {
            if (message.Role == "assistant")
                yield return new ChatResponseUpdate(ChatRole.Assistant, message.Content)
                { MessageId = Guid.NewGuid().ToString("N") };
            else
                yield return Custom("debug.message", message);
        }
        else if (evt.Type == "tool" && evt.ToolCall is { } tool && logTools)
        {
            var callId = Guid.NewGuid().ToString("N");
            var arguments = tool.Input is string json
                ? JsonSerializer.Deserialize<Dictionary<string, object?>>(json) ?? []
                : JsonSerializer.Deserialize<Dictionary<string, object?>>(JsonSerializer.Serialize(tool.Input)) ?? [];
            yield return new ChatResponseUpdate(ChatRole.Assistant, new List<AIContent>
            { new FunctionCallContent(callId, tool.Name, arguments) });
            yield return new ChatResponseUpdate(ChatRole.Tool, new List<AIContent>
            { new FunctionResultContent(callId, tool.Output) });
        }
        else if (evt.Type == "done" && evt.Metrics is { } metrics)
        {
            yield return Custom("debug.metrics", metrics);
            yield return new ChatResponseUpdate { FinishReason = ChatFinishReason.Stop };
        }
        else if (evt.Type == "error")
        {
            yield return new ChatResponseUpdate
            { RawRepresentation = new RunErrorEvent { Message = evt.Error ?? "Agent execution failed", Code = "AGENT_DEBUG_ERROR" } };
        }
    }

    private static ChatResponseUpdate Custom(string name, object value) => new()
    {
        RawRepresentation = new CustomEvent { Name = name, Value = JsonSerializer.SerializeToElement(value, JsonOptions) }
    };

    public Task<ChatResponse> GetResponseAsync(IEnumerable<Microsoft.Extensions.AI.ChatMessage> messages,
        ChatOptions? options = null, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Agent debugging uses AG-UI streaming.");

    public object? GetService(Type serviceType, object? serviceKey = null) =>
        serviceKey is null && serviceType.IsInstanceOfType(this) ? this : null;
    public void Dispose() { }
}
