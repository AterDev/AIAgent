namespace KnowledgeBaseMod.Models.RagDocumentDtos;

/// <summary>
/// 文档 AddDto
/// </summary>
/// <see cref="RagDocument"/>
public class RagDocumentAddDto
{
    public Guid? ApplicationId { get; set; }

    public Guid CollectionId { get; set; }

    [MaxLength(200)]
    public string Name { get; set; } = default!;

    [MaxLength(260)]
    public string? FileName { get; set; }

    [MaxLength(500)]
    public string? FilePath { get; set; }

    public RagDocumentStatus Status { get; set; }

    public List<string>? Tags { get; set; }

    public List<string>? Roles { get; set; }
}
