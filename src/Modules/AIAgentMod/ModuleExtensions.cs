using AIAgentMod.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
namespace AIAgentMod;

public static class ModuleExtensions
{
    /// <summary>
    /// module services or init task
    /// </summary>
    public static IHostApplicationBuilder AddAIAgentMod(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IEntityTaskQueue<AgentExecutionTask>>(new EntityTaskQueue<AgentExecutionTask>());
        builder.Services.AddSingleton<IAgentExecutionQueue, AgentExecutionQueue>();
        builder.Services.AddScoped<IAgentExecutionService, AgentExecutionService>();
        builder.Services.AddHostedService<AgentExecutionWorker>();
        
        // AG-UI Integration Services
        builder.Services.AddSingleton<AgUiCommunicationService>();
        builder.Services.AddScoped<IStreamingAgentExecutor, StreamingAgentExecutor>();
        
        return builder;
    }
}