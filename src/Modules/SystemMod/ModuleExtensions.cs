using CoreMod.Abstraction;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Share.Abstraction;
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

        builder.Services.AddHostedService<InitSystemModService>();
        builder.AddModServices();
        return builder;
    }

    // The module services registration
    private static IHostApplicationBuilder AddModServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<ISystemConfigService, SystemConfigService>();
        builder.Services.AddScoped<IFileStorageService, FileStorageService>();
        builder.Services.AddScoped<IStorageProviderQuery, StorageProviderQueryService>();
        return builder;
    }

    // The module middlewares registration
    public static WebApplication UseSystemModServices(this WebApplication app)
    {
        // custom middlewares and init task
        return app;
    }
}