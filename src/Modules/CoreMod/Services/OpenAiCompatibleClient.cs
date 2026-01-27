using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CoreMod.Models;

namespace CoreMod.Services;

/// <summary>
/// OpenAI 兼容模型调用
/// </summary>
public class OpenAiCompatibleClient(
    IHttpClientFactory httpClientFactory,
    IModelRouter modelRouter,
    IModelCapabilityResolver capabilityResolver,
    ILogger<OpenAiCompatibleClient> logger
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

        var body = new
        {
            model = request.Model,
            messages = request.Messages.Select(m => new { role = m.Role, content = m.Content }).ToList(),
        };

        try
        {
            var client = CreateClient(route);
            using var response = await client.PostAsJsonAsync("/v1/chat/completions", body, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                return Failed(errorBody);
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            var root = doc.RootElement;

            var content = root.TryGetProperty("choices", out var choices) && choices.ValueKind == JsonValueKind.Array
                ? choices.EnumerateArray().FirstOrDefault().GetProperty("message").GetProperty("content").GetString()
                : null;

            var usage = ParseUsage(root);
            return new ModelResponse
            {
                Success = true,
                Content = content,
                Usage = usage,
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Chat request failed");
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

        var body = new
        {
            model = request.Model,
            input,
        };

        try
        {
            var client = CreateClient(route);
            using var response = await client.PostAsJsonAsync("/v1/embeddings", body, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                return Failed(errorBody);
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            var root = doc.RootElement;
            var embedding = root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array
                ? data.EnumerateArray().FirstOrDefault().GetProperty("embedding").GetRawText()
                : "[]";

            var usage = ParseUsage(root);
            return new ModelResponse
            {
                Success = true,
                Content = embedding,
                Usage = usage,
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Embedding request failed");
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

    private static UsageStats ParseUsage(JsonElement root)
    {
        if (root.TryGetProperty("usage", out var usageElement))
        {
            return new UsageStats
            {
                PromptTokens = usageElement.TryGetProperty("prompt_tokens", out var prompt) && prompt.TryGetInt32(out var pt) ? pt : 0,
                CompletionTokens = usageElement.TryGetProperty("completion_tokens", out var completion) && completion.TryGetInt32(out var ct) ? ct : 0,
                TotalTokens = usageElement.TryGetProperty("total_tokens", out var total) && total.TryGetInt32(out var tt) ? tt : 0,
            };
        }

        return new UsageStats();
    }

    private HttpClient CreateClient(ModelRoute route)
    {
        var client = httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(route.BaseUrl!.TrimEnd('/'));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", route.ApiKey);
        return client;
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
