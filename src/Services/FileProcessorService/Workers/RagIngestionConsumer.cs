using NATS.Client.Core;
using Share.Models;
using System.Text.Json;
using CoreMod.Services;
using CoreMod.Services.RagIngestion;

namespace FileProcessorService.Workers;

/// <summary>
/// RAG 文档处理消费者 - 直接消费 NATS JetStream 消息
/// </summary>
public class RagIngestionConsumer(
    NatsJetStreamService jetStreamService,
    IServiceProvider serviceProvider,
    ILogger<RagIngestionConsumer> logger) : BackgroundService
{
    private const string StreamName = "RAG_PROCESSING";
    private const string SubjectName = "rag.ingestion";
    private const string ConsumerName = "file-processor";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("RagIngestionConsumer starting...");

        try
        {
            await jetStreamService.EnsureWorkQueueStreamAsync(
                streamName: StreamName,
                subject: SubjectName,
                description: "RAG document ingestion processing stream",
                cancellationToken: stoppingToken,
                maxBytes: 1024L * 1024L * 100L,
                maxAge: TimeSpan.FromDays(7),
                duplicateWindow: TimeSpan.FromMinutes(5)
            );

            await jetStreamService.EnsureDurableConsumerAsync(
                streamName: StreamName,
                consumerName: ConsumerName,
                filterSubject: SubjectName,
                cancellationToken: stoppingToken,
                maxDeliver: 3,
                ackWait: TimeSpan.FromMinutes(5),
                maxAckPending: 10
            );

            // 开始消费消息
            await ConsumeMessagesAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fatal error in RagIngestionConsumer");
            throw;
        }
    }

    private async Task ConsumeMessagesAsync(CancellationToken cancellationToken)
    {
        var consumer = await jetStreamService.GetConsumerAsync(StreamName, ConsumerName, cancellationToken);

        await foreach (var msg in consumer.ConsumeAsync<NatsMemoryOwner<byte>>(cancellationToken: cancellationToken))
        {
            try
            {
                var json = System.Text.Encoding.UTF8.GetString(msg.Data.Span);
                var message = JsonSerializer.Deserialize<RagIngestionMessage>(json);

                if (message == null)
                {
                    logger.LogWarning("Failed to deserialize message, skipping");
                    await msg.AckAsync(cancellationToken: cancellationToken);
                    continue;
                }

                logger.LogInformation("Processing document {DocumentId} from collection {CollectionId}",
                    message.DocumentId, message.CollectionId);

                // 在独立的作用域中处理消息，确保正确的 DI 生命周期
                await ProcessMessageAsync(message, cancellationToken);

                // 确认消息
                await msg.AckAsync(cancellationToken: cancellationToken);
                logger.LogInformation("Successfully processed document {DocumentId}", message.DocumentId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing message, will retry");
                // NAK 消息，让它重新投递
                await msg.NakAsync(
                    new NATS.Client.JetStream.AckOpts { NakDelay = TimeSpan.FromSeconds(30) },
                    cancellationToken: cancellationToken);
            }
        }
    }

    private async Task ProcessMessageAsync(RagIngestionMessage message, CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var ingestionService = scope.ServiceProvider.GetRequiredService<RagIngestionService>();

        // 执行完整的 RAG 处理流程：解析 → 分块 → 向量化 → 存储
        await ingestionService.IngestAsync(
            documentId: message.DocumentId,
            tenantId: message.TenantId,
            contentText: null,
            cancellationToken: cancellationToken
        );
    }
}
