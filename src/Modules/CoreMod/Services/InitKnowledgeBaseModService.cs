using Microsoft.Extensions.Hosting;

namespace CoreMod.Services;

/// <summary>
/// module init host service
/// </summary>
public class InitKnowledgeBaseModService(
    IServiceProvider serviceProvider,
    ILogger<InitKnowledgeBaseModService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _ = serviceProvider;
        // using var scope = serviceProvider.CreateScope();

        try
        {
            logger.LogInformation("KnowledgeBaseMod initializing...");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "KnowledgeBaseMod initialization failed");
            return;
        }
        finally
        {
        }
    }
}