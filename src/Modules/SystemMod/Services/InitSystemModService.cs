using Microsoft.Extensions.Hosting;

namespace SystemMod.Services;

/// <summary>
/// module init host service
/// </summary>
public class InitSystemModService(
    IServiceProvider serviceProvider,
    ILogger<InitSystemModService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // using var scope = serviceProvider.CreateScope();

        try
        {
            logger.LogInformation("SystemMod initializing...");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SystemMod initialization failed");
            return;
        }
        finally
        {
        }
    }
}