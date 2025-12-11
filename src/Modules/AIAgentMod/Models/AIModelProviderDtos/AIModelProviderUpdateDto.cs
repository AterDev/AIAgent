using Entity.AIAgentMod;
namespace AIAgentMod.Models.AIModelProviderDtos;
/// <summary>
/// AI模型提供商UpdateDto
/// </summary>
/// <see cref="Entity.AIAgentMod.AIModelProvider"/>
public class AIModelProviderUpdateDto
{
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
    /// <summary>
    /// 官网地址
    /// </summary>
    [MaxLength(500)]
    public string? Website { get; set; }
    
}
