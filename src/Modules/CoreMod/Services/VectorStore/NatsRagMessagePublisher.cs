using NATS.Client.Core;
using Share.Models;

namespace CoreMod.Services;

/// <summary>
/// 基于 NATS JetStream 的 RAG 消息发布者
/// </summary>
public class NatsRagMessagePublisher(NatsJetStreamService jetStreamService, ILogger<NatsRagMessagePublisher> logger)
{
    private const string StreamName = "RAG_PROCESSING";
    private const string SubjectName = "rag.ingestion";

    public async Task PublishAsync(RagIngestionMessage message, CancellationToken cancellationToken = default)
    {
        await jetStreamService.EnsureWorkQueueStreamAsync(
            streamName: StreamName,
            subject: SubjectName,
            description: "RAG document ingestion processing stream",
            cancellationToken: cancellationToken,
            maxMsgs: 100000,
            maxAge: TimeSpan.FromDays(7),
            duplicateWindow: TimeSpan.FromMinutes(5)
        );

        var json = JsonSerializer.Serialize(message);
        var data = System.Text.Encoding.UTF8.GetBytes(json);

        var headers = new NatsHeaders
        {
            { "Nats-Msg-Id", message.DocumentId.ToString() }
        };

        var duplicate = await jetStreamService.PublishAsync(
            subject: SubjectName,
            data: data,
            headers: headers,
            cancellationToken: cancellationToken
        );

        if (!duplicate)
        {
            logger.LogInformation("Published RAG ingestion message for document {DocumentId}", message.DocumentId);
        }
        else
        {
            logger.LogDebug("Duplicate message detected for document {DocumentId}, skipped", message.DocumentId);
        }
    }
}