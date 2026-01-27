namespace ModelMod.Models.ModelInvocationDtos;

/// <summary>
/// 调用记录 ItemDto
/// </summary>
/// <see cref="Entity.ModelMod.ModelInvocation"/>
public class ModelInvocationItemDto
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public Guid ModelProfileId { get; set; }

    [MaxLength(100)]
    public string? Scene { get; set; }

    public int TotalTokens { get; set; }

    public int DurationMs { get; set; }

    public InvocationStatus Status { get; set; }
}
