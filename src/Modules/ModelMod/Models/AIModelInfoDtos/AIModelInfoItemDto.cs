namespace ModelMod.Models.AIModelInfoDtos;

/// <summary>
/// 模型信息ItemDto
/// </summary>
public class AIModelInfoItemDto
{
    public Guid Id { get; set; }

    /// <summary>
    /// 模型名称
    /// </summary>
    [MaxLength(200)]
    public string? Name { get; set; }

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
    /// 所属提供商 Id
    /// </summary>
    public Guid? ProviderId { get; set; }

    /// <summary>
    /// 上下文长度（tokens）
    /// </summary>
    public int? ContextLength { get; set; }

    /// <summary>
    /// 最大上下文长度（tokens）
    /// </summary>
    public int? MaxContextTokens { get; set; }

    /// <summary>
    /// 支持聊天
    /// </summary>
    public bool? SupportsChat { get; set; }

    /// <summary>
    /// 支持向量化
    /// </summary>
    public bool? SupportsEmbedding { get; set; }

    /// <summary>
    /// 支持工具调用
    /// </summary>
    public bool? SupportsTools { get; set; }

    /// <summary>
    /// 支持视觉
    /// </summary>
    public bool? SupportsVision { get; set; }

    /// <summary>
    /// 支持 Responses API
    /// </summary>
    public bool? SupportsResponsesApi { get; set; }

    /// <summary>
    /// 价格（单位: 每 1k tokens 的价格）
    /// </summary>
    public decimal? InputPrice { get; set; }

    /// <summary>
    /// 价格（单位: 每 1k tokens 的价格）
    /// </summary>
    public decimal? OutputPrice { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }

    public DateTimeOffset CreatedTime { get; set; }
}
