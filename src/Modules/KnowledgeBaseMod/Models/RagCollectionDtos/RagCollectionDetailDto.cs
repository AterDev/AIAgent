namespace KnowledgeBaseMod.Models.RagCollectionDtos;

/// <summary>
/// 知识库 DetailDto
/// </summary>
/// <see cref="RagCollection"/>
public class RagCollectionDetailDto
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
    public DateTimeOffset UpdatedTime { get; set; }
    public Guid TenantId { get; set; }

    [MaxLength(200)]
    public string? Name { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public bool IsPublic { get; set; }

    public bool IsEnabled { get; set; }

    public List<string>? Tags { get; set; }
}
