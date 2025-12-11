using Entity.AIAgentMod;
namespace AIAgentMod.Models.AIModelProviderDtos;
/// <summary>
/// AI模型提供商ItemDto
/// </summary>
/// <see cref="Entity.AIAgentMod.AIModelProvider"/>
public class AIModelProviderItemDto
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
    
}
