using System.Text.Json;

namespace WorkflowMod.Services;

/// <summary>
/// Workflow definition validation helpers.
/// </summary>
internal static class WorkflowDefinitionValidator
{
    public static void ValidateOrThrow(string? definitionJson)
    {
        if (string.IsNullOrWhiteSpace(definitionJson))
        {
            return;
        }

        try
        {
            using var document = JsonDocument.Parse(definitionJson);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                throw new BusinessException("Workflow definition must be a JSON array.");
            }

            foreach (var step in document.RootElement.EnumerateArray())
            {
                if (step.ValueKind != JsonValueKind.Object)
                {
                    throw new BusinessException("Workflow step must be an object.");
                }

                if (!step.TryGetProperty("type", out var typeProperty))
                {
                    throw new BusinessException("Workflow step missing type.");
                }

                if (typeProperty.ValueKind is not (JsonValueKind.String or JsonValueKind.Number))
                {
                    throw new BusinessException("Workflow step type must be string or number.");
                }
            }
        }
        catch (JsonException)
        {
            throw new BusinessException("Workflow definition JSON invalid.");
        }
    }
}