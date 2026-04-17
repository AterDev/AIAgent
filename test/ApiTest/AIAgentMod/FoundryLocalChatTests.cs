using ApiTest.Data;
using CoreMod.Models;
using Entity.ModelMod;
using ModelMod.Models.AIModelInfoDtos;
using ModelMod.Models.AIModelProviderDtos;
using ModelMod.Models.ApplicationApiKeyDtos;
using ModelMod.Models.ApplicationDtos;
using ModelMod.Models.ApplicationModelPermissionDtos;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Sockets;

namespace ApiTest.AIAgentMod;

/// <summary>
/// Foundry Local 本地推理集成测试：验证 OpenAiCompatible 路由 + 空 ApiKey 容忍。
/// 依赖本地已安装并运行的 Foundry Local 服务 (默认 http://127.0.0.1:55655)，
/// 以及已缓存并可加载的 qwen3-0.6b 模型。
/// 若环境不满足，则跳过测试（以避免 CI 失败）。
/// </summary>
public class FoundryLocalChatTests
{
    private const string FoundryHost = "127.0.0.1";
    private const int FoundryPort = 55655;
    private const string FoundryBaseUrl = "http://127.0.0.1:55655/v1";
    private const string FoundryModelId = "qwen3-0.6b-generic-cpu:4";

    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task FoundryLocal_ShouldServeChat_ViaApplicationApiKey(HttpClientDataClass httpClientData)
    {
        if (!await IsFoundryLocalReachableAsync())
        {
            // 环境不可用时跳过：不作为失败，仅做信息输出
            Console.WriteLine($"[SKIP] Foundry Local not reachable at {FoundryHost}:{FoundryPort}");
            return;
        }

        var adminClient = httpClientData.HttpClient;
        Guid applicationId = Guid.Empty;
        Guid providerId = Guid.Empty;
        Guid modelId = Guid.Empty;

        try
        {
            var provider = await CreateFoundryProviderAsync(adminClient);
            providerId = provider.Id;

            var model = await CreateFoundryModelAsync(adminClient, providerId);
            modelId = model.Id;

            var application = await CreateApplicationAsync(adminClient, "验证 Foundry Local 推理");
            applicationId = application.Id;

            await CreateApplicationModelPermissionAsync(adminClient, applicationId, modelId);
            var apiKey = await CreateApplicationApiKeyAsync(adminClient, applicationId, "FoundryLocal Key");

            using var apiClient = await CreateApiServiceClientAsync(apiKey.ApiKey);
            var response = await apiClient.PostAsJsonAsync("/api/v1/models/chat", new
            {
                model = model.Name,
                provider = provider.Name,
                scene = "FoundryLocalChatTest",
                temperature = 0.1,
                maxTokens = 64,
                messages = new[]
                {
                    new { role = "user", content = "Reply with the single word: OK" }
                }
            });

            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<ModelResponse>();
            await Assert.That(result).IsNotNull();
            await Assert.That(result!.Success).IsTrue();
            await Assert.That(result.ErrorMessage).IsNull();
            await Assert.That(string.IsNullOrWhiteSpace(result.Content)).IsFalse();
        }
        finally
        {
            if (modelId != Guid.Empty)
            {
                await adminClient.DeleteAsync($"/api/AIModelInfo/{modelId}");
            }
            if (providerId != Guid.Empty)
            {
                await adminClient.DeleteAsync($"/api/AIModelProvider/{providerId}");
            }
            if (applicationId != Guid.Empty)
            {
                await adminClient.DeleteAsync($"/api/Application/{applicationId}");
            }
        }
    }

    private static async Task<bool> IsFoundryLocalReachableAsync()
    {
        try
        {
            using var tcp = new TcpClient();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            await tcp.ConnectAsync(FoundryHost, FoundryPort, cts.Token);
            return tcp.Connected;
        }
        catch
        {
            return false;
        }
    }

    private static async Task<AIModelProvider> CreateFoundryProviderAsync(HttpClient adminClient)
    {
        var response = await adminClient.PostAsJsonAsync("/api/AIModelProvider", new AIModelProviderAddDto
        {
            Name = $"FoundryLocal {Guid.NewGuid().ToString()[..8]}",
            Description = "本地 Foundry Local OpenAI 兼容端点",
            Website = "https://learn.microsoft.com/azure/ai-studio/foundry-local",
            ApiKey = "not-required",
            BaseUrl = FoundryBaseUrl,
        });

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<AIModelProvider>())!;
    }

    private static async Task<AIModelInfo> CreateFoundryModelAsync(HttpClient adminClient, Guid providerId)
    {
        var response = await adminClient.PostAsJsonAsync("/api/AIModelInfo", new AIModelInfoAddDto
        {
            Name = FoundryModelId,
            DisplayName = "Qwen3 0.6B (Foundry Local)",
            Description = "Foundry Local 本地 CPU 推理模型",
            ProviderId = providerId,
            ContextLength = 4096,
            MaxContextTokens = 4096,
            SupportsChat = true,
            SupportsTools = true,
            SupportsEmbedding = false,
            SupportsVision = false,
            SupportsResponsesApi = false,
            InputPrice = 0,
            OutputPrice = 0,
            IsEnabled = true,
        });

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<AIModelInfo>())!;
    }

    private static async Task<ApplicationDetailDto> CreateApplicationAsync(HttpClient adminClient, string description)
    {
        var response = await adminClient.PostAsJsonAsync("/api/Application", new ApplicationAddDto
        {
            Name = $"FoundryApp {Guid.NewGuid().ToString()[..8]}",
            Description = description,
            IsEnabled = true,
        });

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<ApplicationDetailDto>())!;
    }

    private static async Task<ApplicationApiKeyCredentialResultDto> CreateApplicationApiKeyAsync(
        HttpClient adminClient, Guid applicationId, string keyName)
    {
        var response = await adminClient.PostAsJsonAsync($"/api/Application/{applicationId}/api-keys", new ApplicationApiKeyAddDto
        {
            Name = keyName,
            ApiKeyExpiresInMonths = 3,
        });

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<ApplicationApiKeyCredentialResultDto>())!;
    }

    private static async Task<ApplicationModelPermission> CreateApplicationModelPermissionAsync(
        HttpClient adminClient, Guid applicationId, Guid modelId)
    {
        var response = await adminClient.PostAsJsonAsync("/api/ApplicationModelPermission", new ApplicationModelPermissionAddDto
        {
            ApplicationId = applicationId,
            AIModelInfoId = modelId,
            IsEnabled = true,
        });

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<ApplicationModelPermission>())!;
    }

    private static async Task<HttpClient> CreateApiServiceClientAsync(string apiKey)
    {
        var client = (GlobalHooks.App ?? throw new NullReferenceException()).CreateHttpClient("ApiService");
        if (GlobalHooks.NotificationService != null)
        {
            await GlobalHooks.NotificationService
                .WaitForResourceAsync("ApiService", KnownResourceStates.Running)
                .WaitAsync(TimeSpan.FromSeconds(30));
        }
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        client.Timeout = TimeSpan.FromMinutes(3);
        return client;
    }
}
