using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using Share.Models;
using System.Text.Json;
using KnowledgeBaseMod.Services;

namespace FileProcessorService.Workers;

/// <summary>
/// RAG 文档处理消费者 - 直接消费 NATS JetStream 消息
/// </summary>
public class RagIngestionConsumer(
    INatsConnection natsConnection,
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
            var jsContext = new NatsJSContext(natsConnection);

            // 确保流已创建
            await EnsureStreamAsync(jsContext, stoppingToken);

            // 确保 Consumer 已创建
            await EnsureConsumerAsync(jsContext, stoppingToken);

            // 开始消费消息
            await ConsumeMessagesAsync(jsContext, stoppingToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fatal error in RagIngestionConsumer");
            throw;
        }
    }

    private async Task EnsureStreamAsync(NatsJSContext jsContext, CancellationToken cancellationToken)
    {
        try
        {
            await jsContext.GetStreamAsync(StreamName, cancellationToken: cancellationToken);
            logger.LogInformation("Stream {StreamName} already exists", StreamName);
        }
        catch (NatsJSApiException ex) when (ex.Error?.Code == 404)
        {
            // 流不存在，创建它
            var streamConfig = new StreamConfig
            {
                Name = StreamName,
                Description = "RAG document ingestion processing stream",
                Subjects = new[] { SubjectName },
                MaxAge = TimeSpan.FromDays(7),
                MaxBytes = 1024 * 1024 * 100, // 100 MB
                Storage = StreamConfigStorage.File,
                Retention = StreamConfigRetention.Workqueue, // WorkQueue 模式确保消息只被消费一次
                Discard = StreamConfigDiscard.Old,
                DuplicateWindow = TimeSpan.FromMinutes(5),
            };

            await jsContext.CreateStreamAsync(streamConfig, cancellationToken: cancellationToken);
            logger.LogInformation("Created stream {StreamName}", StreamName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking/creating stream {StreamName}", StreamName);
            throw;
        }
    }

    private async Task EnsureConsumerAsync(NatsJSContext jsContext, CancellationToken cancellationToken)
    {
        try
        {
            var consumer = await jsContext.GetConsumerAsync(StreamName, ConsumerName, cancellationToken: cancellationToken);
            logger.LogInformation("Consumer {ConsumerName} already exists", ConsumerName);
        }
        catch (NatsJSApiException ex) when (ex.Error.Code == 404)
        {
            // Consumer 不存在，创建它
            var consumerConfig = new ConsumerConfig
            {
                Name = ConsumerName,
                DurableName = ConsumerName,
                FilterSubject = SubjectName,
                AckPolicy = ConsumerConfigAckPolicy.Explicit,
                MaxDeliver = 3, // 最多重试 3 次
                AckWait = TimeSpan.FromMinutes(5), // 5 分钟内必须确认
                MaxAckPending = 10, // 最多 10 条未确认消息
            };

            await jsContext.CreateConsumerAsync(StreamName, consumerConfig, cancellationToken: cancellationToken);
            logger.LogInformation("Created consumer {ConsumerName}", ConsumerName);
        }
    }

    private async Task ConsumeMessagesAsync(NatsJSContext jsContext, CancellationToken cancellationToken)
    {
        var consumer = await jsContext.GetConsumerAsync(StreamName, ConsumerName, cancellationToken: cancellationToken);

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
                await msg.NakAsync(delay: TimeSpan.FromSeconds(30), cancellationToken: cancellationToken);
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
