using Microsoft.EntityFrameworkCore;
using Share.Implement;
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

    private async Task SeedDataAsync<T>(T dbContext, CancellationToken cancellationToken)
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
            await SeedDemoApplicationAsync(defaultDb, cancellationToken);
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
                new AIModelInfo { Name = "deepseek-chat", DisplayName = "DeepSeek Chat (V3.2)", Description = "DeepSeek 当前主力通用模型，适合对话、工具调用和日常开发任务", ContextLength = 131072, MaxContextTokens = 131072, SupportsChat = true, SupportsTools = true, InputPrice = 0.28m, OutputPrice = 0.42m, IsEnabled = true },
                new AIModelInfo { Name = "deepseek-reasoner", DisplayName = "DeepSeek Reasoner (V3.2 Thinking)", Description = "DeepSeek 当前主力深度推理模型，适合复杂分析、编程与长链路推理", ContextLength = 131072, MaxContextTokens = 131072, SupportsChat = true, SupportsTools = true, InputPrice = 0.55m, OutputPrice = 2.19m, IsEnabled = true },
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
                new AIModelInfo { Name = "gpt-5.4", DisplayName = "GPT-5.4", Description = "OpenAI 当前旗舰模型，适合高复杂度推理、编码与智能体任务", ContextLength = 400000, MaxContextTokens = 400000, SupportsChat = true, SupportsTools = true, SupportsVision = true, SupportsResponsesApi = true, InputPrice = 2.50m, OutputPrice = 15.00m, IsEnabled = true },
                new AIModelInfo { Name = "gpt-5.4-mini", DisplayName = "GPT-5.4 Mini", Description = "OpenAI 当前主力小型模型，适合高频编码、工具调用与日常业务场景", ContextLength = 400000, MaxContextTokens = 400000, SupportsChat = true, SupportsTools = true, SupportsVision = true, SupportsResponsesApi = true, InputPrice = 0.75m, OutputPrice = 4.50m, IsEnabled = true },
                new AIModelInfo { Name = "text-embedding-3-large", DisplayName = "Embedding 3 Large", Description = "OpenAI 主力文本向量模型，适合检索增强、聚类与语义搜索", ContextLength = 8191, MaxContextTokens = 8191, SupportsEmbedding = true, InputPrice = 0.13m, OutputPrice = 0m, IsEnabled = true },
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
                new AIModelInfo { Name = "claude-opus-4-6", DisplayName = "Claude Opus 4.6", Description = "Anthropic 当前最强旗舰模型，适合复杂编码、长链路智能体与企业级任务", ContextLength = 1000000, MaxContextTokens = 1000000, SupportsChat = true, SupportsTools = true, SupportsVision = true, InputPrice = 5.00m, OutputPrice = 25.00m, IsEnabled = true },
                new AIModelInfo { Name = "claude-sonnet-4-6", DisplayName = "Claude Sonnet 4.6", Description = "Anthropic 当前最主流平衡模型，适合生产场景中的编码、代理与专业工作流", ContextLength = 1000000, MaxContextTokens = 1000000, SupportsChat = true, SupportsTools = true, SupportsVision = true, InputPrice = 3.00m, OutputPrice = 15.00m, IsEnabled = true },
                new AIModelInfo { Name = "claude-haiku-4-5", DisplayName = "Claude Haiku 4.5", Description = "Anthropic 当前最快速高性价比模型，适合高并发与低延迟场景", ContextLength = 200000, MaxContextTokens = 200000, SupportsChat = true, SupportsTools = true, SupportsVision = true, InputPrice = 1.00m, OutputPrice = 5.00m, IsEnabled = true },
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
                new AIModelInfo { Name = "qwen3-max", DisplayName = "Qwen3 Max", Description = "通义千问当前旗舰模型，适合复杂推理、长链路任务与高质量生成", ContextLength = 262144, MaxContextTokens = 262144, SupportsChat = true, SupportsTools = true, InputPrice = 2.50m, OutputPrice = 10.00m, IsEnabled = true },
                new AIModelInfo { Name = "qwen3.5-plus", DisplayName = "Qwen3.5 Plus", Description = "通义千问当前主力均衡模型，适合大多数生产场景和多模态任务", ContextLength = 1000000, MaxContextTokens = 1000000, SupportsChat = true, SupportsTools = true, SupportsVision = true, InputPrice = 0.80m, OutputPrice = 4.80m, IsEnabled = true },
                new AIModelInfo { Name = "text-embedding-v4", DisplayName = "通义千问 Embedding V4", Description = "通义千问当前主力文本向量模型，适合语义检索与知识库场景", ContextLength = 8192, MaxContextTokens = 8192, SupportsEmbedding = true, InputPrice = 0.50m, OutputPrice = 0m, IsEnabled = true },
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
                new AIModelInfo { Name = "gemini-3.1-pro-preview", DisplayName = "Gemini 3.1 Pro", Description = "Google 当前最强推理与智能体模型，适合复杂多模态与长上下文任务", ContextLength = 1000000, MaxContextTokens = 1000000, SupportsChat = true, SupportsTools = true, SupportsVision = true, InputPrice = 1.25m, OutputPrice = 10.00m, IsEnabled = true },
                new AIModelInfo { Name = "gemini-3-flash-preview", DisplayName = "Gemini 3 Flash", Description = "Google 当前主力高速模型，兼顾推理质量、速度与成本", ContextLength = 1000000, MaxContextTokens = 1000000, SupportsChat = true, SupportsTools = true, SupportsVision = true, InputPrice = 0.50m, OutputPrice = 3.00m, IsEnabled = true },
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
                new AIModelInfo { Name = "gpt-5.4", DisplayName = "Azure GPT-5.4", Description = "Azure 当前旗舰模型，适合企业级复杂推理、编码与多工具工作流", ContextLength = 1050000, MaxContextTokens = 1050000, SupportsChat = true, SupportsTools = true, SupportsVision = true, SupportsResponsesApi = true, InputPrice = 2.50m, OutputPrice = 15.00m, IsEnabled = true },
                new AIModelInfo { Name = "gpt-5.4-mini", DisplayName = "Azure GPT-5.4 Mini", Description = "Azure 当前主力小型模型，适合高频业务与成本敏感场景", ContextLength = 400000, MaxContextTokens = 400000, SupportsChat = true, SupportsTools = true, SupportsVision = true, SupportsResponsesApi = true, InputPrice = 0.75m, OutputPrice = 4.50m, IsEnabled = true },
            ]
        };

        db.AIModelProviders.AddRange([deepSeek, openAI, anthropic, qwen, google, azure]);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedDemoApplicationAsync(DefaultDbContext db, CancellationToken cancellationToken)
    {
        const string demoAppName = "Demo Open Platform App";

        var deepSeekChat = await db.AIModelInfos
            .Include(q => q.Provider)
            .FirstOrDefaultAsync(q => q.Name == "deepseek-chat", cancellationToken);

        if (deepSeekChat is null)
        {
            _logger.LogWarning("Skip seeding demo application because deepseek-chat model was not found.");
            return;
        }

        var application = await db.Applications
            .FirstOrDefaultAsync(q => q.Name == demoAppName, cancellationToken);

        if (application is null)
        {
            application = new Application
            {
                Name = demoAppName,
                Description = "用于第三方开放平台接入与模型调用验证的示例应用",
                IsEnabled = true,
                TenantId = deepSeekChat.TenantId,
            };

            db.Applications.Add(application);
            await db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Seeded demo application {ApplicationName}. Generate or reset the API key through the admin application UI when needed.",
                application.Name
            );
        }

        var existingApiKeys = await db.ApiKeyAuthIndexes
            .IgnoreQueryFilters()
            .Where(q => q.ApplicationId == application.Id)
            .ToListAsync(cancellationToken);

        foreach (var apiKey in existingApiKeys)
        {
            apiKey.ApplicationName = application.Name;
            apiKey.TenantId = application.TenantId;
            apiKey.IsDeleted = false;
            apiKey.UpdatedTime = DateTimeOffset.UtcNow;
        }

        var hasPermission = await db.ApplicationModelPermissions.AnyAsync(
            q => q.ApplicationId == application.Id && q.AIModelInfoId == deepSeekChat.Id,
            cancellationToken
        );

        if (!hasPermission)
        {
            db.ApplicationModelPermissions.Add(new ApplicationModelPermission
            {
                ApplicationId = application.Id,
                AIModelInfoId = deepSeekChat.Id,
                IsEnabled = true,
                TenantId = application.TenantId,
            });
        }

        var existingQuotas = await db.ApplicationQuotas
            .Where(q => q.ApplicationId == application.Id)
            .Select(q => q.PeriodType)
            .ToListAsync(cancellationToken);

        if (!existingQuotas.Contains(QuotaPeriodType.Minute))
        {
            db.ApplicationQuotas.Add(new ApplicationQuota
            {
                ApplicationId = application.Id,
                PeriodType = QuotaPeriodType.Minute,
                MaxRequests = 20,
                MaxTokens = 40_000,
                WindowSeconds = 60,
                IsEnabled = true,
                TenantId = application.TenantId,
            });
        }

        if (!existingQuotas.Contains(QuotaPeriodType.Day))
        {
            db.ApplicationQuotas.Add(new ApplicationQuota
            {
                ApplicationId = application.Id,
                PeriodType = QuotaPeriodType.Day,
                MaxRequests = 1_000,
                MaxTokens = 2_000_000,
                WindowSeconds = 86_400,
                IsEnabled = true,
                TenantId = application.TenantId,
            });
        }

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
