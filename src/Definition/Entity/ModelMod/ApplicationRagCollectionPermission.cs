using Entity.KnowledgeBaseMod;

namespace Entity.ModelMod;

/// <summary>
/// 应用知识库关联
/// </summary>
[Index(nameof(ApplicationId), nameof(RagCollectionId), IsUnique = true)]
public class ApplicationRagCollectionPermission : EntityBase
{
    public Guid ApplicationId { get; set; }

    [ForeignKey(nameof(ApplicationId))]
    public Application? Application { get; set; }

    public Guid RagCollectionId { get; set; }

    [ForeignKey(nameof(RagCollectionId))]
    public RagCollection? RagCollection { get; set; }

    public bool IsEnabled { get; set; } = true;
}