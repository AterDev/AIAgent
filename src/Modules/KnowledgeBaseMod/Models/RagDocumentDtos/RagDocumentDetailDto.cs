namespace KnowledgeBaseMod.Models.RagDocumentDtos;

/// <summary>
/// 文档 DetailDto
/// </summary>
/// <see cref="Entity.KnowledgeBaseMod.RagDocument"/>
public class RagDocumentDetailDto
{
    public Guid Id { get; set; }
    public Guid CollectionId { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
    public DateTimeOffset UpdatedTime { get; set; }
    public Guid TenantId { get; set; }

    [MaxLength(200)]
    public string? Name { get; set; }

    [MaxLength(260)]
    public string? FileName { get; set; }

    [MaxLength(500)]
    public string? FilePath { get; set; }

    [MaxLength(100)]
    public string? ContentType { get; set; }

    public StorageType StorageType { get; set; }

    public RagDocumentStatus Status { get; set; }

    public List<string>? Tags { get; set; }

    public List<string>? Roles { get; set; }

    [MaxLength(500)]
    public string? SourceUrl { get; set; }

    public int ChunkCount { get; set; }

    public int TokenCount { get; set; }

    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }
}
