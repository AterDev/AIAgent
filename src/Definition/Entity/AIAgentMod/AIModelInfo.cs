namespace Entity.AIAgentMod;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// 模型信息
/// </summary>

[Index(nameof(ProviderId), nameof(Name), IsUnique = true)]
public class AIModelInfo : EntityBase
{
    /// <summary>
    /// 所属提供商 Id
    /// </summary>
    public Guid ProviderId { get; set; }

    /// <summary>
    /// 提供商引用
    /// </summary>
    [ForeignKey(nameof(ProviderId))]
    public AIModelProvider? Provider { get; set; }

    /// <summary>
    /// 模型名称
    /// </summary>
    [MaxLength(200)]
    public required string Name { get; set; }

    /// <summary>
    /// 说明
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// 上下文长度（tokens）
    /// </summary>
    public int ContextLength { get; set; }

    /// <summary>
    /// 价格（单位: 每 1k tokens 的价格）
    /// </summary>
    public decimal InputPrice { get; set; }
    public decimal OutputPrice { get; set; }
}
