namespace CoreMod.Options;

public class QdrantOptions
{
    public string Url { get; set; } = string.Empty;

    public string? ApiKey { get; set; }

    public string CollectionName { get; set; } = "rag_chunks";

    public int VectorSize { get; set; } = 768;

    public string Distance { get; set; } = "Cosine";

    /// <summary>
    /// 默认 embedding 模型名（如 text-embedding-3-small / qwen3-embedding-0.6b）。
    /// 可被 SystemConfig 覆盖。
    /// </summary>
    public string EmbeddingModel { get; set; } = "text-embedding-3-small";
}
