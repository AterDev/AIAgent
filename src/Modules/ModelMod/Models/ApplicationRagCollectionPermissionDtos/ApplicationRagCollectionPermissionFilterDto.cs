namespace ModelMod.Models.ApplicationRagCollectionPermissionDtos;

/// <summary>
/// 应用知识库关联 FilterDto
/// </summary>
public class ApplicationRagCollectionPermissionFilterDto : FilterBase
{
    public Guid? ApplicationId { get; set; }

    public Guid? RagCollectionId { get; set; }

    public bool? IsEnabled { get; set; }
}