namespace Entity.KnowledgeBaseMod;

/// <summary>
/// 文档
/// </summary>
[Index(nameof(CollectionId), nameof(Name))]
public class RagDocument : EntityBase
{
    public Guid CollectionId { get; set; }

    [ForeignKey(nameof(CollectionId))]
    public RagCollection? Collection { get; set; }

    [MaxLength(200)]
    public required string Name { get; set; }

    [MaxLength(260)]
    public string FileName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string ContentType { get; set; } = string.Empty;

    public RagDocumentStatus Status { get; set; }

    public List<string> Tags { get; set; } = [];

    public List<string> Roles { get; set; } = [];

    [MaxLength(500)]
    public string? SourceUrl { get; set; }

    public int ChunkCount { get; set; }

    public int TokenCount { get; set; }

    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }
}
