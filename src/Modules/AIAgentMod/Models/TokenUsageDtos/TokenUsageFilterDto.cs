using Entity.AIAgentMod;
namespace AIAgentMod.Models.TokenUsageDtos;
/// <summary>
/// 用户Token用量信息FilterDto
/// </summary>
/// <see cref="Entity.AIAgentMod.TokenUsage"/>
public class TokenUsageFilterDto : FilterBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid? UserId { get; set; }
    
}
