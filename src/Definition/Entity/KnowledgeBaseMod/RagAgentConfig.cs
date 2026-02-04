using Entity.CoreMod;
using Entity.ModelMod;

namespace Entity.KnowledgeBaseMod;

/// <summary>
/// RAG 模型配置
/// </summary>
[Index(nameof(Key), IsUnique = true)]
public class RagAgentConfig : EntityBase
{
    /// <summary>
    /// 配置项名称
    /// </summary>
    [MaxLength(100)]
    public required string Key { get; set; }

    /// <summary>
    /// 配置项值
    /// </summary>
    [MaxLength(200)]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// 关联的 AI 模型 ID
    /// </summary>
    public Guid? AIModelInfoId { get; set; }

    /// <summary>
    /// 关联的 AI 模型
    /// </summary>
    [ForeignKey(nameof(AIModelInfoId))]
    public AIModelInfo? AIModelInfo { get; set; }

    /// <summary>
    /// 配置项描述
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// 关联的提示词 ID
    /// </summary>
    public Guid? AIPromptId { get; set; }

    /// <summary>
    /// 关联的提示词
    /// </summary>
    [ForeignKey(nameof(AIPromptId))]
    public AIPrompt? AIPrompt { get; set; }
}
