using AIAgentMod.Services;
using AIAgentMod.Services.Maf;
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
        builder.Services.AddHttpClient("A2A")
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(10),
            })
            .SetHandlerLifetime(TimeSpan.FromMinutes(10));
        builder.Services.AddSingleton<IEntityTaskQueue<AgentExecutionTask>>(new EntityTaskQueue<AgentExecutionTask>());
        builder.Services.AddSingleton<AgentExecutionQueue>();
        // 使用增强的 Agent 执行服务，支持多轮对话和工具调用链路
        builder.Services.AddScoped<IAgentExecutionService, EnhancedAgentExecutionService>();
        builder.Services.AddScoped<AgentDebugService>();
        builder.Services.AddScoped<A2AClientService>();
        // 保留旧实现作为备选
        builder.Services.AddScoped<AgentExecutionService>();
        builder.Services.AddHostedService<AgentExecutionWorker>();

        // MAF 1.1 原生运行时 - 新业务层优先使用
        builder.Services.AddScoped<MafAgentRuntime>();
        builder.Services.AddScoped<AgentToolFactory>();

        return builder;
    }
}