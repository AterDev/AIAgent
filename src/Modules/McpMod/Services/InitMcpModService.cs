using System.Net.Http.Json;
using McpMod.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace McpMod.Services;

/// <summary>
/// module init host service
/// </summary>
public class InitMcpModService(
    IServiceProvider serviceProvider,
    IHttpClientFactory httpClientFactory,
    ILogger<InitMcpModService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            logger.LogInformation("McpMod initializing...");
            await SyncServersAsync(stoppingToken);

            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await SyncServersAsync(stoppingToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "McpMod initialization failed");
            return;
        }
        finally
        {
        }
    }

    private async Task SyncServersAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DefaultDbContext>();

        var servers = await dbContext.MCPServerInfos
            .AsNoTracking()
            .Where(q => q.TransportType == TransportType.Http)
            .ToListAsync(cancellationToken);

        if (servers.Count == 0)
        {
            return;
        }

        foreach (var server in servers)
        {
            if (string.IsNullOrWhiteSpace(server.Endpoint))
            {
                continue;
            }

            var endpoint = server.Endpoint.TrimEnd('/');
            await ProbeHealthAsync(endpoint, cancellationToken);

            var toolDefinitions = await FetchToolDefinitionsAsync(endpoint, cancellationToken);
            if (toolDefinitions is null || toolDefinitions.Count == 0)
            {
                continue;
            }

            foreach (var tool in toolDefinitions)
            {
                if (string.IsNullOrWhiteSpace(tool.Name))
                {
                    continue;
                }

                var existing = await dbContext.McpTools
                    .FirstOrDefaultAsync(
                        q => q.TenantId == server.TenantId
                            && q.Name == tool.Name
                            && q.Version == tool.Version,
                        cancellationToken
                    );

                if (existing is null)
                {
                    dbContext.McpTools.Add(new McpTool
                    {
                        Name = tool.Name,
                        Description = tool.Description ?? string.Empty,
                        SchemaJson = tool.SchemaJson,
                        Version = tool.Version,
                        ToolType = McpToolType.External,
                        IsEnabled = true,
                        ServerId = server.Id,
                        TenantId = server.TenantId,
                    });
                }
                else
                {
                    existing.Description = tool.Description ?? string.Empty;
                    existing.SchemaJson = tool.SchemaJson;
                    existing.ToolType = McpToolType.External;
                    existing.IsEnabled = true;
                    existing.ServerId = server.Id;
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task ProbeHealthAsync(string endpoint, CancellationToken cancellationToken)
    {
        try
        {
            var client = httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            using var response = await client.GetAsync($"{endpoint}/health", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("MCP server health check failed: {Endpoint} {Status}", endpoint, response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "MCP server health check error: {Endpoint}", endpoint);
        }
    }

    private async Task<List<ToolDefinitionDto>?> FetchToolDefinitionsAsync(string endpoint, CancellationToken cancellationToken)
    {
        try
        {
            var client = httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            return await client.GetFromJsonAsync<List<ToolDefinitionDto>>($"{endpoint}/tools", cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to fetch MCP tools from {Endpoint}", endpoint);
            return null;
        }
    }
}