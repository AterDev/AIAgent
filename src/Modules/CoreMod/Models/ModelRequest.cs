namespace CoreMod.Models;

/// <summary>
/// 模型调用请求
/// </summary>
public sealed class ModelRequest
{
    public required string Model { get; set; }

    public string? Provider { get; set; }

    public string? Scene { get; set; }

    public List<ModelMessage> Messages { get; set; } = new();

    public List<ToolCall> Tools { get; set; } = new();

    public Dictionary<string, string> Metadata { get; set; } = new();
}
