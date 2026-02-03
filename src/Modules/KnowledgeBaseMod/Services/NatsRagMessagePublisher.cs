using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using Share.Models;
using Share.Services;
using System.Text.Json;

namespace KnowledgeBaseMod.Services;

/// <summary>
/// 基于 NATS JetStream 的 RAG 消息发布者
/// </summary>
public class NatsRagMessagePublisher(INatsConnection natsConnection, ILogger<NatsRagMessagePublisher> logger)
{
    private const string StreamName = "RAG_PROCESSING";
    private const string SubjectName = "rag.ingestion";

    private INatsJSContext? _jsContext;

    /// <summary>
    /// 确保 Stream 已创建
    /// </summary>
    private async Task<INatsJSContext> EnsureStreamAsync(CancellationToken cancellationToken)
    {
        if (_jsContext != null)
        {
            return _jsContext;
        }

        _jsContext = new NatsJSContext(natsConnection);

        try
        {
            // 尝试获取 Stream，如果不存在则创建
            await _jsContext.GetStreamAsync(StreamName, cancellationToken: cancellationToken);
        }
        catch (NatsJSApiException ex) when (ex.Error.Code == 404)
        {
            // Stream 不存在，创建它
            var streamConfig = new StreamConfig
            {
                Name = StreamName,
                Subjects = [SubjectName],
                Storage = StreamConfigStorage.File,
                Retention = StreamConfigRetention.Workqueue, // WorkQueue 模式确保消息只被消费一次
                MaxAge = TimeSpan.FromDays(7), // 保留7天
                MaxMsgs = 100000,
                Discard = StreamConfigDiscard.Old,
                DuplicateWindow = TimeSpan.FromMinutes(5), // 5分钟内重复消息去重
            };

            await _jsContext.CreateStreamAsync(streamConfig, cancellationToken: cancellationToken);
        }

        return _jsContext;
    }

    /// <summary>
    /// 发布文档处理消息
    /// </summary>
    public async Task PublishAsync(RagIngestionMessage message, CancellationToken cancellationToken = default)
    {
        var js = await EnsureStreamAsync(cancellationToken);

        var json = JsonSerializer.Serialize(message);
        var data = System.Text.Encoding.UTF8.GetBytes(json);

        // 使用文档 ID 作为消息 ID 实现去重
        var headers = new NatsHeaders
        {
            { "Nats-Msg-Id", message.DocumentId.ToString() }
        };

        var ack = await js.PublishAsync(
            subject: SubjectName,
            data: data,
            headers: headers,
            cancellationToken: cancellationToken
        );

        if (!ack.Duplicate)
        {
            logger.LogInformation("Published RAG ingestion message for document {DocumentId}", message.DocumentId);
        }
        else
        {
            logger.LogDebug("Duplicate message detected for document {DocumentId}, skipped", message.DocumentId);
        }
    }
}
