namespace Share.Services;

public sealed class ModelInvokeRequest
{
    public required string Model { get; set; }

    public string? Provider { get; set; }

    public string? Scene { get; set; }

    public List<ModelInvokeMessage> Messages { get; set; } = [];

    public Dictionary<string, string> Metadata { get; set; } = new();
}

public sealed class ModelInvokeMessage
{
    public required string Role { get; set; }

    public string Content { get; set; } = string.Empty;
}

public sealed class ModelInvokeResponse
{
    public bool Success { get; set; }

    public string? Content { get; set; }

    public UsageStats Usage { get; set; } = new();

    public string? ErrorMessage { get; set; }
}

public sealed class UsageStats
{
    public int PromptTokens { get; set; }

    public int CompletionTokens { get; set; }

    public int TotalTokens { get; set; }
}
