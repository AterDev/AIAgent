using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Entity.KnowledgeBaseMod;
using Entity.ModelMod;

namespace MigrationService;

public class Worker(
    IServiceProvider serviceProvider,
    IHostApplicationLifetime hostApplicationLifetime,
    ILogger<Worker> logger
) : BackgroundService
{
    private readonly ILogger<Worker> _logger = logger;
    public const string ActivitySourceName = "Migrations";
    private static readonly ActivitySource _activitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var activity = _activitySource.StartActivity(
            "Migrating database",
            ActivityKind.Client
        );
        try
        {
            using var scope = serviceProvider.CreateScope();
            _logger.LogInformation("migrations {db}", nameof(DefaultDbContext));
            var dbContext = scope.ServiceProvider.GetRequiredService<DefaultDbContext>();
            await RunMigrationAsync(dbContext, cancellationToken);
            await SeedDataAsync(dbContext, cancellationToken);
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
        }
        finally
        {
            hostApplicationLifetime.StopApplication();
        }
    }

    private static async Task RunMigrationAsync<T>(T dbContext, CancellationToken cancellationToken)
        where T : DbContext
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        });
    }

    private static async Task SeedDataAsync<T>(T dbContext, CancellationToken cancellationToken)
        where T : DbContext
    {
        if (dbContext is not DefaultDbContext defaultDb)
        {
            return;
        }

        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await SeedAdminUserAsync(defaultDb, cancellationToken);
            await SeedModelProvidersAsync(defaultDb, cancellationToken);
            await SeedStorageProviderAsync(defaultDb, cancellationToken);
            await SeedDefaultKnowledgeBaseAsync(defaultDb, cancellationToken);
        });
    }

    private static async Task SeedAdminUserAsync(DefaultDbContext db, CancellationToken cancellationToken)
    {
        if (await db.SystemUsers.AnyAsync(cancellationToken))
        {
            return;
        }

        var passwordSalt = HashCrypto.BuildSalt();
        var adminUser = new SystemUser
        {
            Id = Guid.NewGuid(),
            UserName = "admin",
            Email = "admin@default.com",
            RealName = "System Administrator",
            Phone = "13800138000",
            PasswordSalt = passwordSalt,
            PasswordHash = HashCrypto.GeneratePwd("Perigon.2026", passwordSalt),
            Roles = WebConst.SuperAdmin,
            Enabled = true
        };

        db.SystemUsers.Add(adminUser);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedModelProvidersAsync(DefaultDbContext db, CancellationToken cancellationToken)
    {
        if (await db.AIModelProviders.AnyAsync(cancellationToken))
        {
            return;
        }

        // DeepSeek
        var deepSeek = new AIModelProvider
        {
            Name = "DeepSeek",
            Description = "DeepSeek AI - 高性能开源大语言模型提供商",
            Website = "https://www.deepseek.com",
            BaseUrl = "https://api.deepseek.com/v1",
            LogoUrl = "https://www.deepseek.com/favicon.ico",
            Models =
            [
                new AIModelInfo { Name = "deepseek-chat", DisplayName = "DeepSeek Chat (V3)", Description = "DeepSeek V3 通用对话模型", ContextLength = 65536, MaxContextTokens = 65536, SupportsChat = true, SupportsTools = true, InputPrice = 0.27m, OutputPrice = 1.10m, IsEnabled = true },
                new AIModelInfo { Name = "deepseek-reasoner", DisplayName = "DeepSeek Reasoner (R1)", Description = "DeepSeek R1 深度推理模型", ContextLength = 65536, MaxContextTokens = 65536, SupportsChat = true, InputPrice = 0.55m, OutputPrice = 2.19m, IsEnabled = true },
            ]
        };

        // OpenAI
        var openAI = new AIModelProvider
        {
            Name = "OpenAI",
            Description = "OpenAI - GPT 系列模型提供商",
            Website = "https://openai.com",
            BaseUrl = "https://api.openai.com/v1",
            LogoUrl = "https://openai.com/favicon.ico",
            Models =
            [
                new AIModelInfo { Name = "gpt-4.1", DisplayName = "GPT-4.1", Description = "最新旗舰模型，适合复杂任务", ContextLength = 1047576, MaxContextTokens = 1047576, SupportsChat = true, SupportsTools = true, SupportsVision = true, SupportsResponsesApi = true, InputPrice = 2.00m, OutputPrice = 8.00m, IsEnabled = true },
                new AIModelInfo { Name = "gpt-4.1-mini", DisplayName = "GPT-4.1 Mini", Description = "高性价比模型，适合快速任务", ContextLength = 1047576, MaxContextTokens = 1047576, SupportsChat = true, SupportsTools = true, SupportsVision = true, SupportsResponsesApi = true, InputPrice = 0.40m, OutputPrice = 1.60m, IsEnabled = true },
                new AIModelInfo { Name = "gpt-4.1-nano", DisplayName = "GPT-4.1 Nano", Description = "最快速、最经济的模型", ContextLength = 1047576, MaxContextTokens = 1047576, SupportsChat = true, SupportsTools = true, SupportsResponsesApi = true, InputPrice = 0.10m, OutputPrice = 0.40m, IsEnabled = true },
                new AIModelInfo { Name = "o3", DisplayName = "o3", Description = "最强推理模型，适合数学/编程/科学", ContextLength = 200000, MaxContextTokens = 200000, SupportsChat = true, SupportsTools = true, SupportsResponsesApi = true, InputPrice = 2.00m, OutputPrice = 8.00m, IsEnabled = true },
                new AIModelInfo { Name = "o4-mini", DisplayName = "o4-mini", Description = "快速推理模型", ContextLength = 200000, MaxContextTokens = 200000, SupportsChat = true, SupportsTools = true, SupportsResponsesApi = true, InputPrice = 1.10m, OutputPrice = 4.40m, IsEnabled = true },
                new AIModelInfo { Name = "text-embedding-3-small", DisplayName = "Embedding 3 Small", Description = "小型向量化模型，性价比高", ContextLength = 8191, MaxContextTokens = 8191, SupportsEmbedding = true, InputPrice = 0.02m, OutputPrice = 0m, IsEnabled = true },
                new AIModelInfo { Name = "text-embedding-3-large", DisplayName = "Embedding 3 Large", Description = "大型向量化模型，精度更高", ContextLength = 8191, MaxContextTokens = 8191, SupportsEmbedding = true, InputPrice = 0.13m, OutputPrice = 0m, IsEnabled = true },
            ]
        };

        // Anthropic
        var anthropic = new AIModelProvider
        {
            Name = "Anthropic",
            Description = "Anthropic - Claude 系列模型提供商",
            Website = "https://www.anthropic.com",
            BaseUrl = "https://api.anthropic.com/v1",
            LogoUrl = "https://www.anthropic.com/favicon.ico",
            Models =
            [
                new AIModelInfo { Name = "claude-sonnet-4-20250514", DisplayName = "Claude Sonnet 4", Description = "高性能模型，编程与推理能力强", ContextLength = 200000, MaxContextTokens = 200000, SupportsChat = true, SupportsTools = true, SupportsVision = true, InputPrice = 3.00m, OutputPrice = 15.00m, IsEnabled = true },
                new AIModelInfo { Name = "claude-opus-4-20250514", DisplayName = "Claude Opus 4", Description = "最强旗舰模型，长时间自主工作能力", ContextLength = 200000, MaxContextTokens = 200000, SupportsChat = true, SupportsTools = true, SupportsVision = true, InputPrice = 15.00m, OutputPrice = 75.00m, IsEnabled = true },
                new AIModelInfo { Name = "claude-haiku-3-5-20241022", DisplayName = "Claude 3.5 Haiku", Description = "最快速经济的模型", ContextLength = 200000, MaxContextTokens = 200000, SupportsChat = true, SupportsTools = true, SupportsVision = true, InputPrice = 0.80m, OutputPrice = 4.00m, IsEnabled = true },
            ]
        };

        // Qwen (通义千问)
        var qwen = new AIModelProvider
        {
            Name = "Qwen",
            Description = "阿里云通义千问 - 国产大语言模型",
            Website = "https://dashscope.aliyun.com",
            BaseUrl = "https://dashscope.aliyuncs.com/compatible-mode/v1",
            LogoUrl = "https://img.alicdn.com/imgextra/i4/O1CN01c4CIoB1Ztqqe5wDfz_!!6000000003254-73-tps-16-16.ico",
            Models =
            [
                new AIModelInfo { Name = "qwen-max", DisplayName = "Qwen Max", Description = "通义千问超大规模模型", ContextLength = 131072, MaxContextTokens = 131072, SupportsChat = true, SupportsTools = true, InputPrice = 2.40m, OutputPrice = 9.60m, IsEnabled = true },
                new AIModelInfo { Name = "qwen-plus", DisplayName = "Qwen Plus", Description = "通义千问增强模型，适合意图识别", ContextLength = 131072, MaxContextTokens = 131072, SupportsChat = true, SupportsTools = true, InputPrice = 0.80m, OutputPrice = 2.00m, IsEnabled = true },
                new AIModelInfo { Name = "qwen-turbo", DisplayName = "Qwen Turbo", Description = "通义千问快速模型，性价比高", ContextLength = 131072, MaxContextTokens = 131072, SupportsChat = true, SupportsTools = true, InputPrice = 0.30m, OutputPrice = 0.60m, IsEnabled = true },
                new AIModelInfo { Name = "text-embedding-v3", DisplayName = "通义千问 Embedding V3", Description = "通义千问文本向量化模型", ContextLength = 8192, MaxContextTokens = 8192, SupportsEmbedding = true, InputPrice = 0.07m, OutputPrice = 0m, IsEnabled = true },
            ]
        };

        // Google
        var google = new AIModelProvider
        {
            Name = "Google",
            Description = "Google AI - Gemini 系列模型",
            Website = "https://ai.google.dev",
            BaseUrl = "https://generativelanguage.googleapis.com/v1beta/openai",
            LogoUrl = "https://www.gstatic.com/lamda/images/gemini_favicon_f069958c85030456e93de685481c559f160ea06b.png",
            Models =
            [
                new AIModelInfo { Name = "gemini-2.5-pro", DisplayName = "Gemini 2.5 Pro", Description = "Google 最强模型，支持思考", ContextLength = 1048576, MaxContextTokens = 1048576, SupportsChat = true, SupportsTools = true, SupportsVision = true, InputPrice = 1.25m, OutputPrice = 10.00m, IsEnabled = true },
                new AIModelInfo { Name = "gemini-2.5-flash", DisplayName = "Gemini 2.5 Flash", Description = "快速经济模型", ContextLength = 1048576, MaxContextTokens = 1048576, SupportsChat = true, SupportsTools = true, SupportsVision = true, InputPrice = 0.15m, OutputPrice = 0.60m, IsEnabled = true },
            ]
        };

        // Azure OpenAI
        var azure = new AIModelProvider
        {
            Name = "Azure OpenAI",
            Description = "Microsoft Azure OpenAI Service - 企业级 AI 服务",
            Website = "https://azure.microsoft.com/products/ai-services/openai-service",
            BaseUrl = "https://{your-resource}.openai.azure.com/openai/deployments/{deployment-name}",
            LogoUrl = "https://azure.microsoft.com/favicon.ico",
            Models =
            [
                new AIModelInfo { Name = "gpt-4.1", DisplayName = "Azure GPT-4.1", Description = "Azure 托管的 GPT-4.1 模型", ContextLength = 1047576, MaxContextTokens = 1047576, SupportsChat = true, SupportsTools = true, SupportsVision = true, InputPrice = 2.00m, OutputPrice = 8.00m, IsEnabled = true },
                new AIModelInfo { Name = "gpt-4.1-mini", DisplayName = "Azure GPT-4.1 Mini", Description = "Azure 托管的 GPT-4.1 Mini 模型", ContextLength = 1047576, MaxContextTokens = 1047576, SupportsChat = true, SupportsTools = true, SupportsVision = true, InputPrice = 0.40m, OutputPrice = 1.60m, IsEnabled = true },
            ]
        };

        db.AIModelProviders.AddRange([deepSeek, openAI, anthropic, qwen, google, azure]);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedStorageProviderAsync(DefaultDbContext db, CancellationToken cancellationToken)
    {
        if (await db.StorageProviders.AnyAsync(cancellationToken))
        {
            return;
        }

        var storagePath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? @"C:\AppData\Local\AIAgent\Storage"
            : "/var/lib/aiagent/storage";

        var localStorage = new StorageProvider
        {
            Name = "LocalStorageProvider",
            IsCloud = false,
            Path = storagePath,
            IsActive = true,
        };

        db.StorageProviders.Add(localStorage);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedDefaultKnowledgeBaseAsync(DefaultDbContext db, CancellationToken cancellationToken)
    {
        if (await db.RagCollections.AnyAsync(cancellationToken))
        {
            return;
        }

        var collection = new RagCollection
        {
            Name = "DefaultKnowledgeBase",
            Description = "Default knowledge base for document storage and retrieval",
            IsPublic = true,
            IsEnabled = true,
            Tags = ["default"],
        };

        db.RagCollections.Add(collection);
        await db.SaveChangesAsync(cancellationToken);
    }
}
