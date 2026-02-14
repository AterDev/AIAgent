using KnowledgeBaseMod.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Perigon.AspNetCore.Toolkit.Options;
using Perigon.AspNetCore.Toolkit.Services;
using System.ComponentModel;

namespace KnowledgeBaseMod;

[DisplayName("Perigon::KnowledgeBaseMod")]
[Description("Knowledge base and RAG module")]
public static class ModuleExtensions
{
    /// <summary>
    /// Module services or init task.
    /// </summary>
    public static IHostApplicationBuilder AddKnowledgeBaseMod(this IHostApplicationBuilder builder)
    {
        builder.AddModServices();
        return builder;
    }

    // The module services registration
    private static IHostApplicationBuilder AddModServices(this IHostApplicationBuilder builder)
    {
        // Configure AWS S3
        builder.Services.Configure<AWSS3Option>(builder.Configuration.GetSection(AWSS3Option.ConfigPath));
        builder.Services.AddScoped<AWSS3Service>();
        builder.Services.AddScoped<FileStorageService>();
        
        // RAG 查询业务服务
        builder.Services.AddScoped<IRagQueryService, RagQueryService>();
        
        // Business managers
        builder.Services.AddScoped<DocumentParsingResultManager>();
        
        return builder;
    }

    // The module middlewares registration
    public static WebApplication UseKnowledgeBaseModServices(this WebApplication app)
    {
        // custom middlewares and init task
        return app;
    }
}