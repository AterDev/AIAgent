using Microsoft.Extensions.Hosting;
using NATS.Client.Core;
using Share.Models;
using System.Text.Json;

namespace KnowledgeBaseMod.Services;

/// <summary>
/// Background service for processing RAG document parsing and vectorization tasks
/// </summary>
public class BackgroundParsingService(
    IServiceProvider serviceProvider,
    INatsConnection natsConnection,
    ILogger<BackgroundParsingService> logger
) : BackgroundService
{
    private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(10);
    private readonly SemaphoreSlim _processingLock = new(1, 1);
    private const string SubjectName = "rag.ingestion";

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
                ContentType = document.ContentType ?? "text/plain",
                DocumentName = document.Name,
                FileName = document.FileName
            };

            var json = JsonSerializer.Serialize(message);
            var data = System.Text.Encoding.UTF8.GetBytes(json);

            await natsConnection.PublishAsync(SubjectName, data, cancellationToken: cancellationToken);
            logger.LogInformation("Enqueued document {DocumentId} for parsing", documentId);
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
