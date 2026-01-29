namespace ModelMod.Models.AIModelInfoDtos;

/// <summary>
/// 模型信息FilterDto
/// </summary>
public class AIModelInfoFilterDto : FilterBase
{
    public Guid? ProviderId { get; set; }
}
