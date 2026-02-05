using Microsoft.Extensions.Hosting;
using NATS.Client.Core;
using Share.Models;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Perigon.AspNetCore.Options;
using Entity.KnowledgeBaseMod;

namespace CoreMod.Services;

/// <summary>
/// Background service for processing RAG document parsing and vectorization tasks
/// </summary>
/// <remarks>
/// The service supports two modes of operation:
/// 1. When NATS is available and MQType != None: Uses message queue for immediate processing
/// 2. When NATS is unavailable or MQType == None: Uses polling-based processing
/// 
/// Design Note: INatsConnection is nullable to support graceful degradation. While NATS is registered
/// in DI, it may fail to connect or be intentionally disabled via configuration. The nullable parameter
/// allows the service to start and operate in polling mode rather than failing at startup.
/// This is a deliberate design choice for resilience over fail-fast behavior.
/// </remarks>
public class BackgroundParsingService(
    IServiceProvider serviceProvider,
    IOptions<ComponentOption> componentOptions,
    ILogger<BackgroundParsingService> logger,
    INatsConnection? natsConnection = null
) : BackgroundService
{
    private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(10);
    private readonly SemaphoreSlim _processingLock = new(1, 1);
    private const string SubjectName = "rag.ingestion";
    private readonly ComponentOption _componentOptions = componentOptions.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("BackgroundParsingService starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingDocumentsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing pending documents");
            }

            try
            {
                await Task.Delay(_pollInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        logger.LogInformation("BackgroundParsingService stopped");
    }

    /// <summary>
    /// 手动将文档加入解析队列
    /// </summary>
    public async Task EnqueueDocumentAsync(Guid documentId, Guid tenantId, CancellationToken cancellationToken = default, Guid? collectionId = null)
    {
        try
        {
            // Fetch document details to get required fields
            using var scope = serviceProvider.CreateScope();
            var dbFactory = scope.ServiceProvider.GetRequiredService<TenantDbFactory>();
            await using var dbContext = await dbFactory.CreateDbContextAsync();
            
            var document = await dbContext.RagDocuments
                .FirstOrDefaultAsync(d => d.Id == documentId && d.TenantId == tenantId, cancellationToken);

            if (document == null)
            {
                throw new BusinessException("Document not found");
            }

            var message = new RagIngestionMessage
            {
                DocumentId = documentId,
                TenantId = tenantId,
                CollectionId = collectionId ?? document.CollectionId,
                FilePath = document.FilePath ?? string.Empty,
                FileType = document.FileType ?? "txt",
                DocumentName = document.Name,
                FileName = document.FileName,
                StorageProviderId = document.StorageProviderId
            };

            // Publish to NATS if available; otherwise rely on polling mechanism
            // Check both MQType config and natsConnection since injection is optional
            if (_componentOptions.MQType != MQType.None && natsConnection != null)
            {
                var json = JsonSerializer.Serialize(message);
                var data = System.Text.Encoding.UTF8.GetBytes(json);

                await natsConnection.PublishAsync(SubjectName, data, cancellationToken: cancellationToken);
                logger.LogInformation("Enqueued document {DocumentId} for parsing via NATS", documentId);
            }
            else
            {
                logger.LogInformation("Enqueued document {DocumentId} for parsing (MQ disabled, will be processed by polling)", documentId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to enqueue document {DocumentId}", documentId);
            throw;
        }
    }

    private async Task ProcessPendingDocumentsAsync(CancellationToken cancellationToken)
    {
        // Prevent concurrent processing
        if (!await _processingLock.WaitAsync(0, cancellationToken))
        {
            return;
        }

        try
        {
            using var scope = serviceProvider.CreateScope();
            var dbFactory = scope.ServiceProvider.GetRequiredService<TenantDbFactory>();
            var ingestionService = scope.ServiceProvider.GetRequiredService<RagIngestionService>();

            await using var dbContext = await dbFactory.CreateDbContextAsync();

            // Find documents that are in Pending or Failed status
            var pendingDocuments = await dbContext.RagDocuments
                .Where(d => d.Status == RagDocumentStatus.Pending || 
                           (d.Status == RagDocumentStatus.Failed && d.RetryCount < 3))
                .OrderBy(d => d.CreatedTime)
                .Take(10)
                .ToListAsync(cancellationToken);

            if (pendingDocuments.Count == 0)
            {
                return;
            }

            logger.LogInformation("Processing {Count} pending documents", pendingDocuments.Count);

            foreach (var document in pendingDocuments)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    // Increment retry count if failed
                    if (document.Status == RagDocumentStatus.Failed)
                    {
                        document.RetryCount++;
                        await dbContext.SaveChangesAsync(cancellationToken);
                    }

                    await ingestionService.IngestAsync(document.Id, cancellationToken: cancellationToken);

                    logger.LogInformation("Successfully processed document {DocumentId}", document.Id);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to process document {DocumentId}", document.Id);

                    // Update status to Failed if not already
                    if (document.Status != RagDocumentStatus.Failed)
                    {
                        document.Status = RagDocumentStatus.Failed;
                        document.ErrorMessage = ex.Message;
                        await dbContext.SaveChangesAsync(cancellationToken);
                    }
                }
            }
        }
        finally
        {
            _processingLock.Release();
        }
    }

    public override void Dispose()
    {
        _processingLock.Dispose();
        base.Dispose();
    }
}
