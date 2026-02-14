using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;

namespace Share.Implement;

/// <summary>
/// NATS JetStream 操作封装。
/// </summary>
public class NatsJetStreamService(
    INatsConnection natsConnection,
    ILogger<NatsJetStreamService> logger
)
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private INatsJSContext? _context;

    public Task<INatsJSContext> GetContextAsync(CancellationToken cancellationToken = default)
    {
        if (_context != null)
        {
            return Task.FromResult(_context);
        }

        _context = new NatsJSContext(natsConnection);
        return Task.FromResult(_context);
    }

    public async Task EnsureWorkQueueStreamAsync(
        string streamName,
        string subject,
        string? description = null,
        CancellationToken cancellationToken = default,
        long? maxBytes = null,
        int? maxMsgs = null,
        TimeSpan? maxAge = null,
        TimeSpan? duplicateWindow = null)
    {
        var context = await GetContextAsync(cancellationToken);

        await _lock.WaitAsync(cancellationToken);
        try
        {
            try
            {
                await context.GetStreamAsync(streamName, cancellationToken: cancellationToken);
                return;
            }
            catch (NatsJSApiException ex) when (ex.Error?.Code == 404)
            {
                var config = new StreamConfig
                {
                    Name = streamName,
                    Description = description,
                    Subjects = [subject],
                    Storage = StreamConfigStorage.File,
                    Retention = StreamConfigRetention.Workqueue,
                    MaxAge = maxAge ?? TimeSpan.FromDays(7),
                    MaxBytes = maxBytes ?? 0,
                    MaxMsgs = maxMsgs ?? 0,
                    Discard = StreamConfigDiscard.Old,
                    DuplicateWindow = duplicateWindow ?? TimeSpan.FromMinutes(5),
                };

                await context.CreateStreamAsync(config, cancellationToken: cancellationToken);
                logger.LogInformation("Created stream {StreamName}", streamName);
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task EnsureDurableConsumerAsync(
        string streamName,
        string consumerName,
        string filterSubject,
        CancellationToken cancellationToken = default,
        int maxDeliver = 3,
        TimeSpan? ackWait = null,
        int maxAckPending = 10)
    {
        var context = await GetContextAsync(cancellationToken);

        await _lock.WaitAsync(cancellationToken);
        try
        {
            try
            {
                _ = await context.GetConsumerAsync(streamName, consumerName, cancellationToken: cancellationToken);
                return;
            }
            catch (NatsJSApiException ex) when (ex.Error?.Code == 404)
            {
                var config = new ConsumerConfig
                {
                    Name = consumerName,
                    DurableName = consumerName,
                    FilterSubject = filterSubject,
                    AckPolicy = ConsumerConfigAckPolicy.Explicit,
                    MaxDeliver = maxDeliver,
                    AckWait = ackWait ?? TimeSpan.FromMinutes(5),
                    MaxAckPending = maxAckPending,
                };

                await context.CreateConsumerAsync(streamName, config, cancellationToken: cancellationToken);
                logger.LogInformation("Created consumer {ConsumerName} on stream {StreamName}", consumerName, streamName);
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<INatsJSConsumer> GetConsumerAsync(string streamName, string consumerName, CancellationToken cancellationToken = default)
    {
        var context = await GetContextAsync(cancellationToken);
        return await context.GetConsumerAsync(streamName, consumerName, cancellationToken: cancellationToken);
    }

    public async Task<bool> PublishAsync(string subject, byte[] data, NatsHeaders? headers = null, CancellationToken cancellationToken = default)
    {
        var context = await GetContextAsync(cancellationToken);
        var ack = await context.PublishAsync(subject, data, headers: headers, cancellationToken: cancellationToken);
        return ack.Duplicate;
    }
}
