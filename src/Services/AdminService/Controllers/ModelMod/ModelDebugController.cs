using AdminService.Services;
using ModelMod.Models.ModelDebugDtos;
using ModelMod.Services;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace AdminService.Controllers.ModelMod;

public class ModelDebugController(
    Localizer localizer,
    ModelDebugService debugService,
    DebugSessionRegistry sessionRegistry,
    ILogger<ModelDebugController> logger
) : RestControllerBase(localizer)
{
    [HttpPost]
    public async Task<ActionResult<ModelDebugResponse>> ChatAsync(ModelDebugRequest request, CancellationToken cancellationToken)
    {
        var response = await debugService.ChatAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("stream")]
    [Produces("text/event-stream")]
    public async Task StreamAsync([FromBody] ModelDebugRequest request, CancellationToken cancellationToken)
    {
        var session = await debugService.CreateStreamSessionAsync(request, cancellationToken);
        var cts = sessionRegistry.Create(session.RequestId);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, HttpContext.RequestAborted, cancellationToken);
        var linkedToken = linkedCts.Token;

        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("X-Accel-Buffering", "no");
        Response.ContentType = "text/event-stream";

        var buffer = new StringBuilder();
        var usage = new Share.Services.UsageStats();
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await WriteEventAsync(new ModelDebugStreamEvent
            {
                Type = "meta",
                RequestId = session.RequestId,
            }, linkedToken);

            await foreach (var chunk in session.Stream.WithCancellation(linkedToken))
            {
                if (!string.IsNullOrWhiteSpace(chunk.ErrorMessage))
                {
                    await WriteEventAsync(new ModelDebugStreamEvent
                    {
                        Type = "error",
                        RequestId = session.RequestId,
                        Error = chunk.ErrorMessage,
                    }, linkedToken);
                    return;
                }

                if (chunk.Usage != null)
                {
                    usage = new Share.Services.UsageStats
                    {
                        PromptTokens = chunk.Usage.PromptTokens,
                        CompletionTokens = chunk.Usage.CompletionTokens,
                        TotalTokens = chunk.Usage.TotalTokens,
                    };
                }

                if (!string.IsNullOrWhiteSpace(chunk.Delta))
                {
                    buffer.Append(chunk.Delta);
                    await WriteEventAsync(new ModelDebugStreamEvent
                    {
                        Type = "delta",
                        RequestId = session.RequestId,
                        Delta = chunk.Delta,
                    }, linkedToken);
                }

                if (chunk.IsFinal)
                {
                    break;
                }
            }

            stopwatch.Stop();
            await WriteEventAsync(new ModelDebugStreamEvent
            {
                Type = "final",
                RequestId = session.RequestId,
                Final = new ModelDebugResponse
                {
                    Content = buffer.ToString(),
                    Model = session.ModelName,
                    PromptTokens = usage.PromptTokens,
                    CompletionTokens = usage.CompletionTokens,
                    TotalTokens = usage.TotalTokens,
                    FinishReason = "stop",
                    DurationMs = (int)stopwatch.ElapsedMilliseconds,
                }
            }, linkedToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Model debug stream canceled: {RequestId}", session.RequestId);
        }
        finally
        {
            sessionRegistry.Remove(session.RequestId);
        }
    }

    [HttpPost("stop/{requestId}")]
    public ActionResult<bool> StopAsync([FromRoute] string requestId)
    {
        return Ok(sessionRegistry.TryCancel(requestId));
    }

    private async Task WriteEventAsync(ModelDebugStreamEvent payload, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await Response.WriteAsync($"data: {json}\n\n", cancellationToken);
        await Response.Body.FlushAsync(cancellationToken);
    }
}
