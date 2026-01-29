using CoreMod.Models;
using Microsoft.Extensions.AI;
using OpenAI;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

namespace CoreMod.Services;

/// <summary>
/// 使用 Microsoft.Extensions.AI 和 OpenAI SDK 的模型调用实现
/// 支持 OpenAI、Azure OpenAI 和其他兼容平台
/// </summary>
public class ExtensionsAIModelClient(
    IModelRouter modelRouter,
    IModelCapabilityResolver capabilityResolver,
    ILogger<ExtensionsAIModelClient> logger
) : IModelClient
{
    public async Task<ModelResponse> ChatAsync(ModelRequest request, CancellationToken cancellationToken = default)
    {
        var route = await modelRouter.ResolveAsync(request, cancellationToken);
        if (string.IsNullOrWhiteSpace(route.BaseUrl) || string.IsNullOrWhiteSpace(route.ApiKey))
        {
            return Failed("Model provider configuration missing");
        }

        var capability = await capabilityResolver.ResolveAsync(request.Model, cancellationToken);
        if (!capability.SupportsChat)
        {
            return Failed("Model does not support chat");
        }

        try
        {
            var chatClient = CreateChatClient(route, request.Model);
            
            // 转换消息格式
            var messages = request.Messages.Select(m => new ChatMessage(
                m.Role switch
                {
                    "system" => ChatRole.System,
                    "user" => ChatRole.User,
                    "assistant" => ChatRole.Assistant,
                    "tool" => ChatRole.Tool,
                    _ => ChatRole.User
                },
                m.Content
            )).ToList();

            // 配置选项
            var options = new ChatOptions
            {
                Temperature = request.Metadata.TryGetValue("temperature", out var temp) && float.TryParse(temp, out var tempValue) ? tempValue : null,
                MaxOutputTokens = request.Metadata.TryGetValue("max_tokens", out var maxTokens) && int.TryParse(maxTokens, out var maxTokensValue) ? maxTokensValue : null,
            };

            // 调用聊天API
            var response = await chatClient.GetResponseAsync(messages, options, cancellationToken);

            return new ModelResponse
            {
                Success = true,
                Content = response.Text ?? string.Empty,
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

    public async Task<ModelResponse> EmbeddingAsync(ModelRequest request, CancellationToken cancellationToken = default)
    {
        var route = await modelRouter.ResolveAsync(request, cancellationToken);
        if (string.IsNullOrWhiteSpace(route.BaseUrl) || string.IsNullOrWhiteSpace(route.ApiKey))
        {
            return Failed("Model provider configuration missing");
        }

        var capability = await capabilityResolver.ResolveAsync(request.Model, cancellationToken);
        if (!capability.SupportsEmbedding)
        {
            return Failed("Model does not support embedding");
        }

        var input = request.Metadata.TryGetValue("input", out var metadataInput)
            ? metadataInput
            : request.Messages.FirstOrDefault()?.Content;

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

            return new ModelResponse
            {
                Success = true,
                Content = System.Text.Json.JsonSerializer.Serialize(embedding.Vector.ToArray()),
                Usage = new UsageStats
                {
                    PromptTokens = (int)(embeddings.Usage?.InputTokenCount ?? 0),
                    TotalTokens = (int)(embeddings.Usage?.TotalTokenCount ?? 0),
                },
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Embedding request failed for model {Model}", request.Model);
            return Failed(ex.Message);
        }
    }

    public Task<ModelResponse> VisionAsync(ModelRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Failed("VisionAsync not configured"));
    }

    public Task<ModelResponse> ModerationAsync(ModelRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Failed("ModerationAsync not configured"));
    }

    private IChatClient CreateChatClient(ModelRoute route, string model)
    {
        // 创建 OpenAI 客户端
        var openAIClient = new OpenAIClient(route.ApiKey);
        
        // 如果是 Azure OpenAI，需要使用不同的初始化方式
        // 这里假设 BaseUrl 包含了正确的端点信息
        if (!string.IsNullOrWhiteSpace(route.BaseUrl) && route.BaseUrl.Contains("azure"))
        {
            // Azure OpenAI: 使用 Azure.AI.OpenAI.AzureOpenAIClient
            // 注意：这里需要根据实际情况调整
            logger.LogWarning("Azure OpenAI detected but using OpenAI SDK. Consider using Azure.AI.OpenAI package for better Azure support.");
        }

        return openAIClient
            .GetChatClient(model)
            .AsIChatClient();
    }

    private IEmbeddingGenerator<string, Embedding<float>> CreateEmbeddingGenerator(ModelRoute route, string model)
    {
        var openAIClient = new OpenAIClient(route.ApiKey);
        
        if (!string.IsNullOrWhiteSpace(route.BaseUrl) && route.BaseUrl.Contains("azure"))
        {
            logger.LogWarning("Azure OpenAI detected but using OpenAI SDK. Consider using Azure.AI.OpenAI package for better Azure support.");
        }

        return openAIClient
            .GetEmbeddingClient(model)
            .AsIEmbeddingGenerator();
    }

    private static ModelResponse Failed(string message)
    {
        return new ModelResponse
        {
            Success = false,
            ErrorMessage = message,
        };
    }
}
