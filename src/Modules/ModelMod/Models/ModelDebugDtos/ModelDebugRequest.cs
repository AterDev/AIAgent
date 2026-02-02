namespace ModelMod.Models.ModelDebugDtos;

public sealed class ModelDebugRequest
{
    public Guid? ApplicationId { get; set; }

    public Guid ModelId { get; set; }

    public string? Provider { get; set; }

    public string? SystemPrompt { get; set; }

    public string Prompt { get; set; } = string.Empty;

    public double? Temperature { get; set; }

    public int? MaxTokens { get; set; }

    public string? RequestId { get; set; }
}
