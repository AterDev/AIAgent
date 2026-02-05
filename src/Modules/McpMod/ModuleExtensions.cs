using CoreMod.Services;
using McpMod.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using System.ComponentModel;

namespace McpMod;

[DisplayName("Perigon::McpMod")]
[Description("MCP tool registry and execution module")]
public static class ModuleExtensions
{
    /// <summary>
    /// Module services or init task.
    /// </summary>
    public static IHostApplicationBuilder AddMcpMod(this IHostApplicationBuilder builder)
    {
        builder.AddModServices();
        return builder;
    }

    // The module services registration
    private static IHostApplicationBuilder AddModServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<BuiltinToolExecutor>();
        builder.Services.AddScoped<IMcpToolExecutor, McpToolExecutor>();
        builder.Services.AddHostedService<InitMcpModService>();
        return builder;
    }

    // The module middlewares registration
    public static WebApplication UseMcpModServices(this WebApplication app)
    {
        // custom middlewares and init task
        return app;
    }
}