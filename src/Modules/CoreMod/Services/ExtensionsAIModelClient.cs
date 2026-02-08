using CoreMod.Models;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;
using System.Text.Json;

namespace CoreMod.Services;

/// <summary>
/// OpenAI 兼容模型调用 - 支持 OpenAI、DeepSeek、Azure OpenAI 等所有 OpenAI 协议兼容的服务
/// 使用 Microsoft.Extensions.AI 统一规范，通过 OpenAI SDK 的 OpenAIClientOptions 配置自定义 BaseUrl
/// </summary>
public class ExtensionsAIModelClient(
    DbModelRouter modelRouter,
    ILogger<ExtensionsAIModelClient> logger
)
{
    public async Task<ModelResponse> ChatAsync(ModelRequest request, CancellationToken cancellationToken = default)
    {
        var route = await modelRouter.ResolveAsync(request, cancellationToken);
        if (string.IsNullOrWhiteSpace(route.BaseUrl) || string.IsNullOrWhiteSpace(route.ApiKey))
        {
            return Failed("Model provider configuration missing");
        }

        try
        {
            var chatClient = CreateChatClient(route, request.Model);
            var messages = BuildChatMessages(request.Messages);
            var options = BuildChatOptions(request);

            var response = await chatClient.GetResponseAsync(messages, options, cancellationToken);

            // Extract tool calls from response
            var toolCalls = ExtractToolCalls(response);
            var textContent = response.Text ?? string.Empty;

            return new ModelResponse
            {
                Success = true,
                Content = textContent,
                ToolCalls = toolCalls,
                Usage = new UsageStats
                {
                    PromptTokens = (int)(response.Usage?.InputTokenCount ?? 0),
                    CompletionTokens = (int)(response.Usage?.OutputTokenCount ?? 0),
                    TotalTokens = (int)(response.Usage?.TotalTokenCount ?? 0),
                },
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Chat request failed for model {Model}", request.Model);
            return Failed(ex.Message);
        }
    }

    public async IAsyncEnumerable<ModelStreamChunk> StreamChatAsync(
        ModelRequest request,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var route = await modelRouter.ResolveAsync(request, cancellationToken);
        if (string.IsNullOrWhiteSpace(route.BaseUrl) || string.IsNullOrWhiteSpace(route.ApiKey))
        {
            yield return new ModelStreamChunk { ErrorMessage = "Model provider configuration missing", IsFinal = true };
            yield break;
        }

        IChatClient? chatClient = null;
        Exception? createClientError = null;
        try
        {
            chatClient = CreateChatClient(route, request.Model);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Stream chat client creation failed");
            createClientError = ex;
        }

        if (createClientError != null || chatClient == null)
        {
            yield return new ModelStreamChunk { ErrorMessage = createClientError?.Message ?? "Chat client creation failed", IsFinal = true };
            yield break;
        }

        var messages = BuildChatMessages(request.Messages);
        var options = BuildChatOptions(request);

        await foreach (var update in chatClient.GetStreamingResponseAsync(messages, options, cancellationToken))
        {
            if (!string.IsNullOrEmpty(update.Text))
            {
                yield return new ModelStreamChunk { Delta = update.Text };
            }
        }

        yield return new ModelStreamChunk { IsFinal = true };
    }

    public async Task<ModelResponse> EmbeddingAsync(ModelRequest request, CancellationToken cancellationToken = default)
    {
        var route = await modelRouter.ResolveAsync(request, cancellationToken);
        if (string.IsNullOrWhiteSpace(route.BaseUrl) || string.IsNullOrWhiteSpace(route.ApiKey))
        {
            return Failed("Model provider configuration missing");
        }

        var input = request.Metadata.TryGetValue("input", out var metadataInput) ? metadataInput : request.Messages.FirstOrDefault()?.Content;
        if (string.IsNullOrWhiteSpace(input))
        {
            return Failed("Embedding input missing");
        }

        try
        {
            var embeddingGenerator = CreateEmbeddingGenerator(route, request.Model);
            var embeddings = await embeddingGenerator.GenerateAsync([input], cancellationToken: cancellationToken);
            var embedding = embeddings.FirstOrDefault();

            if (embedding == null)
            {
                return Failed("Failed to generate embedding");
            }

            var usage = embeddings.Usage;
            return new ModelResponse
            {
                Success = true,
                Content = JsonSerializer.Serialize(embedding.Vector.ToArray()),
                Usage = new UsageStats
                {
                    PromptTokens = (int)(usage?.InputTokenCount ?? 0),
                    CompletionTokens = (int)(usage?.OutputTokenCount ?? 0),
                    TotalTokens = (int)(usage?.TotalTokenCount ?? 0),
                },
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Embedding failed");
            return Failed(ex.Message);
        }
    }

    public Task<ModelResponse> VisionAsync(ModelRequest request, CancellationToken cancellationToken = default) => Task.FromResult(Failed("VisionAsync not configured"));
    public Task<ModelResponse> ModerationAsync(ModelRequest request, CancellationToken cancellationToken = default) => Task.FromResult(Failed("ModerationAsync not configured"));

    private static List<ChatMessage> BuildChatMessages(List<ModelMessage> messages)
    {
        var chatMessages = new List<ChatMessage>();
        foreach (var m in messages)
        {
            var role = MapRole(m.Role);
            if (role == ChatRole.Tool && !string.IsNullOrWhiteSpace(m.ToolCallId))
            {
                // Tool result message with call ID
                chatMessages.Add(new ChatMessage(ChatRole.Tool, [new FunctionResultContent(m.ToolCallId, m.Content)]));
            }
            else
            {
                chatMessages.Add(new ChatMessage(role, m.Content));
            }
        }

        return chatMessages;
    }

    private static ChatOptions BuildChatOptions(ModelRequest request)
    {
        var options = new ChatOptions();

        if (request.Metadata.TryGetValue("temperature", out var tempStr) && float.TryParse(tempStr, out var temp))
        {
            options.Temperature = temp;
        }

        if (request.Metadata.TryGetValue("max_tokens", out var maxTokensStr) && int.TryParse(maxTokensStr, out var maxTokens))
        {
            options.MaxOutputTokens = maxTokens;
        }

        // Add tool definitions if available
        if (request.ToolDefinitions.Count > 0)
        {
            options.Tools = request.ToolDefinitions
                .Select(BuildToolDefinition)
                .ToList();
        }

        return options;
    }

    private static List<ToolCall> ExtractToolCalls(ChatResponse response)
    {
        var toolCalls = new List<ToolCall>();
        foreach (var message in response.Messages)
        {
            foreach (var content in message.Contents)
            {
                if (content is FunctionCallContent functionCall)
                {
                    var argsJson = functionCall.Arguments != null
                        ? JsonSerializer.Serialize(functionCall.Arguments)
                        : string.Empty;

                    toolCalls.Add(new ToolCall
                    {
                        Id = functionCall.CallId,
                        Name = functionCall.Name,
                        ArgumentsJson = argsJson,
                    });
                }
            }
        }

        return toolCalls;
    }

    private static AITool BuildToolDefinition(ModelToolDefinition tool)
    {
        var schema = TryParseToolParameters(tool.ParametersJson);
        if (schema.HasValue)
        {
            return AIFunctionFactory.CreateDeclaration(
                name: tool.Name,
                description: tool.Description ?? string.Empty,
                jsonSchema: schema.Value,
                returnJsonSchema: null
            );
        }

        return AIFunctionFactory.Create(
            method: (string input) => input,
            name: tool.Name,
            description: tool.Description ?? string.Empty
        );
    }

    private static JsonElement? TryParseToolParameters(string? parametersJson)
    {
        if (string.IsNullOrWhiteSpace(parametersJson))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(parametersJson);
            return doc.RootElement.Clone();
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private IChatClient CreateChatClient(ModelRoute route, string model) => CreateOpenAIClient(route).GetChatClient(model).AsIChatClient();
    private IEmbeddingGenerator<string, Embedding<float>> CreateEmbeddingGenerator(ModelRoute route, string model) =>
        CreateOpenAIClient(route).GetEmbeddingClient(model).AsIEmbeddingGenerator();

    private static ChatRole MapRole(string? role) => role?.ToLowerInvariant() switch
    {
        "system" => ChatRole.System,
        "assistant" => ChatRole.Assistant,
        "tool" => ChatRole.Tool,
        _ => ChatRole.User,
    };

    private OpenAIClient CreateOpenAIClient(ModelRoute route)
    {
        var credential = new ApiKeyCredential(route.ApiKey!);
        var options = new OpenAIClientOptions { Endpoint = new Uri(route.BaseUrl!) };
        return new OpenAIClient(credential, options);
    }

    private static ModelResponse Failed(string message) => new() { Success = false, ErrorMessage = message };
}
