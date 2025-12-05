namespace Entity.AIAgentMod;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// AI模型提供商
/// </summary>
public class AIModelProvider : EntityBase
{
    /// <summary>
    /// 提供商名称
    /// </summary>
    [MaxLength(200)]
    public required string Name { get; set; }

    [MaxLength(500)]
    public string? LogoUrl { get; set; }

    /// <summary>
    /// 说明
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// 官网地址
    /// </summary>
    [MaxLength(500)]
    public string? Website { get; set; }

    /// <summary>
    /// 关联的模型列表
    /// </summary>
    public ICollection<AIModelInfo> Models { get; set; } = [];
}
