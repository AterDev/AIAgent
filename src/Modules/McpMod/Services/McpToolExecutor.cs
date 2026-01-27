using McpMod.Models.ToolExecutionDtos;
using System.Text.Json;

namespace McpMod.Services;

/// <summary>
/// MCP 工具执行器（含审计记录）
/// </summary>
public class McpToolExecutor(
    TenantDbFactory dbContextFactory,
    ILogger<McpToolExecutor> logger,
    IUserContext userContext,
    BuiltinToolExecutor builtinToolExecutor
) : IMcpToolExecutor
{
    public async Task<ToolExecutionResult> ExecuteAsync(ToolExecutionRequest request, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var stopwatch = Stopwatch.StartNew();

        var tool = await dbContext.McpTools
            .AsNoTracking()
            .FirstOrDefaultAsync(
                q => q.Name == request.ToolName
                    && q.IsEnabled
                    && q.TenantId == userContext.TenantId,
                cancellationToken
            );

        if (tool is null)
        {
            return await SaveRecordAsync(
                dbContext,
                request,
                null,
                new ToolExecutionResult
                {
                    Success = false,
                    ErrorMessage = $"Tool '{request.ToolName}' not found",
                },
                stopwatch,
                cancellationToken
            );
        }

        if (!string.IsNullOrWhiteSpace(tool.SchemaJson))
        {
            if (!TryParseJson(tool.SchemaJson, out var schemaError))
            {
                return await SaveRecordAsync(
                    dbContext,
                    request,
                    tool.Id,
                    new ToolExecutionResult
                    {
                        Success = false,
                        ErrorMessage = $"Tool schema is invalid: {schemaError}",
                    },
                    stopwatch,
                    cancellationToken
                );
            }

            if (!TryParseJson(request.ArgumentsJson, out var argumentError))
            {
                return await SaveRecordAsync(
                    dbContext,
                    request,
                    tool.Id,
                    new ToolExecutionResult
                    {
                        Success = false,
                        ErrorMessage = $"Arguments json is invalid: {argumentError}",
                    },
                    stopwatch,
                    cancellationToken
                );
            }
        }

        if (request.ApplicationId.HasValue)
        {
            var permitted = await dbContext.ApplicationToolPermissions
                .AsNoTracking()
                .AnyAsync(
                    q => q.ApplicationId == request.ApplicationId
                        && q.ToolName == tool.Name
                        && q.IsEnabled
                        && q.TenantId == userContext.TenantId,
                    cancellationToken
                );

            if (!permitted)
            {
                return await SaveRecordAsync(
                    dbContext,
                    request,
                    tool.Id,
                    new ToolExecutionResult
                    {
                        Success = false,
                        ErrorMessage = "Application is not allowed to use this tool",
                    },
                    stopwatch,
                    cancellationToken
                );
            }
        }

        ToolExecutionResult result;
        try
        {
            result = tool.ToolType switch
            {
                McpToolType.Builtin => await builtinToolExecutor.ExecuteAsync(request, cancellationToken),
                _ => new ToolExecutionResult
                {
                    Success = false,
                    ErrorMessage = $"Tool '{tool.Name}' is not supported yet",
                },
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Tool execution failed: {ToolName}", tool.Name);
            result = new ToolExecutionResult
            {
                Success = false,
                ErrorMessage = ex.Message,
            };
        }

        return await SaveRecordAsync(dbContext, request, tool.Id, result, stopwatch, cancellationToken);
    }

    private static async Task<ToolExecutionResult> SaveRecordAsync(
        DefaultDbContext dbContext,
        ToolExecutionRequest request,
        Guid? toolId,
        ToolExecutionResult result,
        Stopwatch stopwatch,
        CancellationToken cancellationToken
    )
    {
        stopwatch.Stop();
        dbContext.ToolCallRecords.Add(new ToolCallRecord
        {
            ToolId = toolId ?? Guid.Empty,
            ApplicationId = request.ApplicationId,
            AgentId = request.AgentId,
            InputJson = request.ArgumentsJson ?? string.Empty,
            OutputJson = result.OutputJson ?? string.Empty,
            DurationMs = (int)stopwatch.ElapsedMilliseconds,
            Status = result.Success ? ToolCallStatus.Success : ToolCallStatus.Failed,
            ErrorMessage = result.ErrorMessage,
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        return result;
    }

    private static bool TryParseJson(string? json, out string? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(json))
        {
            return true;
        }

        try
        {
            using var _ = JsonDocument.Parse(json);
            return true;
        }
        catch (JsonException ex)
        {
            error = ex.Message;
            return false;
        }
    }
}
