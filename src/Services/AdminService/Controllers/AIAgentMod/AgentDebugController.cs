using AdminService.Services;
using AIAgentMod.Models.AgentDebugDtos;
using AIAgentMod.Services;
using System.Text.Json;

namespace AdminService.Controllers.AIAgentMod;

public class AgentDebugController(
    Localizer localizer,
    AgentDebugService debugService,
    DebugSessionRegistry sessionRegistry,
    ILogger<AgentDebugController> logger
) : RestControllerBase(localizer)
{
    [HttpPost("stream")]
    public async Task StreamAsync([FromBody] AgentDebugRequest request, CancellationToken cancellationToken)
    {
        var requestId = string.IsNullOrWhiteSpace(request.RequestId)
            ? Guid.NewGuid().ToString("N")
            : request.RequestId;

        request.RequestId = requestId;

        var cts = sessionRegistry.Create(requestId);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, HttpContext.RequestAborted, cancellationToken);
        var linkedToken = linkedCts.Token;

        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("X-Accel-Buffering", "no");
        Response.ContentType = "text/event-stream";

        try
        {
            await WriteEventAsync(new AgentDebugStreamEvent
            {
                Type = "meta",
                RequestId = requestId,
            }, linkedToken);

            await debugService.ExecuteStreamAsync(request, evt => WriteEventAsync(evt, linkedToken), linkedToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Agent debug stream canceled: {RequestId}", requestId);
        }
        finally
        {
            sessionRegistry.Remove(requestId);
        }
    }

    [HttpPost("stop/{requestId}")]
    public ActionResult<bool> StopAsync([FromRoute] string requestId)
    {
        return Ok(sessionRegistry.TryCancel(requestId));
    }

    private async Task WriteEventAsync(AgentDebugStreamEvent payload, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await Response.WriteAsync($"data: {json}\n\n", cancellationToken);
        await Response.Body.FlushAsync(cancellationToken);
    }
}
