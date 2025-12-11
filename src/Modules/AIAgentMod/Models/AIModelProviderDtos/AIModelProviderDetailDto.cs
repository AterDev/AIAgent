using Entity.AIAgentMod;
namespace AIAgentMod.Models.AIModelProviderDtos;
/// <summary>
/// AI模型提供商DetailDto
/// </summary>
/// <see cref="Entity.AIAgentMod.AIModelProvider"/>
public class AIModelProviderDetailDto
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
    /// <summary>
    /// 说明
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }
    [MaxLength(500)]
    public string? LogoUrl { get; set; }
    /// <summary>
    /// 提供商名称
    /// </summary>
    [MaxLength(200)]
    public string? Name { get; set; }
    public Guid TenantId { get; set; }
    public DateTimeOffset UpdatedTime { get; set; }
    /// <summary>
    /// 官网地址
    /// </summary>
    [MaxLength(500)]
    public string? Website { get; set; }
    
}
