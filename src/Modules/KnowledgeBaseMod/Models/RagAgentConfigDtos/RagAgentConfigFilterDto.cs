namespace KnowledgeBaseMod.Models.RagAgentConfigDtos;
/// <summary>
/// RAG 模型配置FilterDto
/// </summary>
/// <see cref="RagAgentConfig"/>
public class RagAgentConfigFilterDto : FilterBase
{
    /// <summary>
    /// 配置项名称
    /// </summary>
    [MaxLength(100)]
    public string? Key { get; set; }
    
}
