using Microsoft.Extensions.Hosting;

namespace WorkflowMod.Services;

/// <summary>
/// module init host service
/// </summary>
public class InitWorkflowModService(
    IServiceProvider serviceProvider,
    ILogger<InitWorkflowModService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _ = serviceProvider;
        // using var scope = serviceProvider.CreateScope();

        try
        {
            logger.LogInformation("WorkflowMod initializing...");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "WorkflowMod initialization failed");
            return;
        }
        finally
        {
        }
    }
}