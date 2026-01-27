using Microsoft.Extensions.Hosting;

namespace KnowledgeBaseMod.Services;

public class RagIngestionWorker(
    IServiceProvider serviceProvider,
    IRagIngestionQueue queue,
    ILogger<RagIngestionWorker> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            RagIngestionTask task;
            try
            {
                task = await queue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            try
            {
                using var scope = serviceProvider.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IRagIngestionService>();
                await service.IngestAsync(task.DocumentId, task.ContentText, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "RAG ingestion failed for document {DocumentId}", task.DocumentId);
            }
        }
    }
}
