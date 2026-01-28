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
        // 使用增强的 Agent 执行服务，支持多轮对话和工具调用链路
        builder.Services.AddScoped<IAgentExecutionService, EnhancedAgentExecutionService>();
        // 保留旧实现作为备选
        builder.Services.AddScoped<AgentExecutionService>();
        builder.Services.AddHostedService<AgentExecutionWorker>();

        return builder;
    }
}