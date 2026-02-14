namespace ModelMod.Models.ModelInvocationDtos;

/// <summary>
/// 调用记录 AddDto
/// </summary>
/// <see cref="ModelInvocation"/>
public class ModelInvocationAddDto
{
    public Guid ApplicationId { get; set; }

    public Guid AIModelInfoId { get; set; }

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
