using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using ModelMod.Services;
using ModelMod.Managers;
using System.ComponentModel;

namespace ModelMod;

[DisplayName("Perigon::ModelMod")]
[Description("Model management and application quota module")]
public static class ModuleExtensions
{
    /// <summary>
    /// Module services or init task.
    /// </summary>
    public static IHostApplicationBuilder AddModelMod(this IHostApplicationBuilder builder)
    {
        builder.AddModServices();
        return builder;
    }

    // The module services registration
    private static IHostApplicationBuilder AddModServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IModelInvokeService, ModelInvokeService>();
        builder.Services.AddScoped<Share.Services.IModelInvokeFacade, ModelInvokeFacade>();
        builder.Services.AddScoped<AIModelProviderManager>();
        builder.Services.AddScoped<AIModelInfoManager>();
        return builder;
    }

    // The module middlewares registration
    public static WebApplication UseModelModServices(this WebApplication app)
    {
        // custom middlewares and init task
        return app;
    }
}