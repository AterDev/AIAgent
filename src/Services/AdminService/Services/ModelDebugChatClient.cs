using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using AGUI.Abstractions;
using AGUI.Server;
using Microsoft.Extensions.AI;
using ModelMod.Models.ModelDebugDtos;
using ModelMod.Services;

namespace AdminService.Services;

/// <summary>Preserves model permissions and usage accounting while the SDK owns AG-UI transport.</summary>
public sealed class ModelDebugChatClient(IHttpContextAccessor contextAccessor) : IChatClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<Microsoft.Extensions.AI.ChatMessage> messages, ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (options is null || !options.TryGetRunAgentInput(out var input))
            throw new InvalidOperationException("AG-UI run input is required.");
        var request = JsonSerializer.Deserialize<ModelDebugRequest>(
            JsonSerializer.Serialize(input.ForwardedProperties), JsonOptions)
            ?? throw new InvalidOperationException("Model configuration is required.");
        request.RequestId = input.RunId;
        request.Prompt = messages.LastOrDefault(m => m.Role == ChatRole.User)?.Text ?? string.Empty;
        var context = contextAccessor.HttpContext
            ?? throw new InvalidOperationException("An authenticated HTTP request is required.");
        var service = context.RequestServices.GetRequiredService<ModelDebugService>();
        var session = await service.CreateStreamSessionAsync(request, cancellationToken);
        await foreach (var update in StreamSessionAsync(session, cancellationToken))
            yield return update;
    }

    public static async IAsyncEnumerable<ChatResponseUpdate> StreamSessionAsync(
        ModelDebugStreamSession session,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var text = new StringBuilder();
        var usage = new CoreMod.Models.UsageStats();
        var messageId = Guid.NewGuid().ToString("N");
        await foreach (var chunk in session.Stream.WithCancellation(cancellationToken))
        {
            if (!string.IsNullOrWhiteSpace(chunk.ErrorMessage))
            {
                yield return new ChatResponseUpdate { RawRepresentation = new RunErrorEvent
                    { Message = chunk.ErrorMessage, Code = "MODEL_DEBUG_ERROR" } };
                yield break;
            }
            if (chunk.Usage is not null)
                usage = new() { PromptTokens = chunk.Usage.PromptTokens,
                    CompletionTokens = chunk.Usage.CompletionTokens, TotalTokens = chunk.Usage.TotalTokens };
            if (!string.IsNullOrEmpty(chunk.Delta))
            {
                text.Append(chunk.Delta);
                yield return new ChatResponseUpdate(ChatRole.Assistant, chunk.Delta) { MessageId = messageId };
            }
            if (chunk.IsFinal) break;
        }
        var result = new ModelDebugResponse
        {
            Content = text.ToString(), Model = session.ModelName, PromptTokens = usage.PromptTokens,
            CompletionTokens = usage.CompletionTokens, TotalTokens = usage.TotalTokens,
            FinishReason = "stop", DurationMs = (int)stopwatch.ElapsedMilliseconds
        };
        yield return new ChatResponseUpdate { RawRepresentation = new CustomEvent
            { Name = "model.metrics", Value = JsonSerializer.SerializeToElement(result, JsonOptions) } };
        yield return new ChatResponseUpdate { FinishReason = ChatFinishReason.Stop };
    }

    public Task<ChatResponse> GetResponseAsync(IEnumerable<Microsoft.Extensions.AI.ChatMessage> messages,
        ChatOptions? options = null, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Model debugging uses AG-UI streaming.");
    public object? GetService(Type serviceType, object? serviceKey = null) =>
        serviceKey is null && serviceType.IsInstanceOfType(this) ? this : null;
    public void Dispose() { }
}
