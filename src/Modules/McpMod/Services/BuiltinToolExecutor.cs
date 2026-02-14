using System.Data;
using System.Text.Json;

namespace McpMod.Services;

/// <summary>
/// 内置工具执行器
/// </summary>
public class BuiltinToolExecutor(
    IRagQueryService ragQueryService,
    TenantDbFactory dbContextFactory,
    IUserContext userContext,
    IHttpClientFactory httpClientFactory,
    ILogger<BuiltinToolExecutor> logger
) : IMcpToolExecutor
{
    private const int MaxSqlRows = 200;
    private const string HttpAllowlistGroup = "McpMod";
    private const string HttpAllowlistKey = "HttpAllowlist";

    public async Task<ToolExecutionResult> ExecuteAsync(ToolExecutionRequest request, CancellationToken cancellationToken = default)
    {
        _ = userContext;
        return request.ToolName switch
        {
            "query_knowledge_base" => await ExecuteRagQueryAsync(request, cancellationToken),
            "execute_sql_query" => await ExecuteSqlQueryAsync(request, cancellationToken),
            "http_request" => await ExecuteHttpRequestAsync(request, cancellationToken),
            _ => new ToolExecutionResult
            {
                Success = false,
                ErrorMessage = $"Tool '{request.ToolName}' not implemented",
            },
        };
    }

    private async Task<ToolExecutionResult> ExecuteRagQueryAsync(ToolExecutionRequest request, CancellationToken cancellationToken)
    {
        if (!TryParseArguments(request.ArgumentsJson, out var document, out var error))
        {
            return Failed(error);
        }

        var root = document!.RootElement;
        var query = root.TryGetProperty("query", out var q) ? q.GetString() : null;
        if (string.IsNullOrWhiteSpace(query))
        {
            return Failed("Missing 'query' argument");
        }

        int? topK = root.TryGetProperty("topK", out var topKElem) && topKElem.TryGetInt32(out var tk) ? tk : null;
        Guid? collectionId = root.TryGetProperty("collectionId", out var collectionElem) && collectionElem.TryGetGuid(out var cid)
            ? cid
            : null;

        var result = await ragQueryService.QueryAsync(new RagQueryRequest
        {
            Query = query,
            TopK = topK ?? 5,
            CollectionId = collectionId,
        }, cancellationToken);

        // 直接返回 CoreMod.Services 中的 RagQueryResult
        return Success(result);
    }

    private async Task<ToolExecutionResult> ExecuteSqlQueryAsync(ToolExecutionRequest request, CancellationToken cancellationToken)
    {
        if (!TryParseArguments(request.ArgumentsJson, out var document, out var error))
        {
            return Failed(error);
        }

        var root = document!.RootElement;
        var sql = root.TryGetProperty("sql", out var sqlElem) ? sqlElem.GetString() : null;
        if (string.IsNullOrWhiteSpace(sql))
        {
            return Failed("Missing 'sql' argument");
        }

        if (!IsSafeReadOnlySql(sql))
        {
            return Failed("Only read-only SELECT queries are allowed");
        }

        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var connection = dbContext.Database.GetDbConnection();
        await using var _ = connection;
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.CommandType = CommandType.Text;
        command.CommandTimeout = 5;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var rows = new List<Dictionary<string, object?>>();
        var columns = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();
        var count = 0;
        while (await reader.ReadAsync(cancellationToken))
        {
            var row = new Dictionary<string, object?>();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                var value = reader.GetValue(i);
                row[columns[i]] = value == DBNull.Value ? null : value;
            }
            rows.Add(row);
            count++;
            if (count >= MaxSqlRows)
            {
                break;
            }
        }

        var payload = new
        {
            rows,
            columns,
            truncated = count >= MaxSqlRows,
        };

        return Success(payload);
    }

    private async Task<ToolExecutionResult> ExecuteHttpRequestAsync(ToolExecutionRequest request, CancellationToken cancellationToken)
    {
        if (!TryParseArguments(request.ArgumentsJson, out var document, out var error))
        {
            return Failed(error);
        }

        var root = document!.RootElement;
        var url = root.TryGetProperty("url", out var urlElem) ? urlElem.GetString() : null;
        var method = root.TryGetProperty("method", out var methodElem) ? methodElem.GetString() : "GET";
        if (string.IsNullOrWhiteSpace(url))
        {
            return Failed("Missing 'url' argument");
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
        {
            return Failed("Only HTTPS requests are allowed");
        }

        var allowedHosts = await GetHttpAllowlistAsync(cancellationToken);
        if (allowedHosts.Count == 0 || !allowedHosts.Contains(uri.Host))
        {
            return Failed("Host is not in allowlist");
        }

        var requestMessage = new HttpRequestMessage(new HttpMethod(method ?? "GET"), uri);
        if (root.TryGetProperty("headers", out var headersElem) && headersElem.ValueKind == JsonValueKind.Object)
        {
            foreach (var header in headersElem.EnumerateObject())
            {
                requestMessage.Headers.TryAddWithoutValidation(header.Name, header.Value.GetString());
            }
        }

        if (root.TryGetProperty("body", out var bodyElem) && bodyElem.ValueKind != JsonValueKind.Null)
        {
            var bodyText = bodyElem.ValueKind == JsonValueKind.String ? bodyElem.GetString() : bodyElem.GetRawText();
            requestMessage.Content = new StringContent(bodyText ?? string.Empty, System.Text.Encoding.UTF8, "application/json");
        }

        var client = httpClientFactory.CreateClient();
        using var response = await client.SendAsync(requestMessage, cancellationToken);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        var payload = new
        {
            status = (int)response.StatusCode,
            headers = response.Headers.ToDictionary(h => h.Key, h => string.Join(",", h.Value)),
            body = responseText,
        };

        return Success(payload);
    }

    private async Task<HashSet<string>> GetHttpAllowlistAsync(CancellationToken cancellationToken)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var config = await dbContext.SystemConfigs
            .AsNoTracking()
            .FirstOrDefaultAsync(
                q => q.GroupName == HttpAllowlistGroup
                    && q.Key == HttpAllowlistKey
                    && q.Valid,
                cancellationToken
            );

        if (config is null || string.IsNullOrWhiteSpace(config.Value))
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        try
        {
            var json = JsonSerializer.Deserialize<string[]>(config.Value);
            if (json is { Length: > 0 })
            {
                return new HashSet<string>(json, StringComparer.OrdinalIgnoreCase);
            }
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Invalid HTTP allowlist json");
        }

        return config.Value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static bool TryParseArguments(string? argumentsJson, out JsonDocument? document, out string? error)
    {
        document = null;
        error = null;
        if (string.IsNullOrWhiteSpace(argumentsJson))
        {
            document = JsonDocument.Parse("{}");
            return true;
        }

        try
        {
            document = JsonDocument.Parse(argumentsJson);
            return true;
        }
        catch (JsonException ex)
        {
            error = $"Invalid arguments json: {ex.Message}";
            return false;
        }
    }

    private static bool IsSafeReadOnlySql(string sql)
    {
        var trimmed = sql.TrimStart();
        if (!trimmed.StartsWith("select", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (trimmed.Contains(';'))
        {
            return false;
        }

        var lower = trimmed.ToLowerInvariant();
        return !lower.Contains(" insert ")
            && !lower.Contains(" update ")
            && !lower.Contains(" delete ")
            && !lower.Contains(" drop ")
            && !lower.Contains(" alter ")
            && !lower.Contains(" create ")
            && !lower.Contains(" truncate ");
    }

    private static ToolExecutionResult Success<T>(T payload)
    {
        return new ToolExecutionResult
        {
            Success = true,
            OutputJson = JsonSerializer.Serialize(payload),
        };
    }

    private static ToolExecutionResult Failed(string? message)
    {
        return new ToolExecutionResult
        {
            Success = false,
            ErrorMessage = message,
        };
    }
}
