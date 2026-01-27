using Microsoft.Agents.AI.Workflows;
using Share.Services;
using System.Text.Json;
using MAFWorkflow = Microsoft.Agents.AI.Workflows.Workflow;
using WorkflowEntity = Entity.WorkflowMod.Workflow;

namespace WorkflowMod.Services;

/// <summary>
/// 工作流执行器（简化：逐步执行）
/// </summary>
public class WorkflowExecutor(
    TenantDbFactory dbContextFactory,
    IUserContext userContext,
    IModelInvokeFacade modelInvokeFacade,
    IRagQueryFacade ragQueryFacade,
    IMcpToolExecutorFacade mcpToolExecutorFacade,
    Share.Services.ISystemConfigFacade systemConfigFacade,
    ILogger<WorkflowExecutor> logger
) : IWorkflowExecutor
{
    public async Task<bool> ExecuteAsync(Guid executionId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var execution = await dbContext.WorkflowExecutions
            .FirstOrDefaultAsync(q => q.Id == executionId && q.TenantId == userContext.TenantId, cancellationToken);

        if (execution is null)
        {
            return false;
        }

        var workflow = await dbContext.Workflows
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == execution.WorkflowId && q.TenantId == userContext.TenantId, cancellationToken);

        if (workflow is null)
        {
            execution.Status = WorkflowExecutionStatus.Failed;
            execution.ErrorMessage = "Workflow not found";
            await dbContext.SaveChangesAsync(cancellationToken);
            return false;
        }

        execution.Status = WorkflowExecutionStatus.Running;
        execution.ErrorMessage = null;
        await dbContext.SaveChangesAsync(cancellationToken);

        var stopwatch = Stopwatch.StartNew();

        var steps = ParseSteps(workflow.DefinitionJson);
        var context = new Dictionary<string, object?>
        {
            ["executionId"] = executionId,
            ["workflowId"] = workflow.Id,
            ["tenantId"] = userContext.TenantId,
        };

        try
        {
            if (steps.Count == 0)
            {
                throw new BusinessException("Workflow definition is empty.");
            }

            var workflowInstance = BuildWorkflow(workflow, steps);
            await InProcessExecution.RunAsync(
                workflowInstance,
                context,
                executionId.ToString(),
                cancellationToken
            );

            execution.Status = WorkflowExecutionStatus.Completed;
            execution.OutputJson = JsonSerializer.Serialize(context);
            execution.CompletedTime = DateTimeOffset.UtcNow;
            execution.DurationMs = (int)stopwatch.ElapsedMilliseconds;
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Workflow execution failed {ExecutionId}", executionId);
            execution.Status = WorkflowExecutionStatus.Failed;
            execution.ErrorMessage = ex.Message;
            execution.CompletedTime = DateTimeOffset.UtcNow;
            execution.DurationMs = (int)stopwatch.ElapsedMilliseconds;
            await dbContext.SaveChangesAsync(cancellationToken);
            return false;
        }
    }

    private async Task ExecuteModelStepAsync(WorkflowStep step, Dictionary<string, object?> context, CancellationToken cancellationToken)
    {
        if (!step.ApplicationId.HasValue || string.IsNullOrWhiteSpace(step.Model))
        {
            throw new BusinessException("Model step missing application or model");
        }

        var prompt = step.Prompt ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(step.PromptTemplateKey))
        {
            var template = await systemConfigFacade.GetValueAsync("Workflow", step.PromptTemplateKey, cancellationToken);
            if (!string.IsNullOrWhiteSpace(template))
            {
                prompt = systemConfigFacade.RenderTemplate(template, new Dictionary<string, string>
                {
                    ["input"] = prompt,
                });
            }
        }

        var request = new ModelInvokeRequest
        {
            Model = step.Model,
            Provider = step.Provider,
            Scene = step.Name,
            Messages = step.Prompt is not null
                ? [new ModelInvokeMessage { Role = "user", Content = prompt }]
                : [],
        };

        var response = await modelInvokeFacade.ChatAsync(step.ApplicationId.Value, request, cancellationToken);
        context[step.Name] = response.Content;
    }

    private async Task ExecuteToolStepAsync(WorkflowStep step, Dictionary<string, object?> context, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(step.ToolName))
        {
            throw new BusinessException("Tool step missing tool name");
        }

        var request = new Share.Services.ToolExecutionRequest
        {
            ToolName = step.ToolName,
            ArgumentsJson = step.ArgumentsJson,
            ApplicationId = step.ApplicationId,
            AgentId = step.AgentId,
        };

        var result = await mcpToolExecutorFacade.ExecuteAsync(request, cancellationToken);
        context[step.Name] = result.OutputJson;
    }

    private async Task ExecuteRagStepAsync(WorkflowStep step, Dictionary<string, object?> context, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(step.Query))
        {
            throw new BusinessException("RAG step missing query");
        }

        var result = await ragQueryFacade.QueryAsync(new Share.Services.RagQueryRequest
        {
            Query = step.Query,
            CollectionId = step.CollectionId,
            TopK = step.TopK ?? 5,
        }, cancellationToken);

        context[step.Name] = result;
    }

    private static List<WorkflowStep> ParseSteps(string? definitionJson)
    {
        if (string.IsNullOrWhiteSpace(definitionJson))
        {
            return [];
        }

        try
        {
            using var document = JsonDocument.Parse(definitionJson);
            if (document.RootElement.ValueKind == JsonValueKind.Array)
            {
                return document.RootElement.EnumerateArray()
                    .Select(ToStep)
                    .ToList();
            }

            if (document.RootElement.ValueKind == JsonValueKind.Object
                && TryGetProperty(document.RootElement, "steps", out var stepsElement)
                && stepsElement.ValueKind == JsonValueKind.Array)
            {
                return stepsElement.EnumerateArray()
                    .Select(ToStep)
                    .ToList();
            }

            return [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private MAFWorkflow BuildWorkflow(WorkflowEntity workflow, List<WorkflowStep> steps)
    {
        var executorBindings = new Dictionary<string, ExecutorBinding>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < steps.Count; i++)
        {
            var step = steps[i];
            var stepId = step.Id ?? step.Name ?? $"step_{i}";
            if (executorBindings.ContainsKey(stepId))
            {
                stepId = $"{stepId}_{i}";
                step.Id = stepId;
            }

            var executor = new FunctionExecutor<Dictionary<string, object?>, Dictionary<string, object?>>(
                stepId,
                async (input, _, token) =>
                {
                    await ExecuteStepAsync(step, input, token);
                    return input;
                },
                declareCrossRunShareable: true
            );

            executorBindings[stepId] = executor;
        }

        var firstStep = steps[0];
        var firstBinding = executorBindings[firstStep.Id ?? firstStep.Name ?? "step_0"];
        var builder = new WorkflowBuilder(firstBinding)
            .WithName(workflow.Name)
            .WithDescription(workflow.Description);

        foreach (var binding in executorBindings.Values)
        {
            builder.BindExecutor(binding);
        }

        var stepById = steps
            .Where(step => !string.IsNullOrWhiteSpace(step.Id))
            .ToDictionary(step => step.Id!, step => step, StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < steps.Count; i++)
        {
            var step = steps[i];
            var currentId = step.Id ?? step.Name ?? $"step_{i}";
            if (!executorBindings.TryGetValue(currentId, out var currentBinding))
            {
                continue;
            }

            if (step.NextStepIds is { Length: > 0 })
            {
                foreach (var nextId in step.NextStepIds)
                {
                    if (string.IsNullOrWhiteSpace(nextId))
                    {
                        continue;
                    }

                    if (executorBindings.TryGetValue(nextId, out var nextBinding))
                    {
                        builder.AddEdge(currentBinding, nextBinding);
                    }
                    else if (stepById.TryGetValue(nextId, out var nextStep))
                    {
                        var resolvedId = nextStep.Id ?? nextStep.Name;
                        if (!string.IsNullOrWhiteSpace(resolvedId) && executorBindings.TryGetValue(resolvedId, out var resolvedBinding))
                        {
                            builder.AddEdge(currentBinding, resolvedBinding);
                        }
                    }
                }
                continue;
            }

            if (i + 1 < steps.Count)
            {
                var nextStep = steps[i + 1];
                var nextId = nextStep.Id ?? nextStep.Name ?? $"step_{i + 1}";
                if (executorBindings.TryGetValue(nextId, out var nextBinding))
                {
                    builder.AddEdge(currentBinding, nextBinding);
                }
            }
        }

        return builder.Build();
    }

    private async Task ExecuteStepAsync(WorkflowStep step, Dictionary<string, object?> context, CancellationToken cancellationToken)
    {
        switch (step.Kind)
        {
            case WorkflowStepKind.ModelCall:
                await ExecuteModelStepAsync(step, context, cancellationToken);
                break;
            case WorkflowStepKind.ToolCall:
                await ExecuteToolStepAsync(step, context, cancellationToken);
                break;
            case WorkflowStepKind.RagQuery:
                await ExecuteRagStepAsync(step, context, cancellationToken);
                break;
            default:
                throw new BusinessException($"Workflow step type not supported: {step.Type}");
        }
    }

    private static WorkflowStep ToStep(JsonElement element)
    {
        var step = new WorkflowStep
        {
            Id = GetString(element, "id"),
            Name = GetString(element, "name") ?? "step",
            Type = GetString(element, "type"),
            ToolName = GetString(element, "toolName"),
            ArgumentsJson = GetString(element, "argumentsJson"),
            Prompt = GetString(element, "prompt"),
            PromptTemplateKey = GetString(element, "promptTemplateKey"),
            Query = GetString(element, "query"),
            Model = GetString(element, "model"),
            Provider = GetString(element, "provider"),
            TopK = GetInt(element, "topK"),
            ApplicationId = GetGuid(element, "applicationId"),
            AgentId = GetGuid(element, "agentId"),
            CollectionId = GetGuid(element, "collectionId"),
            NextStepIds = GetStringArray(element, "nextStepIds"),
        };

        step.Kind = ParseStepKind(step.Type, element);
        return step;
    }

    private static WorkflowStepKind ParseStepKind(string? type, JsonElement element)
    {
        if (!string.IsNullOrWhiteSpace(type))
        {
            return type.Trim().ToLowerInvariant() switch
            {
                "model_call" or "modelcall" => WorkflowStepKind.ModelCall,
                "tool_call" or "toolcall" => WorkflowStepKind.ToolCall,
                "rag_query" or "ragquery" or "rag" => WorkflowStepKind.RagQuery,
                "agent_call" => WorkflowStepKind.AgentCall,
                "condition" => WorkflowStepKind.Condition,
                "loop" => WorkflowStepKind.Loop,
                "data_transform" or "datatransform" => WorkflowStepKind.DataTransform,
                "delay" => WorkflowStepKind.Delay,
                _ => WorkflowStepKind.Unknown,
            };
        }

        if (TryGetProperty(element, "type", out var typeElement) && typeElement.ValueKind == JsonValueKind.Number)
        {
            var intValue = typeElement.GetInt32();
            return intValue switch
            {
                0 => WorkflowStepKind.ModelCall,
                1 => WorkflowStepKind.ToolCall,
                2 => WorkflowStepKind.RagQuery,
                _ => WorkflowStepKind.Unknown,
            };
        }

        return WorkflowStepKind.Unknown;
    }

    private static bool TryGetProperty(JsonElement element, string name, out JsonElement value)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    private static string? GetString(JsonElement element, string name)
    {
        if (!TryGetProperty(element, name, out var value))
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
    }

    private static Guid? GetGuid(JsonElement element, string name)
    {
        var value = GetString(element, name);
        return Guid.TryParse(value, out var guid) ? guid : null;
    }

    private static int? GetInt(JsonElement element, string name)
    {
        if (!TryGetProperty(element, name, out var value))
        {
            return null;
        }

        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number))
        {
            return number;
        }

        if (value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out var parsed))
        {
            return parsed;
        }

        return null;
    }

    private static string[]? GetStringArray(JsonElement element, string name)
    {
        if (!TryGetProperty(element, name, out var value))
        {
            return null;
        }

        if (value.ValueKind == JsonValueKind.Array)
        {
            return value.EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.String)
                .Select(item => item.GetString()!)
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .ToArray();
        }

        if (value.ValueKind == JsonValueKind.String)
        {
            var raw = value.GetString();
            return string.IsNullOrWhiteSpace(raw) ? null : [raw];
        }

        return null;
    }

    private sealed class WorkflowStep
    {
        public string? Id { get; set; }
        public string Name { get; set; } = "step";
        public string? Type { get; set; }
        public WorkflowStepKind Kind { get; set; } = WorkflowStepKind.Unknown;
        public Guid? ApplicationId { get; set; }
        public Guid? AgentId { get; set; }
        public string? Model { get; set; }
        public string? Provider { get; set; }
        public string? Prompt { get; set; }
        public string? PromptTemplateKey { get; set; }
        public string? ToolName { get; set; }
        public string? ArgumentsJson { get; set; }
        public string? Query { get; set; }
        public Guid? CollectionId { get; set; }
        public int? TopK { get; set; }
        public string[]? NextStepIds { get; set; }
    }

    private enum WorkflowStepKind
    {
        Unknown = 0,
        ModelCall = 1,
        ToolCall = 2,
        RagQuery = 3,
        AgentCall = 4,
        Condition = 5,
        Loop = 6,
        DataTransform = 7,
        Delay = 8,
    }
}
