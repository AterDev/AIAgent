using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using System.ComponentModel;
using SystemMod.Services;

namespace SystemMod;

[DisplayName("Perigon::SystemMod")]
[Description("System configuration and prompt management module")]
public static class ModuleExtensions
{
    /// <summary>
    /// Module services or init task.
    /// </summary>
    public static IHostApplicationBuilder AddSystemMod(this IHostApplicationBuilder builder)
    {
        builder.AddModServices();
        return builder;
    }

    // The module services registration
    private static IHostApplicationBuilder AddModServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<Share.Services.ISystemConfigFacade, SystemConfigFacade>();
        return builder;
    }

    // The module middlewares registration
    public static WebApplication UseSystemModServices(this WebApplication app)
    {
        // custom middlewares and init task
        return app;
    }
}