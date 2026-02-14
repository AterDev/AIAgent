namespace KnowledgeBaseMod.Models.RagAgentConfigDtos;
/// <summary>
/// RAG 模型配置ItemDto
/// </summary>
/// <see cref="RagAgentConfig"/>
public class RagAgentConfigItemDto
{
    /// <summary>
    /// 配置项名称
    /// </summary>
    [MaxLength(100)]
    public string Key { get; set; } = default!;
    [Key]
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public DateTimeOffset CreatedTime { get; set; } = DateTimeOffset.UtcNow;
    
}
