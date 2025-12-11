using Entity.AIAgentMod;
namespace AIAgentMod.Models.TokenUsageRecordDtos;
/// <summary>
/// 用户Token用量信息FilterDto
/// </summary>
/// <see cref="Entity.AIAgentMod.TokenUsageRecord"/>
public class TokenUsageRecordFilterDto : FilterBase
{
    [MaxLength(100)]
    public string? ModelId { get; set; }
    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid? UserId { get; set; }
    
}
