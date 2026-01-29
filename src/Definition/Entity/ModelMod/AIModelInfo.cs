namespace Entity.ModelMod;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// 模型信息（包含能力和定价）
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
    /// 显示名称
    /// </summary>
    [MaxLength(200)]
    public string? DisplayName { get; set; }

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
    /// 最大上下文长度
    /// </summary>
    public int MaxContextTokens { get; set; }

    /// <summary>
    /// 支持聊天
    /// </summary>
    public bool SupportsChat { get; set; }

    /// <summary>
    /// 支持向量化
    /// </summary>
    public bool SupportsEmbedding { get; set; }

    /// <summary>
    /// 支持工具调用
    /// </summary>
    public bool SupportsTools { get; set; }

    /// <summary>
    /// 支持视觉
    /// </summary>
    public bool SupportsVision { get; set; }

    /// <summary>
    /// 支持 Responses API
    /// </summary>
    public bool SupportsResponsesApi { get; set; }

    /// <summary>
    /// 价格（单位: 每 1k tokens 的价格）
    /// </summary>
    public decimal InputPrice { get; set; }
    public decimal OutputPrice { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}
