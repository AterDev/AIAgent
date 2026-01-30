using Microsoft.Extensions.Hosting;

namespace ModelMod.Services;

/// <summary>
/// module init host service
/// </summary>
public class InitModelModService(
    IServiceProvider serviceProvider,
    ILogger<InitModelModService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _ = serviceProvider;
        // using var scope = serviceProvider.CreateScope();

        try
        {
            logger.LogInformation("ModelMod initializing...");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ModelMod initialization failed");
            return;
        }
        finally
        {
        }
    }
}