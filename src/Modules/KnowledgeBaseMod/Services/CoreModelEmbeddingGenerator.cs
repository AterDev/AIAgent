using CoreMod.Services;
using KnowledgeBaseMod.Services;

namespace KnowledgeBaseMod.Services;

/// <summary>
/// 基于 CoreMod 模型调用的真实向量生成器
/// </summary>
public class CoreModelEmbeddingGenerator(
    IModelClient modelClient,
    ILogger<CoreModelEmbeddingGenerator> logger
) : IEmbeddingGenerator
{
    private const string DefaultEmbeddingModel = "text-embedding-3-small";
    private const int DefaultVectorSize = 1536;

    public float[] Generate(string text, int size)
    {
        // 同步包装异步调用
        try
        {
            return GenerateAsync(text, size, CancellationToken.None).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to generate embedding for text: {Text}", text.Length > 100 ? text[..100] + "..." : text);
            // 返回零向量作为降级方案
            return Enumerable.Repeat(0f, size > 0 ? size : DefaultVectorSize).ToArray();
        }
    }

    private async Task<float[]> GenerateAsync(string text, int size, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Enumerable.Repeat(0f, size > 0 ? size : DefaultVectorSize).ToArray();
        }

        var request = new CoreMod.Models.ModelRequest
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

        // 解析 JSON 数组
        var embedding = System.Text.Json.JsonSerializer.Deserialize<float[]>(response.Content);
        
        if (embedding == null || embedding.Length == 0)
        {
            logger.LogWarning("Embedding deserialization returned null or empty array");
            return Enumerable.Repeat(0f, size > 0 ? size : DefaultVectorSize).ToArray();
        }

        // 如果需要调整大小（虽然通常不应该需要）
        if (size > 0 && embedding.Length != size)
        {
            logger.LogWarning("Embedding size mismatch: expected {Expected}, got {Actual}", size, embedding.Length);
            // 可以考虑截断或填充，但通常应该使用正确的模型
        }

        return embedding;
    }
}
