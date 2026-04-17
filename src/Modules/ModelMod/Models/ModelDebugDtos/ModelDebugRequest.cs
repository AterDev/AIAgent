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

    /// <summary>
    /// 多模态图片输入（data URI 或 http(s) URL）。
    /// </summary>
    public List<string> Images { get; set; } = new();

    public string? RequestId { get; set; }
}
