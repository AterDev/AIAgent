using Entity.KnowledgeBaseMod;
namespace KnowledgeBaseMod.Models.RagAgentConfigDtos;
/// <summary>
/// RAG 模型配置AddDto
/// </summary>
/// <see cref="Entity.KnowledgeBaseMod.RagAgentConfig"/>
public class RagAgentConfigAddDto
{
    /// <summary>
    /// 配置项名称
    /// </summary>
    [MaxLength(100)]
    public string Key { get; set; } = default!;
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
    /// 配置项描述
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }
    /// <summary>
    /// 关联的提示词 ID
    /// </summary>
    public Guid? AIPromptId { get; set; }
    
}
