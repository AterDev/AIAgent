using KnowledgeBaseMod.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
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

        builder.Services.AddSingleton<IEntityTaskQueue<RagIngestionTask>>(new EntityTaskQueue<RagIngestionTask>());
        builder.Services.AddSingleton<IRagIngestionQueue, RagIngestionQueue>();
        builder.Services.AddHostedService<RagIngestionWorker>();
        builder.Services.AddScoped<IRagQueryService, RagQueryService>();
        builder.Services.AddScoped<Share.Services.IRagQueryFacade, RagQueryFacade>();
        // 使用多格式文档解析器，支持 PDF、Word、Excel
        builder.Services.AddScoped<IDocumentParser, MultiFormatDocumentParser>();
        // 保留简单解析器作为备选
        builder.Services.AddScoped<SimpleDocumentParser>();
        builder.Services.AddScoped<ITextChunker, DefaultTextChunker>();
        // 使用真实的模型调用生成向量，而不是 MD5 哈希
        builder.Services.AddScoped<IEmbeddingGenerator, CoreModelEmbeddingGenerator>();
        // 保留旧实现作为备选（如果模型调用失败可以降级）
        builder.Services.AddScoped<HashEmbeddingGenerator>();
        builder.Services.AddScoped<QdrantVectorStore>();
        builder.Services.AddScoped<NullVectorStore>();
        builder.Services.AddScoped<IVectorStore>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<QdrantOptions>>().Value;
            return string.IsNullOrWhiteSpace(options.Url)
                ? sp.GetRequiredService<NullVectorStore>()
                : sp.GetRequiredService<QdrantVectorStore>();
        });
        builder.Services.AddScoped<IRagIngestionService, RagIngestionService>();
        return builder;
    }

    // The module middlewares registration
    public static WebApplication UseKnowledgeBaseModServices(this WebApplication app)
    {
        // custom middlewares and init task
        return app;
    }
}