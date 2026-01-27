using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WorkflowMod.Services;

public class WorkflowWorker(
    IServiceProvider serviceProvider,
    IWorkflowQueue queue,
    ILogger<WorkflowWorker> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            WorkflowTask task;
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
                var executor = scope.ServiceProvider.GetRequiredService<IWorkflowExecutor>();
                await executor.ExecuteAsync(task.WorkflowExecutionId, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Workflow execution failed {ExecutionId}", task.WorkflowExecutionId);
            }
        }
    }
}
