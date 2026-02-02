namespace ModelMod.Models.ModelDebugDtos;

public sealed class ModelDebugResponse
{
    public string Content { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public int PromptTokens { get; set; }

    public int CompletionTokens { get; set; }

    public int TotalTokens { get; set; }

    public string FinishReason { get; set; } = "stop";

    public int DurationMs { get; set; }
}
