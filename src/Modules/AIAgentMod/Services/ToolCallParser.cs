using CoreMod.Models;
using System.Text.Json;

namespace AIAgentMod.Services;

/// <summary>
/// Shared utility for parsing tool calls from model response content
/// and loading tool definitions from the database
/// </summary>
public static class ToolCallParser
{
    /// <summary>
    /// Load tool definitions from DB for the specified tool names
    /// </summary>
    public static async Task<List<ModelToolDefinition>> LoadToolDefinitionsAsync(
        DefaultDbContext dbContext,
        List<string> toolNames,
        CancellationToken cancellationToken)
    {
        if (toolNames.Count == 0)
        {
            return [];
        }

        return await dbContext.McpTools
            .AsNoTracking()
            .Where(t => toolNames.Contains(t.Name) && t.IsEnabled && !t.IsDeleted)
            .Select(t => new ModelToolDefinition
            {
                Name = t.Name,
                Description = t.Description,
                ParametersJson = t.SchemaJson,
            })
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Fallback: parse tool calls from response content text
    /// (for models that don't support structured function calling)
    /// </summary>
    public static List<ToolCall> ParseFromContent(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return [];
        }

        try
        {
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;
            var toolCalls = new List<ToolCall>();

            if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("tool", out var tool) || root.TryGetProperty("toolName", out tool))
                {
                    var name = tool.GetString();
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        var arguments = root.TryGetProperty("arguments", out var args) ? args.GetRawText() : null;
                        toolCalls.Add(new ToolCall { Name = name, ArgumentsJson = arguments ?? string.Empty });
                    }
                }

                if (root.TryGetProperty("tool_calls", out var toolCallsArray) && toolCallsArray.ValueKind == JsonValueKind.Array)
                {
                    foreach (var toolCallElement in toolCallsArray.EnumerateArray())
                    {
                        if (toolCallElement.ValueKind != JsonValueKind.Object)
                        {
                            continue;
                        }

                        var name = toolCallElement.TryGetProperty("name", out var nameElement) ? nameElement.GetString() : null;
                        var args = toolCallElement.TryGetProperty("arguments", out var argsElement) ? argsElement.GetRawText() : null;

                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            toolCalls.Add(new ToolCall { Name = name, ArgumentsJson = args ?? string.Empty });
                        }
                    }
                }
            }

            return toolCalls;
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
