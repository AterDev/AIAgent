using CoreMod.Services;
using CoreMod.Models.RagIngestion;
using CoreMod.Services.DocumentParsing;
using CoreMod.Services.Embedding;
using CoreMod.Services.ModelRouting;
using CoreMod.Services.RagIngestion;
using CoreMod.Services.VectorStore;
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
        
        // 模型路由 - CoreMod 内部使用
        builder.Services.AddScoped<DbModelRouter>();
        builder.Services.AddSingleton<DefaultUsageMeter>();
        
        // Add Qdrant Client via Aspire integration
        builder.AddQdrantClient("qdrant");
        
        // 注册 NATS 消息发布者
        builder.Services.AddSingleton<NatsJetStreamService>();
        builder.Services.AddScoped<NatsRagMessagePublisher>();
        
        // RAG 文档解析服务
        // Use Kreuzberg-based parser for all formats
        builder.Services.AddScoped<IDocumentParser, KreuzbergDocumentParser>();
        // Keep simple parser as fallback option
        builder.Services.AddScoped<SimpleDocumentParser>();
        
        // 文本分块服务
        builder.Services.AddScoped<DefaultTextChunker>();
        
        // 向量嵌入生成服务 - CoreMod 内部使用
        // Use real model embedding generation
        builder.Services.AddScoped<CoreModelEmbeddingGenerator>();
        // Keep hash-based generator as fallback
        builder.Services.AddScoped<HashEmbeddingGenerator>();
        
        // 向量存储服务
        builder.Services.AddScoped<QdrantService>();
        builder.Services.AddScoped<NullVectorStore>();
        builder.Services.AddScoped<IVectorStore>(sp =>
        {
            // Always use QdrantService when Aspire client is configured
            return sp.GetRequiredService<QdrantService>();
        });
        
        // RAG 摄取服务 - CoreMod 内部使用
        builder.Services.AddScoped<RagIngestionService>();
        builder.Services.AddScoped<DocumentChunkingService>();
        builder.Services.AddSingleton<IEntityTaskQueue<RagDocumentIngestionTask>>(new EntityTaskQueue<RagDocumentIngestionTask>());
        builder.Services.AddSingleton<RagIngestionQueue>();
        builder.Services.AddHostedService<RagIngestionWorker>();
        builder.Services.AddHostedService<BackgroundParsingService>();
        
        return builder;
    }

    // The module middlewares registration
    public static WebApplication UseCoreModServices(this WebApplication app)
    {
        // custom middlewares and init task
        return app;
    }
}