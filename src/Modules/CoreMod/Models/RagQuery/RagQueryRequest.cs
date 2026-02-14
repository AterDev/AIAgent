namespace CoreMod.Models.RagQuery;

public sealed class RagQueryRequest
{
    public required string Query { get; set; }

    public Guid? CollectionId { get; set; }

    public int TopK { get; set; } = 5;
}
