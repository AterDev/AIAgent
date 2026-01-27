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
        builder.Services.AddScoped<IDocumentParser, SimpleDocumentParser>();
        builder.Services.AddScoped<ITextChunker, DefaultTextChunker>();
        builder.Services.AddScoped<IEmbeddingGenerator, HashEmbeddingGenerator>();
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