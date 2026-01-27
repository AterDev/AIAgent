namespace ModelMod.Models.ModelInvocationDtos;

/// <summary>
/// 调用记录 UpdateDto
/// </summary>
/// <see cref="Entity.ModelMod.ModelInvocation"/>
public class ModelInvocationUpdateDto
{
    [MaxLength(100)]
    public string? Scene { get; set; }

    public int? PromptTokens { get; set; }

    public int? CompletionTokens { get; set; }

    public int? TotalTokens { get; set; }

    public int? DurationMs { get; set; }

    public InvocationStatus? Status { get; set; }

    [MaxLength(500)]
    public string? ErrorMessage { get; set; }
}
