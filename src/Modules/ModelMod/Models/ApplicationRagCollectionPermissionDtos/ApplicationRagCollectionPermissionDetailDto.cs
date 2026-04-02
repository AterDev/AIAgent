namespace ModelMod.Models.ApplicationRagCollectionPermissionDtos;

/// <summary>
/// 应用知识库关联 DetailDto
/// </summary>
public class ApplicationRagCollectionPermissionDetailDto
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedTime { get; set; }

    public DateTimeOffset UpdatedTime { get; set; }

    public Guid TenantId { get; set; }

    public Guid ApplicationId { get; set; }

    public Guid RagCollectionId { get; set; }

    public bool IsEnabled { get; set; }
}