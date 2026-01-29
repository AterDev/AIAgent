namespace ModelMod.Models.AIModelInfoDtos;

/// <summary>
/// 模型信息ItemDto
/// </summary>
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

    /// <summary>
    /// 模型名称
    /// </summary>
    [MaxLength(200)]
    public string? Name { get; set; }

    public Guid? ProviderId { get; set; }
}
