namespace CoreMod.Models.RagQuery;

public sealed class RagQueryItem
{
    public Guid DocumentId { get; set; }

    public string Content { get; set; } = string.Empty;

    public double Score { get; set; }
}