namespace CoreMod.Services.Embedding;

/// <summary>
/// 基于 CoreMod 模型调用的真实向量生成器
/// </summary>
public class CoreModelEmbeddingGenerator(
    ExtensionsAIModelClient modelClient,
    ILogger<CoreModelEmbeddingGenerator> logger
)
{
    private const string DefaultEmbeddingModel = "text-embedding-3-small";
    private const int DefaultVectorSize = 1536;

    public async Task<float[]> GenerateAsync(string text, int size, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Enumerable.Repeat(0f, size > 0 ? size : DefaultVectorSize).ToArray();
        }

        var request = new ModelRequest
        {
            Model = DefaultEmbeddingModel,
            Metadata = new Dictionary<string, string>
            {
                ["input"] = text
            }
        };

        var response = await modelClient.EmbeddingAsync(request, cancellationToken);

        if (!response.Success || string.IsNullOrWhiteSpace(response.Content))
        {
            logger.LogWarning("Embedding generation failed: {Error}", response.ErrorMessage);
            return Enumerable.Repeat(0f, size > 0 ? size : DefaultVectorSize).ToArray();
        }

        var embedding = System.Text.Json.JsonSerializer.Deserialize<float[]>(response.Content);

        if (embedding == null || embedding.Length == 0)
        {
            logger.LogWarning("Embedding deserialization returned null or empty array");
            return Enumerable.Repeat(0f, size > 0 ? size : DefaultVectorSize).ToArray();
        }

        if (size > 0 && embedding.Length != size)
        {
            logger.LogWarning("Embedding size mismatch: expected {Expected}, got {Actual}", size, embedding.Length);
        }

        return embedding;
    }
}