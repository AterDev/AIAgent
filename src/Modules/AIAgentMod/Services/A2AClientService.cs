using AIAgentMod.Models;
using SharpA2A.Core;
using System.Text.Json;

namespace AIAgentMod.Services;

/// <summary>
/// Agent-to-Agent (A2A) protocol client service.
/// Enables sending tasks to remote agents via the Google A2A protocol.
/// </summary>
public class A2AClientService(
    TenantDbFactory dbContextFactory,
    IUserContext userContext,
    IHttpClientFactory httpClientFactory,
    ILogger<A2AClientService> logger
)
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(300);

    /// <summary>
    /// Send a message to a remote agent via A2A protocol and get the response.
    /// </summary>
    public async Task<A2ATaskResult> SendMessageAsync(
        Guid remoteAgentId,
        string message,
        string? taskId = null,
        string? contextId = null,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var remoteAgent = await dbContext.A2ARemoteAgents
            .AsNoTracking()
            .FirstOrDefaultAsync(
                a => a.Id == remoteAgentId
                    && a.IsEnabled
                    && a.TenantId == userContext.TenantId,
                cancellationToken);

        if (remoteAgent is null)
        {
            return new A2ATaskResult
            {
                Success = false,
                ErrorMessage = $"A2A remote agent '{remoteAgentId}' not found or disabled",
            };
        }

        return await SendMessageToAgentAsync(remoteAgent, message, taskId, contextId, cancellationToken);
    }

    /// <summary>
    /// Send a message to a remote agent by URL (without database lookup).
    /// </summary>
    public async Task<A2ATaskResult> SendMessageToUrlAsync(
        string agentUrl,
        string message,
        string? authToken = null,
        CancellationToken cancellationToken = default)
    {
        var remoteAgent = new A2ARemoteAgent
        {
            Name = "Direct",
            AgentUrl = agentUrl,
            AuthType = string.IsNullOrWhiteSpace(authToken) ? AuthType.None : AuthType.Token,
            AuthValue = authToken,
        };

        return await SendMessageToAgentAsync(remoteAgent, message, null, null, cancellationToken);
    }

    /// <summary>
    /// Discover a remote agent's capabilities by fetching its Agent Card.
    /// </summary>
    public async Task<A2AAgentCardResult> GetAgentCardAsync(
        string agentUrl,
        string? authToken = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var httpClient = CreateHttpClientForAgent(
                agentUrl,
                string.IsNullOrWhiteSpace(authToken) ? AuthType.None : AuthType.Token,
                authToken);

            var resolver = new A2ACardResolver(httpClient);
            var card = await resolver.GetAgentCardAsync(cancellationToken);

            return new A2AAgentCardResult
            {
                Success = true,
                Name = card.Name,
                Description = card.Description,
                Skills = card.Skills?.Select(s => s.Name).ToList() ?? [],
                CardJson = JsonSerializer.Serialize(card),
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch agent card from '{AgentUrl}'", agentUrl);
            return new A2AAgentCardResult
            {
                Success = false,
                ErrorMessage = $"Failed to fetch agent card: {ex.Message}",
            };
        }
    }

    /// <summary>
    /// Get the status of a previously sent task.
    /// </summary>
    public async Task<A2ATaskResult> GetTaskAsync(
        Guid remoteAgentId,
        string taskId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var remoteAgent = await dbContext.A2ARemoteAgents
            .AsNoTracking()
            .FirstOrDefaultAsync(
                a => a.Id == remoteAgentId
                    && a.IsEnabled
                    && a.TenantId == userContext.TenantId,
                cancellationToken);

        if (remoteAgent is null)
        {
            return new A2ATaskResult
            {
                Success = false,
                ErrorMessage = $"A2A remote agent '{remoteAgentId}' not found or disabled",
            };
        }

        try
        {
            var a2aClient = CreateA2AClient(remoteAgent);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(DefaultTimeout);

            var stopwatch = Stopwatch.StartNew();
            var agentTask = await a2aClient.GetTaskAsync(taskId).WaitAsync(cts.Token);
            stopwatch.Stop();
            return ExtractTaskResult(agentTask, stopwatch);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get task '{TaskId}' from A2A agent '{AgentName}'",
                taskId, remoteAgent.Name);
            return new A2ATaskResult
            {
                Success = false,
                ErrorMessage = $"Failed to get task: {ex.Message}",
            };
        }
    }

    private async Task<A2ATaskResult> SendMessageToAgentAsync(
        A2ARemoteAgent remoteAgent,
        string message,
        string? taskId,
        string? contextId,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var a2aClient = CreateA2AClient(remoteAgent);

            var messageTaskId = taskId ?? Guid.NewGuid().ToString("N");
            var sendParams = new MessageSendParams
            {
                Message = new Message
                {
                    Role = MessageRole.User,
                    Parts = [new TextPart { Text = message }],
                    TaskId = messageTaskId,
                    ContextId = contextId ?? Guid.NewGuid().ToString("N"),
                },
            };

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(DefaultTimeout);

            // SendMessageAsync returns A2AResponse (may not contain full task data)
            _ = await a2aClient.SendMessageAsync(sendParams).WaitAsync(cts.Token);

            // Retrieve the full task result
            var agentTask = await PollTaskAsync(a2aClient, messageTaskId, cts.Token);

            stopwatch.Stop();
            return ExtractTaskResult(agentTask, stopwatch);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new A2ATaskResult
            {
                Success = false,
                ErrorMessage = $"A2A remote agent '{remoteAgent.Name}' timed out after {DefaultTimeout.TotalSeconds}s",
                DurationMs = (int)stopwatch.ElapsedMilliseconds,
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "A2A task execution failed for agent '{AgentName}' at {AgentUrl}",
                remoteAgent.Name, remoteAgent.AgentUrl);
            return new A2ATaskResult
            {
                Success = false,
                ErrorMessage = $"A2A execution failed: {ex.Message}",
                DurationMs = (int)stopwatch.ElapsedMilliseconds,
            };
        }
    }

    private static A2ATaskResult ExtractTaskResult(AgentTask agentTask, Stopwatch stopwatch)
    {
        var responseParts = new List<string>();

        // Extract from artifacts
        if (agentTask.Artifacts is { Count: > 0 })
        {
            foreach (var artifact in agentTask.Artifacts)
            {
                foreach (var part in artifact.Parts)
                {
                    if (part is TextPart textPart)
                    {
                        responseParts.Add(textPart.Text);
                    }
                }
            }
        }

        // Fallback: check history for agent messages
        if (responseParts.Count == 0 && agentTask.History is { Count: > 0 })
        {
            foreach (var historyMessage in agentTask.History.Where(m => m.Role == MessageRole.Agent))
            {
                foreach (var part in historyMessage.Parts)
                {
                    if (part is TextPart textPart)
                    {
                        responseParts.Add(textPart.Text);
                    }
                }
            }
        }

        var responseContent = responseParts.Count > 0
            ? string.Join("\n", responseParts)
            : string.Empty;

        return new A2ATaskResult
        {
            Success = true,
            TaskId = agentTask.Id,
            ContextId = agentTask.ContextId,
            Status = agentTask.Status?.State.ToString() ?? "unknown",
            Content = responseContent,
            DurationMs = (int)stopwatch.ElapsedMilliseconds,
        };
    }

    private static async Task<AgentTask> PollTaskAsync(
        A2AClient a2aClient,
        string taskId,
        CancellationToken cancellationToken)
    {
        while (true)
        {
            var agentTask = await a2aClient.GetTaskAsync(taskId).WaitAsync(cancellationToken);
            var state = agentTask.Status?.State.ToString();

            if (IsTerminalState(state))
            {
                return agentTask;
            }

            await Task.Delay(PollInterval, cancellationToken);
        }
    }

    private static bool IsTerminalState(string? state)
    {
        if (string.IsNullOrWhiteSpace(state))
        {
            return false;
        }

        return state.Equals("completed", StringComparison.OrdinalIgnoreCase)
            || state.Equals("failed", StringComparison.OrdinalIgnoreCase)
            || state.Equals("canceled", StringComparison.OrdinalIgnoreCase)
            || state.Equals("cancelled", StringComparison.OrdinalIgnoreCase);
    }

    private A2AClient CreateA2AClient(A2ARemoteAgent remoteAgent)
    {
        var httpClient = CreateHttpClientForAgent(remoteAgent.AgentUrl, remoteAgent.AuthType, remoteAgent.AuthValue);
        return new A2AClient(httpClient);
    }

    private HttpClient CreateHttpClientForAgent(string agentUrl, AuthType authType, string? authValue)
    {
        var client = CreateAuthenticatedClient(authType, authValue);
        client.BaseAddress = new Uri(agentUrl.TrimEnd('/') + "/");
        return client;
    }

    private HttpClient CreateAuthenticatedClient(AuthType authType, string? authValue)
    {
        var client = httpClientFactory.CreateClient("A2A");

        if (!string.IsNullOrWhiteSpace(authValue))
        {
            switch (authType)
            {
                case AuthType.ApiKey:
                    client.DefaultRequestHeaders.Add("X-API-Key", authValue);
                    break;
                case AuthType.Token:
                case AuthType.OAuth:
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authValue);
                    break;
            }
        }

        return client;
    }
}
