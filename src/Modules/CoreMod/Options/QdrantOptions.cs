namespace CoreMod.Options;

public class QdrantOptions
{
    public string Url { get; set; } = string.Empty;

    public string? ApiKey { get; set; }

    public string CollectionName { get; set; } = "rag_chunks";

    public int VectorSize { get; set; } = 256;

    public string Distance { get; set; } = "Cosine";
}