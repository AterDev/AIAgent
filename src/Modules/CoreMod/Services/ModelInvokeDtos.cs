namespace CoreMod.Services;

using CoreMod.Models;

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

