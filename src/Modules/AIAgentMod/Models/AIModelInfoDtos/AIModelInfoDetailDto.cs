using Entity.AIAgentMod;
namespace AIAgentMod.Models.AIModelInfoDtos;
/// <summary>
/// 模型信息DetailDto
/// </summary>
/// <see cref="Entity.AIAgentMod.AIModelInfo"/>
public class AIModelInfoDetailDto
{
    public Guid Id { get; set; }
    /// <summary>
    /// 上下文长度（tokens）
    /// </summary>
    public int? ContextLength { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
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
    /// <summary>
    /// 所属提供商 Id
    /// </summary>
    public Guid? ProviderId { get; set; }
    public Guid TenantId { get; set; }
    public DateTimeOffset UpdatedTime { get; set; }
    
}
