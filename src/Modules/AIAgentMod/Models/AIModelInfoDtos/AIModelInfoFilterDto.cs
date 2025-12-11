using Entity.AIAgentMod;
namespace AIAgentMod.Models.AIModelInfoDtos;
/// <summary>
/// 模型信息FilterDto
/// </summary>
/// <see cref="Entity.AIAgentMod.AIModelInfo"/>
public class AIModelInfoFilterDto : FilterBase
{
    public Guid? ProviderId { get; set; }
    
}
