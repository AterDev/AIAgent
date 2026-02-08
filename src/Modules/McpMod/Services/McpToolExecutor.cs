using ModelContextProtocol.Protocol;
using Share.Services;
using System.Text.Json;

namespace McpMod.Services;

/// <summary>
/// MCP 工具执行器（含审计记录）
/// 支持 Builtin / External / Custom 三种工具类型
/// 通过官方 MCP SDK 支持 Http/SSE/stdio 传输
/// </summary>
public class McpToolExecutor(
    TenantDbFactory dbContextFactory,
    ILogger<McpToolExecutor> logger,
    IUserContext userContext,
    BuiltinToolExecutor builtinToolExecutor,
    McpClientProvider mcpClientProvider
) : IMcpToolExecutor
{
    private static readonly TimeSpan ToolTimeout = TimeSpan.FromSeconds(30);

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
                McpToolType.External => await ExecuteExternalToolAsync(dbContext, tool, request, cancellationToken),
                McpToolType.Custom => await ExecuteExternalToolAsync(dbContext, tool, request, cancellationToken),
                _ => new ToolExecutionResult
                {
                    Success = false,
                    ErrorMessage = $"Unknown tool type: {tool.ToolType}",
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

    /// <summary>
    /// Execute external/custom tool via MCP SDK (supports Http, SSE, stdio)
    /// </summary>
    private async Task<ToolExecutionResult> ExecuteExternalToolAsync(
        DefaultDbContext dbContext,
        McpTool tool,
        ToolExecutionRequest request,
        CancellationToken cancellationToken)
    {
        if (!tool.ServerId.HasValue)
        {
            return new ToolExecutionResult
            {
                Success = false,
                ErrorMessage = $"Tool '{tool.Name}' has no associated MCP server",
            };
        }

        var server = await dbContext.MCPServerInfos
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == tool.ServerId && s.TenantId == userContext.TenantId, cancellationToken);

        if (server is null)
        {
            return new ToolExecutionResult
            {
                Success = false,
                ErrorMessage = $"MCP server not found for tool '{tool.Name}'",
            };
        }

        if (server.TransportType == TransportType.Websocket)
        {
            return new ToolExecutionResult
            {
                Success = false,
                ErrorMessage = $"MCP server '{server.DisplayName}' transport 'Websocket' is not yet supported",
            };
        }

        try
        {
            var client = await mcpClientProvider.GetOrCreateClientAsync(server, cancellationToken);

            // Parse arguments into dictionary for MCP SDK
            Dictionary<string, object?>? arguments = null;
            if (!string.IsNullOrWhiteSpace(request.ArgumentsJson))
            {
                arguments = JsonSerializer.Deserialize<Dictionary<string, object?>>(request.ArgumentsJson);
            }

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(ToolTimeout);

            var result = await client.CallToolAsync(tool.Name, arguments, cancellationToken: cts.Token);

            // Extract text content from MCP response
            var outputParts = new List<string>();
            foreach (var content in result.Content)
            {
                if (content is TextContentBlock textBlock)
                {
                    outputParts.Add(textBlock.Text);
                }
                else
                {
                    outputParts.Add(JsonSerializer.Serialize(content));
                }
            }

            var outputJson = outputParts.Count == 1
                ? outputParts[0]
                : JsonSerializer.Serialize(outputParts);

            var isError = result.IsError == true;

            return new ToolExecutionResult
            {
                Success = !isError,
                OutputJson = outputJson,
                ErrorMessage = isError ? outputJson : null,
            };
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new ToolExecutionResult
            {
                Success = false,
                ErrorMessage = $"MCP server '{server.DisplayName}' timed out after {ToolTimeout.TotalSeconds}s",
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Remove the cached client on connection errors so it reconnects next time
            await mcpClientProvider.RemoveClientAsync(server.Id);

            logger.LogError(ex, "MCP tool execution failed for server '{ServerName}', transport {Transport}",
                server.DisplayName, server.TransportType);
            return new ToolExecutionResult
            {
                Success = false,
                ErrorMessage = $"MCP execution failed ({server.TransportType}): {ex.Message}",
            };
        }
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
