using Entity.AIAgentMod;
namespace AIAgentMod.Models.AIModelInfoDtos;
/// <summary>
/// 模型信息UpdateDto
/// </summary>
/// <see cref="Entity.AIAgentMod.AIModelInfo"/>
public class AIModelInfoUpdateDto
{
    /// <summary>
    /// 上下文长度（tokens）
    /// </summary>
    public int? ContextLength { get; set; }
    /// <summary>
    /// 说明
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }
    /// <summary>
    /// 价格（单位: 每 1k tokens 的价格）
    /// </summary>
    public decimal? InputPrice { get; set; }
    /// <summary>
    /// 模型名称
    /// </summary>
    [MaxLength(200)]
    public string? Name { get; set; }
    public decimal? OutputPrice { get; set; }
    public Guid? ProviderId { get; set; }
    
}
