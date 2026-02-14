using ModelContextProtocol.Client;
using System.Collections.Concurrent;

namespace McpMod.Services;

/// <summary>
/// Manages MCP client connections per server, supporting Http/SSE and stdio transports.
/// Clients are cached and reused per server ID to avoid repeated handshakes.
/// </summary>
public sealed class McpClientProvider(
    IHttpClientFactory httpClientFactory,
    ILogger<McpClientProvider> logger
) : IAsyncDisposable
{
    private readonly ConcurrentDictionary<Guid, McpClient> _clients = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    /// <summary>
    /// Get or create an MCP client for the given server configuration.
    /// </summary>
    public async Task<McpClient> GetOrCreateClientAsync(MCPServerInfo server, CancellationToken cancellationToken = default)
    {
        if (_clients.TryGetValue(server.Id, out var existing))
        {
            return existing;
        }

        await _lock.WaitAsync(cancellationToken);
        try
        {
            // Double-check after acquiring lock
            if (_clients.TryGetValue(server.Id, out existing))
            {
                return existing;
            }

            var transport = CreateTransport(server);
            var client = await McpClient.CreateAsync(transport, cancellationToken: cancellationToken);

            _clients[server.Id] = client;
            logger.LogInformation("Created MCP client for server '{ServerName}' with transport {Transport}",
                server.DisplayName, server.TransportType);

            return client;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Remove and dispose a cached client (e.g. when server config changes).
    /// </summary>
    public async Task RemoveClientAsync(Guid serverId)
    {
        if (_clients.TryRemove(serverId, out var client))
        {
            await client.DisposeAsync();
        }
    }

    private IClientTransport CreateTransport(MCPServerInfo server)
    {
        return server.TransportType switch
        {
            TransportType.Http or TransportType.SSE => CreateHttpTransport(server),
            TransportType.Stdio => CreateStdioTransport(server),
            _ => throw new NotSupportedException($"Transport type '{server.TransportType}' is not supported"),
        };
    }

    private HttpClientTransport CreateHttpTransport(MCPServerInfo server)
    {
        if (string.IsNullOrWhiteSpace(server.Endpoint))
        {
            throw new InvalidOperationException($"MCP server '{server.DisplayName}' has no endpoint configured");
        }

        var httpClient = httpClientFactory.CreateClient("McpTransport");
        ApplyAuth(httpClient, server);

        var options = new HttpClientTransportOptions
        {
            Endpoint = new Uri(server.Endpoint),
            TransportMode = server.TransportType == TransportType.SSE
                ? HttpTransportMode.Sse
                : HttpTransportMode.AutoDetect,
        };

        return new HttpClientTransport(options, httpClient);
    }

    private static StdioClientTransport CreateStdioTransport(MCPServerInfo server)
    {
        if (string.IsNullOrWhiteSpace(server.ExecutablePath))
        {
            throw new InvalidOperationException($"MCP server '{server.DisplayName}' has no executable path configured for stdio transport");
        }

        var options = new StdioClientTransportOptions
        {
            Name = server.DisplayName,
            Command = server.ExecutablePath,
            Arguments = server.Arguments ?? [],
        };

        return new StdioClientTransport(options);
    }

    private static void ApplyAuth(HttpClient client, MCPServerInfo server)
    {
        if (string.IsNullOrWhiteSpace(server.AuthValue))
        {
            return;
        }

        switch (server.AuthType)
        {
            case AuthType.ApiKey:
                client.DefaultRequestHeaders.Add("X-API-Key", server.AuthValue);
                break;
            case AuthType.Token:
            case AuthType.OAuth:
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", server.AuthValue);
                break;
        }
    }

    public async ValueTask DisposeAsync()
    {
        List<Exception>? exceptions = null;
        foreach (var (_, client) in _clients)
        {
            try
            {
                await client.DisposeAsync();
            }
            catch (Exception ex)
            {
                exceptions ??= [];
                exceptions.Add(ex);
            }
        }
        _clients.Clear();
        _lock.Dispose();

        if (exceptions is { Count: > 0 })
        {
            throw new AggregateException(exceptions);
        }
    }
}
