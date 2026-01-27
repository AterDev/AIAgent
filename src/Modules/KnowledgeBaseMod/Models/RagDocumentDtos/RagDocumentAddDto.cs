namespace KnowledgeBaseMod.Models.RagDocumentDtos;

/// <summary>
/// 文档 AddDto
/// </summary>
/// <see cref="Entity.KnowledgeBaseMod.RagDocument"/>
public class RagDocumentAddDto
{
    public Guid CollectionId { get; set; }

    [MaxLength(200)]
    public string Name { get; set; } = default!;

    [MaxLength(260)]
    public string? FileName { get; set; }

    [MaxLength(100)]
    public string? ContentType { get; set; }

    public RagDocumentStatus Status { get; set; }

    public List<string>? Tags { get; set; }

    public List<string>? Roles { get; set; }

    [MaxLength(500)]
    public string? SourceUrl { get; set; }
}
