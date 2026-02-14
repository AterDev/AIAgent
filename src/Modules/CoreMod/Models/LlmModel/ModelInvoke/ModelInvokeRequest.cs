using CoreMod.Models;

namespace CoreMod.Models.ModelInvoke;

public sealed class ModelInvokeRequest
{
    public required string Model { get; set; }

    public string? Provider { get; set; }

    public string? Scene { get; set; }

    public List<ModelInvokeMessage> Messages { get; set; } = [];

    /// <summary>
    /// Tool definitions for function calling
    /// </summary>
    public List<ModelToolDefinition> ToolDefinitions { get; set; } = [];

    public Dictionary<string, string> Metadata { get; set; } = new();
}
