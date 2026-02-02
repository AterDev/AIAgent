using CoreMod.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using System.ComponentModel;

namespace CoreMod;

[DisplayName("Perigon::CoreMod")]
[Description("Core module for technical invocation capabilities")]
public static class ModuleExtensions
{
    /// <summary>
    /// Module services or init task.
    /// </summary>
    public static IHostApplicationBuilder AddCoreMod(this IHostApplicationBuilder builder)
    {
        builder.AddModServices();
        return builder;
    }

    // The module services registration
    private static IHostApplicationBuilder AddModServices(this IHostApplicationBuilder builder)
    {
        // ExtensionsAIModelClient: 支持 OpenAI、DeepSeek、Azure OpenAI 等所有 OpenAI 协议兼容的服务
        builder.Services.AddScoped<ExtensionsAIModelClient>();
        
        builder.Services.AddScoped<IModelRouter, DbModelRouter>();
        builder.Services.AddSingleton<IUsageMeter, DefaultUsageMeter>();
        return builder;
    }

    // The module middlewares registration
    public static WebApplication UseCoreModServices(this WebApplication app)
    {
        // custom middlewares and init task
        return app;
    }
}