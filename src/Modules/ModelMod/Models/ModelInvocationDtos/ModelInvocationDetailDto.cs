using Entity.ModelMod;
namespace ModelMod.Models.ModelInvocationDtos;

/// <summary>
/// 调用记录 DetailDto
/// </summary>
/// <see cref="Entity.ModelMod.ModelInvocation"/>
public class ModelInvocationDetailDto
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public Guid ModelProfileId { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
    public DateTimeOffset UpdatedTime { get; set; }
    public Guid TenantId { get; set; }

    [MaxLength(100)]
    public string? Scene { get; set; }

    public int PromptTokens { get; set; }

    public int CompletionTokens { get; set; }

    public int TotalTokens { get; set; }

    public int DurationMs { get; set; }

    public InvocationStatus Status { get; set; }

    [MaxLength(500)]
    public string? ErrorMessage { get; set; }
}
