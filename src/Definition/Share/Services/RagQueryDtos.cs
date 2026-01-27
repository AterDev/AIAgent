namespace Share.Services;

public sealed class RagQueryRequest
{
    public required string Query { get; set; }

    public Guid? CollectionId { get; set; }

    public int TopK { get; set; } = 5;
}

public sealed class RagQueryItem
{
    public Guid DocumentId { get; set; }

    public string Content { get; set; } = string.Empty;

    public double Score { get; set; }
}

public sealed class RagQueryResult
{
    public List<RagQueryItem> Items { get; set; } = [];
}
