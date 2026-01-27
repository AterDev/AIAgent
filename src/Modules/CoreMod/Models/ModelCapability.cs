namespace CoreMod.Models;

/// <summary>
/// 模型能力
/// </summary>
public sealed class ModelCapability
{
    public bool SupportsChat { get; set; }

    public bool SupportsEmbedding { get; set; }

    public bool SupportsTools { get; set; }

    public bool SupportsVision { get; set; }

    public bool SupportsResponsesApi { get; set; }
}

/// <summary>
/// 模型路由结果
/// </summary>
public sealed class ModelRoute
{
    public required string Provider { get; set; }

    public string? BaseUrl { get; set; }

    public string? ApiKey { get; set; }
}
