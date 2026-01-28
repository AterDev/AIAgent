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
        // 使用新的 ExtensionsAIModelClient 支持更广泛的 OpenAI 兼容平台
        builder.Services.AddScoped<IModelClient, ExtensionsAIModelClient>();
        // 保留旧实现作为备选（如果需要可以通过配置切换）
        builder.Services.AddScoped<OpenAiCompatibleClient>();
        
        builder.Services.AddScoped<IModelRouter, DbModelRouter>();
        builder.Services.AddSingleton<IModelCapabilityResolver, DefaultModelCapabilityResolver>();
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