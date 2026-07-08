using CoreMod.Options;
using Microsoft.Extensions.Options;

namespace CoreMod.Services.Embedding;

/// <summary>
/// 基于 CoreMod 模型调用的真实向量生成器。支持通过 QdrantOptions.EmbeddingModel 或
/// 方法参数 overrideModel 指定具体 embedding 模型（OpenAI / Foundry Local / 通义千问 等）。
/// </summary>
public class CoreModelEmbeddingGenerator(
    ExtensionsAIModelClient modelClient,
    IOptions<QdrantOptions> qdrantOptions,
    ILogger<CoreModelEmbeddingGenerator> logger
)
{
    private readonly QdrantOptions _options = qdrantOptions.Value;

    public Task<float[]> GenerateAsync(string text, int size, CancellationToken cancellationToken = default)
        => GenerateAsync(text, modelName: null, size, cancellationToken);

    public async Task<float[]> GenerateAsync(string text, string? modelName, int size, CancellationToken cancellationToken = default)
    {
        var effectiveSize = size > 0 ? size : _options.VectorSize;
        if (string.IsNullOrWhiteSpace(text))
        {
            return new float[effectiveSize];
        }

        var model = string.IsNullOrWhiteSpace(modelName) ? _options.EmbeddingModel : modelName;

        var request = new ModelRequest
        {
            Model = model,
            Metadata = new Dictionary<string, string>
            {
                ["input"] = text,
                ["dimensions"] = effectiveSize.ToString(System.Globalization.CultureInfo.InvariantCulture),
            },
        };

        var response = await modelClient.EmbeddingAsync(request, cancellationToken);

        if (!response.Success || string.IsNullOrWhiteSpace(response.Content))
        {
            logger.LogWarning("Embedding generation failed: {Error} (model={Model})", response.ErrorMessage, model);
            return new float[effectiveSize];
        }

        var embedding = System.Text.Json.JsonSerializer.Deserialize<float[]>(response.Content);

        if (embedding is null || embedding.Length == 0)
        {
            logger.LogWarning("Embedding deserialization returned null or empty array (model={Model})", model);
            return new float[effectiveSize];
        }

        if (size > 0 && embedding.Length != size)
        {
            logger.LogWarning(
                "Embedding size mismatch: expected {Expected}, got {Actual} (model={Model})",
                size,
                embedding.Length,
                model);
        }

        return embedding;
    }
}
