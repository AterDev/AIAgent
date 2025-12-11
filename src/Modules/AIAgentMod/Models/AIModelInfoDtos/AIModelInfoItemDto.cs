using Entity.AIAgentMod;
namespace AIAgentMod.Models.AIModelInfoDtos;
/// <summary>
/// 模型信息ItemDto
/// </summary>
/// <see cref="Entity.AIAgentMod.AIModelInfo"/>
public class AIModelInfoItemDto
{
    public Guid Id { get; set; }
    /// <summary>
    /// 上下文长度（tokens）
    /// </summary>
    public int? ContextLength { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
    /// <summary>
    /// 价格（单位: 每 1k tokens 的价格）
    /// </summary>
    public decimal? InputPrice { get; set; }
    public decimal? OutputPrice { get; set; }
    
}
