using Microsoft.Extensions.Hosting;
using NATS.Client.Core;
using Share.Models;
using Perigon.AspNetCore.Options;
using Entity.KnowledgeBaseMod;

namespace CoreMod.Services.RagIngestion;

/// <summary>
/// Background service for processing RAG document parsing and vectorization tasks
/// </summary>
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

    public async Task EnqueueDocumentAsync(Guid documentId, Guid tenantId, CancellationToken cancellationToken = default, Guid? collectionId = null)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var dbFactory = scope.ServiceProvider.GetRequiredService<AppDbFactory>();
            await using var dbContext = await dbFactory.CreateDbContextAsync(tenantId);

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
        if (!await _processingLock.WaitAsync(0, cancellationToken))
        {
            return;
        }

        try
        {
            using var scope = serviceProvider.CreateScope();
            var dbFactory = scope.ServiceProvider.GetRequiredService<AppDbFactory>();
            var ingestionService = scope.ServiceProvider.GetRequiredService<RagIngestionService>();

            await using var catalogDbContext = await dbFactory.CreateDbContextAsync(null);
            var tenantIds = await catalogDbContext.Tenants
                .Select(tenant => tenant.Id)
                .ToListAsync(cancellationToken);

            foreach (var tenantId in tenantIds)
            {
                await using var dbContext = await dbFactory.CreateDbContextAsync(tenantId);
                var pendingDocuments = await dbContext.RagDocuments
                    .Where(document => document.Status == RagDocumentStatus.Pending
                        || (document.Status == RagDocumentStatus.Failed && document.RetryCount < 3))
                    .OrderBy(document => document.CreatedTime)
                    .Take(10)
                    .ToListAsync(cancellationToken);

                if (pendingDocuments.Count == 0)
                {
                    continue;
                }

                logger.LogInformation(
                    "Processing {Count} pending documents for tenant {TenantId}",
                    pendingDocuments.Count,
                    tenantId
                );

                foreach (var document in pendingDocuments)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    try
                    {
                        if (document.Status == RagDocumentStatus.Failed)
                        {
                            document.RetryCount++;
                            await dbContext.SaveChangesAsync(cancellationToken);
                        }

                        await ingestionService.IngestAsync(
                            document.Id,
                            tenantId,
                            cancellationToken: cancellationToken
                        );

                        logger.LogInformation("Successfully processed document {DocumentId}", document.Id);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to process document {DocumentId}", document.Id);

                        if (document.Status != RagDocumentStatus.Failed)
                        {
                            document.Status = RagDocumentStatus.Failed;
                            document.ErrorMessage = ex.Message;
                            await dbContext.SaveChangesAsync(cancellationToken);
                        }
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
