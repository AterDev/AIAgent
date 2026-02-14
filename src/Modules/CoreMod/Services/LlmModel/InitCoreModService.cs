using Microsoft.Extensions.Hosting;

namespace CoreMod.Services;

/// <summary>
/// module init host service
/// </summary>
public class InitCoreModService(
    IServiceProvider serviceProvider,
    ILogger<InitCoreModService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _ = serviceProvider;

        try
        {
            logger.LogInformation("CoreMod initializing...");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CoreMod initialization failed");
            return;
        }
        finally
        {
        }
    }
}
