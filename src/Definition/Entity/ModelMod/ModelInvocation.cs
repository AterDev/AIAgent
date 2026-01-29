namespace Entity.ModelMod;

/// <summary>
/// 模型调用记录
/// </summary>
[Index(nameof(ApplicationId), nameof(AIModelInfoId), nameof(Scene))]
public class ModelInvocation : EntityBase
{
    public Guid ApplicationId { get; set; }

    [ForeignKey(nameof(ApplicationId))]
    public Application? Application { get; set; }

    public Guid AIModelInfoId { get; set; }

    [ForeignKey(nameof(AIModelInfoId))]
    public AIModelInfo? AIModelInfo { get; set; }

    [MaxLength(100)]
    public string Scene { get; set; } = string.Empty;

    public int PromptTokens { get; set; }

    public int CompletionTokens { get; set; }

    public int TotalTokens { get; set; }

    public int DurationMs { get; set; }

    public InvocationStatus Status { get; set; }

    [MaxLength(500)]
    public string? ErrorMessage { get; set; }
}
