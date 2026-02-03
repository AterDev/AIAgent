using KnowledgeBaseMod.Managers;
using KnowledgeBaseMod.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Perigon.AspNetCore.Toolkit.Options;
using Perigon.AspNetCore.Toolkit.Services;
using Share.Services;
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
        
        builder.Services.Configure<QdrantOptions>(builder.Configuration.GetSection("Qdrant"));
        builder.Services.PostConfigure<QdrantOptions>(options =>
        {
            var url = builder.Configuration.GetConnectionString("qdrant");
            if (!string.IsNullOrWhiteSpace(url))
            {
                options.Url = url;
            }

            if (string.IsNullOrWhiteSpace(options.Url))
            {
                options.Url = builder.Configuration["Aspire:Qdrant:Client:Endpoint"]
                    ?? builder.Configuration["Qdrant:Url"]
                    ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(options.ApiKey))
            {
                options.ApiKey = builder.Configuration["Aspire:Qdrant:Client:Key"]
                    ?? builder.Configuration["Aspire:Qdrant:Client:ApiKey"]
                    ?? builder.Configuration["Qdrant:ApiKey"];
            }
        });

        // 注意：队列和后台工作器不再在此注册，将由 FileProcessorService 独立处理
        // 注册 NATS 消息发布者
        builder.Services.AddScoped<NatsRagMessagePublisher>();
        builder.Services.AddScoped<IRagQueryService, RagQueryService>();
        builder.Services.AddScoped<Share.Services.IRagQueryFacade, RagQueryFacade>();
        // Use Kreuzberg-based parser for all formats
        builder.Services.AddScoped<IDocumentParser, KreuzbergDocumentParser>();
        // Keep simple parser as fallback option
        builder.Services.AddScoped<SimpleDocumentParser>();
        builder.Services.AddScoped<ITextChunker, DefaultTextChunker>();
        // Use real model embedding generation
        builder.Services.AddScoped<IEmbeddingGenerator, CoreModelEmbeddingGenerator>();
        // Keep hash-based generator as fallback
        builder.Services.AddScoped<HashEmbeddingGenerator>();
        builder.Services.AddScoped<QdrantService>();
        builder.Services.AddScoped<NullVectorStore>();
        builder.Services.AddScoped<IVectorStore>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<QdrantOptions>>().Value;
            return string.IsNullOrWhiteSpace(options.Url)
                ? sp.GetRequiredService<NullVectorStore>()
                : sp.GetRequiredService<QdrantService>();
        });
        builder.Services.AddScoped<RagIngestionService>();
        builder.Services.AddScoped<DocumentParsingResultManager>();
        builder.Services.AddScoped<BackgroundParsingService>();
        return builder;
    }

    // The module middlewares registration
    public static WebApplication UseKnowledgeBaseModServices(this WebApplication app)
    {
        // custom middlewares and init task
        return app;
    }
}