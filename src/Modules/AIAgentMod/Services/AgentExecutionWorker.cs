using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AIAgentMod.Services;

public class AgentExecutionWorker(
    IServiceProvider serviceProvider,
    IAgentExecutionQueue queue,
    ILogger<AgentExecutionWorker> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            AgentExecutionTask task;
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
                var service = scope.ServiceProvider.GetRequiredService<IAgentExecutionService>();
                await service.ExecuteAsync(task.ExecutionId, task.ApplicationId, task.InputJson, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Agent execution failed {ExecutionId}", task.ExecutionId);
            }
        }
    }
}
