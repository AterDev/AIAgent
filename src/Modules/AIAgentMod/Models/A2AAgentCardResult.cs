namespace AIAgentMod.Models;

/// <summary>
/// A2A Agent Card discovery result.
/// </summary>
public sealed class A2AAgentCardResult
{
    public bool Success { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<string> Skills { get; set; } = [];
    public string? CardJson { get; set; }
    public string? ErrorMessage { get; set; }
}
