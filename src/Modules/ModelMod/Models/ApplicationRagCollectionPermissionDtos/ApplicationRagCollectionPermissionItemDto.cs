namespace ModelMod.Models.ApplicationRagCollectionPermissionDtos;

/// <summary>
/// 应用知识库关联 ItemDto
/// </summary>
public class ApplicationRagCollectionPermissionItemDto
{
    public Guid Id { get; set; }

    public Guid ApplicationId { get; set; }

    public Guid RagCollectionId { get; set; }

    public bool IsEnabled { get; set; }
}