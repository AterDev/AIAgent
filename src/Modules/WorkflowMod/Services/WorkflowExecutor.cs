using AIAgentMod.Services;
using Entity.AIAgentMod;
using Microsoft.Agents.AI.Workflows;
using Share.Services;
using System.Text.Json;
using SystemMod.Services;
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
    SystemConfigFacade systemConfigFacade,
    IAgentExecutionService agentExecutionService,
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

    private async Task ExecuteAgentStepAsync(WorkflowStep step, Dictionary<string, object?> context, CancellationToken cancellationToken)
    {
        if (!step.AgentId.HasValue)
        {
            throw new BusinessException("Agent step missing agent ID");
        }

        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var agent = await dbContext.AIAgents
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == step.AgentId.Value && q.TenantId == userContext.TenantId, cancellationToken);

        if (agent is null)
        {
            throw new BusinessException($"Agent not found: {step.AgentId}");
        }

        // 构建 Agent 输入
        var input = step.Prompt ?? JsonSerializer.Serialize(new { prompt = "Execute agent task" });

        // 创建 Agent 执行任务
        var execution = new AgentExecution
        {
            Id = Guid.CreateVersion7(),
            AgentId = agent.Id,
            Status = AgentExecutionStatus.Running,
            InputJson = input,
            TenantId = userContext.TenantId,
        };

        dbContext.AgentExecutions.Add(execution);
        await dbContext.SaveChangesAsync(cancellationToken);

        // 执行 Agent（这里简化处理，实际可能需要异步队列）
        var success = await agentExecutionService.ExecuteAsync(
            execution.Id,
            step.ApplicationId ?? Guid.Empty,
            input,
            cancellationToken
        );

        if (!success)
        {
            // 重新加载执行结果以获取错误信息
            await dbContext.Entry(execution).ReloadAsync(cancellationToken);
            throw new BusinessException($"Agent execution failed: {execution.ErrorMessage}");
        }

        // 重新加载执行结果
        await dbContext.Entry(execution).ReloadAsync(cancellationToken);
        context[step.Name] = execution.OutputJson;
    }

    private Task ExecuteConditionStepAsync(WorkflowStep step, Dictionary<string, object?> context, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(step.Condition))
        {
            throw new BusinessException("Condition step missing condition expression");
        }

        // 简单的条件评估（实际项目可以使用更复杂的表达式引擎）
        var conditionResult = EvaluateCondition(step.Condition, context);

        context[step.Name] = new
        {
            condition = step.Condition,
            result = conditionResult,
            nextStep = conditionResult ? step.TrueStepId : step.FalseStepId
        };

        return Task.CompletedTask;
    }

    private async Task ExecuteLoopStepAsync(WorkflowStep step, Dictionary<string, object?> context, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(step.LoopCondition) && !step.MaxIterations.HasValue)
        {
            throw new BusinessException("Loop step missing condition or max iterations");
        }

        var maxIterations = step.MaxIterations ?? 100;
        var iterations = 0;
        var results = new List<object?>();

        while (iterations < maxIterations)
        {
            // 评估循环条件
            if (!string.IsNullOrWhiteSpace(step.LoopCondition))
            {
                var shouldContinue = EvaluateCondition(step.LoopCondition, context);
                if (!shouldContinue)
                {
                    break;
                }
            }

            // 执行循环体（这里简化处理，实际可能需要解析并执行嵌套步骤）
            if (!string.IsNullOrWhiteSpace(step.LoopBody))
            {
                // 将循环体作为提示执行
                var loopBodyResult = await ExecuteLoopBodyAsync(step.LoopBody, context, cancellationToken);
                results.Add(loopBodyResult);
            }

            iterations++;
            context["loop_iteration"] = iterations;
        }

        context[step.Name] = new
        {
            iterations,
            results
        };
    }

    private async Task<object?> ExecuteLoopBodyAsync(string loopBody, Dictionary<string, object?> context, CancellationToken cancellationToken)
    {
        // 简化实现：将循环体作为模板渲染并返回
        // 实际项目可能需要解析并执行嵌套的工作流步骤
        await Task.CompletedTask;
        return new { body = loopBody, executed = true };
    }

    private Task ExecuteDataTransformStepAsync(WorkflowStep step, Dictionary<string, object?> context, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(step.TransformScript))
        {
            throw new BusinessException("DataTransform step missing transform script");
        }

        // 获取输入数据
        var inputData = !string.IsNullOrWhiteSpace(step.InputPath) && context.TryGetValue(step.InputPath, out var input)
            ? input
            : context;

        // 执行转换（这里简化处理，实际可以使用脚本引擎如 Jint 或 IronPython）
        var transformedData = ApplyTransform(step.TransformScript, inputData, context);

        // 保存输出数据
        var outputKey = step.OutputPath ?? step.Name;
        context[outputKey] = transformedData;

        return Task.CompletedTask;
    }

    private async Task ExecuteDelayStepAsync(WorkflowStep step, Dictionary<string, object?> context, CancellationToken cancellationToken)
    {
        var delayMs = step.DelayMs ?? 1000;

        logger.LogInformation("Workflow step {StepName} delaying for {DelayMs}ms", step.Name, delayMs);

        await Task.Delay(delayMs, cancellationToken);

        context[step.Name] = new
        {
            delayed = true,
            delayMs
        };
    }

    private bool EvaluateCondition(string condition, Dictionary<string, object?> context)
    {
        // 简单的条件评估实现
        // 支持格式: "key == value", "key != value", "key > value", "key < value"

        var parts = condition.Split(new[] { "==", "!=", ">", "<", ">=", "<=" }, StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            // 尝试直接作为布尔值
            if (bool.TryParse(condition, out var boolValue))
            {
                return boolValue;
            }

            // 尝试从上下文获取
            if (context.TryGetValue(condition, out var value) && value is bool b)
            {
                return b;
            }

            return false;
        }

        var key = parts[0].Trim();
        var expectedValue = parts[1].Trim().Trim('"', '\'');

        if (!context.TryGetValue(key, out var actualValue))
        {
            return false;
        }

        var actualString = actualValue?.ToString() ?? string.Empty;

        if (condition.Contains("=="))
        {
            return actualString.Equals(expectedValue, StringComparison.OrdinalIgnoreCase);
        }
        else if (condition.Contains("!="))
        {
            return !actualString.Equals(expectedValue, StringComparison.OrdinalIgnoreCase);
        }
        else if (condition.Contains(">="))
        {
            return double.TryParse(actualString, out var a) &&
                   double.TryParse(expectedValue, out var b) &&
                   a >= b;
        }
        else if (condition.Contains("<="))
        {
            return double.TryParse(actualString, out var a) &&
                   double.TryParse(expectedValue, out var b) &&
                   a <= b;
        }
        else if (condition.Contains(">"))
        {
            return double.TryParse(actualString, out var a) &&
                   double.TryParse(expectedValue, out var b) &&
                   a > b;
        }
        else if (condition.Contains("<"))
        {
            return double.TryParse(actualString, out var a) &&
                   double.TryParse(expectedValue, out var b) &&
                   a < b;
        }

        return false;
    }

    private object? ApplyTransform(string transformScript, object? inputData, Dictionary<string, object?> context)
    {
        // 简单的数据转换实现
        // 实际项目可以使用更强大的脚本引擎

        // 支持 JSON 路径提取: "$.path.to.field"
        if (transformScript.StartsWith("$."))
        {
            var path = transformScript[2..];
            return ExtractJsonPath(inputData, path);
        }

        // 支持简单的模板替换: "{{key}}"
        if (transformScript.Contains("{{") && transformScript.Contains("}}"))
        {
            var result = transformScript;
            foreach (var kvp in context)
            {
                result = result.Replace($"{{{{{kvp.Key}}}}}", kvp.Value?.ToString() ?? string.Empty);
            }
            return result;
        }

        // 默认返回原始脚本
        return transformScript;
    }

    private object? ExtractJsonPath(object? data, string path)
    {
        if (data == null)
        {
            return null;
        }

        var segments = path.Split('.');
        var current = data;

        foreach (var segment in segments)
        {
            if (current == null)
            {
                return null;
            }

            if (current is Dictionary<string, object?> dict)
            {
                current = dict.TryGetValue(segment, out var value) ? value : null;
            }
            else if (current is JsonElement element)
            {
                current = element.TryGetProperty(segment, out var prop) ? prop : null;
            }
            else
            {
                // 尝试使用反射
                var type = current.GetType();
                var property = type.GetProperty(segment);
                current = property?.GetValue(current);
            }
        }

        return current;
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
            case WorkflowStepKind.AgentCall:
                await ExecuteAgentStepAsync(step, context, cancellationToken);
                break;
            case WorkflowStepKind.Condition:
                await ExecuteConditionStepAsync(step, context, cancellationToken);
                break;
            case WorkflowStepKind.Loop:
                await ExecuteLoopStepAsync(step, context, cancellationToken);
                break;
            case WorkflowStepKind.DataTransform:
                await ExecuteDataTransformStepAsync(step, context, cancellationToken);
                break;
            case WorkflowStepKind.Delay:
                await ExecuteDelayStepAsync(step, context, cancellationToken);
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
            // Condition step fields
            Condition = GetString(element, "condition"),
            TrueStepId = GetString(element, "trueStepId"),
            FalseStepId = GetString(element, "falseStepId"),
            // Loop step fields
            LoopCondition = GetString(element, "loopCondition"),
            LoopBody = GetString(element, "loopBody"),
            MaxIterations = GetInt(element, "maxIterations"),
            // DataTransform step fields
            TransformScript = GetString(element, "transformScript"),
            InputPath = GetString(element, "inputPath"),
            OutputPath = GetString(element, "outputPath"),
            // Delay step fields
            DelayMs = GetInt(element, "delayMs"),
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

        // Condition step fields
        public string? Condition { get; set; }
        public string? TrueStepId { get; set; }
        public string? FalseStepId { get; set; }

        // Loop step fields
        public string? LoopCondition { get; set; }
        public string? LoopBody { get; set; }
        public int? MaxIterations { get; set; }

        // DataTransform step fields
        public string? TransformScript { get; set; }
        public string? InputPath { get; set; }
        public string? OutputPath { get; set; }

        // Delay step fields
        public int? DelayMs { get; set; }
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
