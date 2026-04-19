using AIAgentMod.Models.AIAgentDtos;
using AIAgentMod.Models.AgentExecutionDtos;
using ApiTest.Data;
using CoreMod.Models;
using CoreMod.Models.RagQuery;
using Entity.AIAgentMod;
using Entity.KnowledgeBaseMod;
using Entity.McpMod;
using Entity.ModelMod;
using KnowledgeBaseMod.Models.RagCollectionDtos;
using KnowledgeBaseMod.Models.RagDocumentDtos;
using McpMod.Models.McpToolDtos;
using ModelMod.Models.AIModelInfoDtos;
using ModelMod.Models.ApplicationApiKeyDtos;
using ModelMod.Models.ApplicationModelPermissionDtos;
using ModelMod.Models.ApplicationToolPermissionDtos;
using ModelMod.Models.AIModelProviderDtos;
using ModelMod.Models.ApplicationDtos;
using Perigon.AspNetCore.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ApiTest.ModelMod;

/// <summary>
/// 应用 API Key 集成测试
/// </summary>
public class ApplicationApiKeyTests
{
    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task ApplicationApiKeyCRUD_ShouldWorkCorrectly(HttpClientDataClass httpClientData)
    {
        var adminClient = httpClientData.HttpClient;
        Guid applicationId = Guid.Empty;

        try
        {
            var addAppDto = new ApplicationAddDto
            {
                Name = $"Test App {Guid.NewGuid().ToString()[..8]}",
                Description = "用于测试 API Key CRUD 的应用",
                IsEnabled = true
            };

            var addAppResponse = await adminClient.PostAsJsonAsync("/api/Application", addAppDto);
            await AssertStatusCodeAsync(addAppResponse, HttpStatusCode.OK);

            var application = await addAppResponse.Content.ReadFromJsonAsync<ApplicationDetailDto>();
            await Assert.That(application).IsNotNull();
            applicationId = application!.Id;

            var emptyListResponse = await adminClient.GetAsync($"/api/Application/{applicationId}/api-keys");
            await AssertStatusCodeAsync(emptyListResponse, HttpStatusCode.OK);

            var emptyKeys = await emptyListResponse.Content.ReadFromJsonAsync<List<ApplicationApiKeyItemDto>>();
            await Assert.That(emptyKeys).IsNotNull();
            await Assert.That(emptyKeys!.Count).IsEqualTo(0);

            var addKeyDto = new ApplicationApiKeyAddDto
            {
                Name = "Primary Key",
                ApiKeyExpiresInMonths = 3
            };

            var addKeyResponse = await adminClient.PostAsJsonAsync($"/api/Application/{applicationId}/api-keys", addKeyDto);
            await AssertStatusCodeAsync(addKeyResponse, HttpStatusCode.OK);

            var apiKeyResult = await addKeyResponse.Content.ReadFromJsonAsync<ApplicationApiKeyCredentialResultDto>();
            await Assert.That(apiKeyResult).IsNotNull();
            await Assert.That(apiKeyResult!.ApplicationId).IsEqualTo(applicationId);
            await Assert.That(apiKeyResult.Name).IsEqualTo(addKeyDto.Name);
            await Assert.That(apiKeyResult.ApiKey).StartsWith("sk-");
            await Assert.That(apiKeyResult.ApiKey.Length).IsEqualTo(35);

            var listResponse = await adminClient.GetAsync($"/api/Application/{applicationId}/api-keys");
            await AssertStatusCodeAsync(listResponse, HttpStatusCode.OK);

            var keys = await listResponse.Content.ReadFromJsonAsync<List<ApplicationApiKeyItemDto>>();
            await Assert.That(keys).IsNotNull();
            await Assert.That(keys!.Count).IsEqualTo(1);
            await Assert.That(keys[0].Id).IsEqualTo(apiKeyResult.Id);
            await Assert.That(keys[0].Name).IsEqualTo(addKeyDto.Name);

            var deleteResponse = await adminClient.DeleteAsync($"/api/Application/{applicationId}/api-keys/{apiKeyResult.Id}");
            await AssertStatusCodeAsync(deleteResponse, HttpStatusCode.OK);

            var deleteResult = await deleteResponse.Content.ReadFromJsonAsync<bool>();
            await Assert.That(deleteResult).IsTrue();

            var verifyDeleteResponse = await adminClient.GetAsync($"/api/Application/{applicationId}/api-keys");
            var deletedKeys = await verifyDeleteResponse.Content.ReadFromJsonAsync<List<ApplicationApiKeyItemDto>>();
            await Assert.That(deletedKeys).IsNotNull();
            await Assert.That(deletedKeys!.Count).IsEqualTo(0);
        }
        finally
        {
            if (applicationId != Guid.Empty)
            {
                await adminClient.DeleteAsync($"/api/Application/{applicationId}");
            }
        }
    }

    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task ApplicationApiKey_ShouldBindIdentityToCapabilities_AndRejectInvalidKeys(HttpClientDataClass httpClientData)
    {
        var adminClient = httpClientData.HttpClient;
        Guid applicationId = Guid.Empty;
        Guid otherApplicationId = Guid.Empty;
        Guid providerId = Guid.Empty;
        Guid modelId = Guid.Empty;

        try
        {
            applicationId = (await CreateApplicationAsync(adminClient, "用于测试开放平台鉴权")).Id;
            otherApplicationId = (await CreateApplicationAsync(adminClient, "用于验证请求体中的 applicationId 不会覆盖 ApiKey 身份")).Id;

            var apiKeyResult = await CreateApplicationApiKeyAsync(adminClient, applicationId, "OpenPlatform Key");

            const string expectedReply = "FAKE_MODEL_OK: identity is derived from api key";

            await using var fakeServer = await FakeOpenAiCompatibleServer.StartAsync(expectedReply);

            var provider = await CreateModelProviderAsync(adminClient, fakeServer.BaseUri);
            providerId = provider.Id;

            var model = await CreateModelAsync(adminClient, providerId);
            modelId = model.Id;

            await CreateApplicationModelPermissionAsync(adminClient, applicationId, modelId);

            using var apiClient = await CreateApiServiceClientAsync(apiKeyResult.ApiKey);

            var modelResponse = await apiClient.PostAsJsonAsync("/api/v1/models/chat", new
            {
                applicationId = otherApplicationId,
                model = model.Name,
                scene = "ApiKeyIdentityBinding",
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = "请返回固定测试结果"
                    }
                },
            });
            await AssertStatusCodeAsync(modelResponse, HttpStatusCode.OK);

            var modelResult = await ReadRequiredJsonAsync<ModelResponse>(modelResponse);
            await Assert.That(modelResult.Success).IsTrue();
            await Assert.That(modelResult.Content).IsEqualTo(expectedReply);

            using var invalidFormatClient = await CreateApiServiceClientAsync("invalid-api-key");
            var invalidFormatResponse = await invalidFormatClient.PostAsJsonAsync("/api/v1/rag/search", new RagQueryRequest
            {
                Query = "invalid api key",
                TopK = 1,
            });
            await AssertStatusCodeAsync(invalidFormatResponse, HttpStatusCode.Unauthorized);

            using var invalidValueClient = await CreateApiServiceClientAsync($"sk-{Guid.NewGuid():N}");
            var invalidValueResponse = await invalidValueClient.PostAsJsonAsync("/api/v1/rag/search", new RagQueryRequest
            {
                Query = "invalid api key",
                TopK = 1,
            });
            await AssertStatusCodeAsync(invalidValueResponse, HttpStatusCode.Unauthorized);
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

            if (otherApplicationId != Guid.Empty)
            {
                await adminClient.DeleteAsync($"/api/Application/{otherApplicationId}");
            }
        }
    }

    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task ApplicationApiKey_ShouldAccessRagSearch(HttpClientDataClass httpClientData)
    {
        var adminClient = httpClientData.HttpClient;
        Guid applicationId = Guid.Empty;

        try
        {
            var application = await CreateApplicationAsync(adminClient, "用于验证应用 ApiKey 的 RAG 访问能力");
            applicationId = application.Id;

            var apiKeyResult = await CreateApplicationApiKeyAsync(adminClient, applicationId, "RAG Key");

            using var apiClient = await CreateApiServiceClientAsync(apiKeyResult.ApiKey);
            var response = await apiClient.PostAsJsonAsync("/api/v1/rag/search", new RagQueryRequest
            {
                Query = "DefaultKnowledgeBase",
                TopK = 3,
            });

            await AssertStatusCodeAsync(response, HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<RagQueryResult>();
            await Assert.That(result).IsNotNull();
            await Assert.That(result!.Items).IsNotNull();
        }
        finally
        {
            if (applicationId != Guid.Empty)
            {
                await adminClient.DeleteAsync($"/api/Application/{applicationId}");
            }
        }
    }

    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task ApplicationApiKey_ShouldExecuteAgent_AndInvokeModel(HttpClientDataClass httpClientData)
    {
        const string expectedReply = "FAKE_MODEL_OK: app api key reached agent+model pipeline";

        var adminClient = httpClientData.HttpClient;
        Guid applicationId = Guid.Empty;
        Guid providerId = Guid.Empty;
        Guid modelId = Guid.Empty;
        Guid templateAgentId = Guid.Empty;
        Guid clonedAgentId = Guid.Empty;

        await using var fakeServer = await FakeOpenAiCompatibleServer.StartAsync(expectedReply);

        try
        {
            var provider = await CreateModelProviderAsync(adminClient, fakeServer.BaseUri);
            providerId = provider.Id;
            var model = await CreateModelAsync(adminClient, provider.Id);
            modelId = model.Id;

            var application = await CreateApplicationAsync(adminClient, "用于验证应用 ApiKey 的 Agent/模型执行能力");
            applicationId = application.Id;

            await CreateApplicationModelPermissionAsync(adminClient, application.Id, model.Id);
            var apiKeyResult = await CreateApplicationApiKeyAsync(adminClient, application.Id, "Agent Key");
            var templateAgent = await CreateAgentAsync(adminClient, model.Name, isPublic: true);
            templateAgentId = templateAgent.Id;

            using var apiClient = await CreateApiServiceClientAsync(apiKeyResult.ApiKey);
            var cloneResponse = await apiClient.PostAsync($"/api/v1/agents/templates/{templateAgent.Id}/clone", content: null);
            await AssertStatusCodeAsync(cloneResponse, HttpStatusCode.Created);

            var clonedAgent = await ReadRequiredJsonAsync<ApplicationAgent>(cloneResponse);
            clonedAgentId = clonedAgent.Id;

            var executeResponse = await apiClient.PostAsJsonAsync($"/api/v1/agents/{clonedAgent.Id}/execute", new AgentExecuteRequestDto
            {
                InputJson = JsonSerializer.Serialize(new { prompt = "请直接回一句测试通过" })
            });

            await AssertStatusCodeAsync(executeResponse, HttpStatusCode.Accepted);

            using var executePayload = JsonDocument.Parse(await executeResponse.Content.ReadAsStringAsync());
            var executionId = executePayload.RootElement.GetProperty("executionId").GetGuid();

            var execution = await WaitForAgentExecutionCompletedAsync(adminClient, executionId, TimeSpan.FromSeconds(30));

            await Assert.That(execution.Status).IsEqualTo(AgentExecutionStatus.Completed);
            await Assert.That(execution.ErrorMessage).IsNull();

            using var outputPayload = JsonDocument.Parse(execution.OutputJson ?? "{}");
            var finalResponse = outputPayload.RootElement.GetProperty("final_response").GetString();
            await Assert.That(finalResponse).IsEqualTo(expectedReply);

            await Assert.That(fakeServer.RequestCount).IsGreaterThan(0);
            await Assert.That(fakeServer.ReceivedBodies.TryPeek(out var requestBody)).IsTrue();
            await Assert.That(requestBody).Contains(model.Name);
        }
        finally
        {
            if (clonedAgentId != Guid.Empty)
            {
                await adminClient.DeleteAsync($"/api/ApplicationAgent/{clonedAgentId}");
            }

            if (templateAgentId != Guid.Empty)
            {
                await adminClient.DeleteAsync($"/api/AIAgent/{templateAgentId}");
            }

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

    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task ApplicationApiKey_ShouldCallModelDirectly(HttpClientDataClass httpClientData)
    {
        const string expectedReply = "FAKE_MODEL_OK: direct model chat via app api key";

        var adminClient = httpClientData.HttpClient;
        Guid applicationId = Guid.Empty;
        Guid providerId = Guid.Empty;
        Guid modelId = Guid.Empty;

        await using var fakeServer = await FakeOpenAiCompatibleServer.StartAsync(expectedReply);

        try
        {
            var provider = await CreateModelProviderAsync(adminClient, fakeServer.BaseUri);
            providerId = provider.Id;
            var model = await CreateModelAsync(adminClient, provider.Id);
            modelId = model.Id;

            var application = await CreateApplicationAsync(adminClient, "用于验证应用 ApiKey 的模型直调能力");
            applicationId = application.Id;

            await CreateApplicationModelPermissionAsync(adminClient, application.Id, model.Id);
            var apiKeyResult = await CreateApplicationApiKeyAsync(adminClient, application.Id, "Model Key");

            using var apiClient = await CreateApiServiceClientAsync(apiKeyResult.ApiKey);
            var response = await apiClient.PostAsJsonAsync("/api/v1/models/chat", new
            {
                model = model.Name,
                provider = provider.Name,
                scene = "OpenPlatformDirectModelTest",
                temperature = 0.2,
                maxTokens = 128,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = "请返回集成测试成功",
                    }
                }
            });

            await AssertStatusCodeAsync(response, HttpStatusCode.OK);

            var result = await ReadRequiredJsonAsync<ModelResponse>(response);
            await Assert.That(result.Success).IsTrue();
            await Assert.That(result.ErrorMessage).IsNull();
            await Assert.That(result.Content).IsEqualTo(expectedReply);
            await Assert.That(fakeServer.RequestCount).IsGreaterThan(0);
            await Assert.That(fakeServer.ReceivedBodies.TryPeek(out var requestBody)).IsTrue();
            await Assert.That(requestBody).Contains(model.Name);
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

    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task ApplicationApiKey_ShouldCreateAndUnlinkOwnRagCollection(HttpClientDataClass httpClientData)
    {
        var adminClient = httpClientData.HttpClient;
        Guid applicationId = Guid.Empty;

        try
        {
            var application = await CreateApplicationAsync(adminClient, "用于验证应用 ApiKey 的知识库创建与解绑能力");
            applicationId = application.Id;

            var apiKeyResult = await CreateApplicationApiKeyAsync(adminClient, application.Id, "Rag Collection Key");

            using var apiClient = await CreateApiServiceClientAsync(apiKeyResult.ApiKey);

            var createResponse = await apiClient.PostAsJsonAsync("/api/v1/rag/collections", new
            {
                name = $"OpenPlatform KB {Guid.NewGuid():N}"[..24],
                description = "通过开放平台创建的知识库",
                isPublic = false,
                isEnabled = true,
                tags = new[] { "integration", "rag" }
            });
            await AssertStatusCodeAsync(createResponse, HttpStatusCode.Created);

            var created = await ReadRequiredJsonAsync<RagCollection>(createResponse);

            var listResponse = await apiClient.PostAsJsonAsync("/api/v1/rag/collections/filter", new
            {
                pageIndex = 1,
                pageSize = 20,
                name = created.Name,
            });
            await AssertStatusCodeAsync(listResponse, HttpStatusCode.OK);

            using var listPayload = JsonDocument.Parse(await listResponse.Content.ReadAsStringAsync());
            var visibleCount = listPayload.RootElement.GetProperty("data").GetArrayLength();
            await Assert.That(visibleCount).IsEqualTo(1);

            var deleteResponse = await apiClient.DeleteAsync($"/api/v1/rag/collections/{created.Id}");
            await AssertStatusCodeAsync(deleteResponse, HttpStatusCode.OK);

            var verifyResponse = await apiClient.PostAsJsonAsync("/api/v1/rag/collections/filter", new
            {
                pageIndex = 1,
                pageSize = 20,
                name = created.Name,
            });
            await AssertStatusCodeAsync(verifyResponse, HttpStatusCode.OK);

            using var verifyPayload = JsonDocument.Parse(await verifyResponse.Content.ReadAsStringAsync());
            var remainingCount = verifyPayload.RootElement.GetProperty("data").GetArrayLength();
            await Assert.That(remainingCount).IsEqualTo(0);
        }
        finally
        {
            if (applicationId != Guid.Empty)
            {
                await adminClient.DeleteAsync($"/api/Application/{applicationId}");
            }
        }
    }

    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task ApplicationApiKey_ShouldUploadParseAndSearchRagDocument_UsingOpenPlatformPipeline(HttpClientDataClass httpClientData)
    {
        var adminClient = httpClientData.HttpClient;
        Guid applicationId = Guid.Empty;

        try
        {
            var scenario = OpenPlatformRagTestData.CreateScenario();
            var application = await CreateApplicationAsync(adminClient, "用于验证应用 ApiKey 的知识库上传、解析与检索链路");
            applicationId = application.Id;

            var apiKeyResult = await CreateApplicationApiKeyAsync(adminClient, application.Id, "Rag Pipeline Key");

            using var apiClient = await CreateApiServiceClientAsync(apiKeyResult.ApiKey);

            var createCollectionResponse = await apiClient.PostAsJsonAsync("/api/v1/rag/collections", new RagCollectionAddDto
            {
                Name = scenario.CollectionName,
                Description = "通过开放平台上传文档并验证解析与检索的测试集合",
                IsEnabled = true,
                Tags = ["integration", "rag", "pipeline"],
            });
            await AssertStatusCodeAsync(createCollectionResponse, HttpStatusCode.Created);

            var collection = await ReadRequiredJsonAsync<RagCollection>(createCollectionResponse);

            using var uploadContent = OpenPlatformRagTestData.CreateUploadContent(collection.Id, scenario);
            var uploadResponse = await apiClient.PostAsync("/api/v1/rag/documents/upload", uploadContent);
            await AssertStatusCodeAsync(uploadResponse, HttpStatusCode.Created);

            var document = await ReadRequiredJsonAsync<RagDocument>(uploadResponse);
            var documentDetail = await WaitForRagDocumentCompletedAsync(apiClient, document.Id, TimeSpan.FromMinutes(2));

            await Assert.That(documentDetail.Status).IsEqualTo(RagDocumentStatus.Completed);
            await Assert.That(documentDetail.ErrorMessage).IsNull();
            await Assert.That(documentDetail.ChunkCount).IsGreaterThan(0);
            await Assert.That(documentDetail.TokenCount).IsGreaterThan(0);

            var searchResponse = await apiClient.PostAsJsonAsync("/api/v1/rag/search", new RagQueryRequest
            {
                CollectionId = collection.Id,
                Query = scenario.SearchQuery,
                TopK = 3,
            });
            await AssertStatusCodeAsync(searchResponse, HttpStatusCode.OK);

            var searchResult = await searchResponse.Content.ReadFromJsonAsync<RagQueryResult>();
            await Assert.That(searchResult).IsNotNull();
            await Assert.That(searchResult!.Items.Count).IsGreaterThan(0);
            await Assert.That(searchResult.Items.Any(item => item.Content.Contains(scenario.UniqueCode, StringComparison.OrdinalIgnoreCase))).IsTrue();
        }
        finally
        {
            if (applicationId != Guid.Empty)
            {
                await adminClient.DeleteAsync($"/api/Application/{applicationId}");
            }
        }
    }

    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task ApplicationApiKey_ShouldExecuteAgentWithKnowledgeBaseTool_UsingDeepSeekAndOllama(HttpClientDataClass httpClientData)
    {
        var adminClient = httpClientData.HttpClient;
        Guid applicationId = Guid.Empty;
        Guid templateAgentId = Guid.Empty;
        Guid clonedAgentId = Guid.Empty;

        try
        {
            var scenario = OpenPlatformRagTestData.CreateScenario();
            var deepSeekChat = await GetModelByNameAsync(adminClient, "deepseek-chat");

            var application = await CreateApplicationAsync(adminClient, "用于验证应用 ApiKey 的 Agent + 知识库工具链路");
            applicationId = application.Id;

            await CreateApplicationModelPermissionAsync(adminClient, application.Id, deepSeekChat.Id);
            await EnsureBuiltinToolAsync(adminClient, "query_knowledge_base", "查询知识库并返回最相关的文档片段。", QueryKnowledgeBaseSchemaJson);
            await CreateApplicationToolPermissionAsync(adminClient, application.Id, "query_knowledge_base");

            var apiKeyResult = await CreateApplicationApiKeyAsync(adminClient, application.Id, "Agent Rag Key");

            using var apiClient = await CreateApiServiceClientAsync(apiKeyResult.ApiKey);

            var createCollectionResponse = await apiClient.PostAsJsonAsync("/api/v1/rag/collections", new RagCollectionAddDto
            {
                Name = scenario.CollectionName,
                Description = "用于开放平台 Agent 调用知识库的测试集合",
                IsEnabled = true,
                Tags = ["integration", "agent", "rag"],
            });
            await AssertStatusCodeAsync(createCollectionResponse, HttpStatusCode.Created);

            var collection = await ReadRequiredJsonAsync<RagCollection>(createCollectionResponse);

            using var uploadContent = OpenPlatformRagTestData.CreateUploadContent(collection.Id, scenario);
            var uploadResponse = await apiClient.PostAsync("/api/v1/rag/documents/upload", uploadContent);
            await AssertStatusCodeAsync(uploadResponse, HttpStatusCode.Created);

            var document = await ReadRequiredJsonAsync<RagDocument>(uploadResponse);
            var documentDetail = await WaitForRagDocumentCompletedAsync(apiClient, document.Id, TimeSpan.FromMinutes(2));
            await Assert.That(documentDetail.Status).IsEqualTo(RagDocumentStatus.Completed);

            var templateAgent = await CreateAgentAsync(
                adminClient,
                deepSeekChat.Name ?? "deepseek-chat",
                isPublic: true,
                tools: ["query_knowledge_base"],
                systemPrompt: "你是知识库测试助手。回答前必须先调用 query_knowledge_base 工具查询知识库；如果拿到结果，只返回知识库中的精确答案，不要编造。",
                description: "用于验证开放平台 Agent 能否经由知识库工具返回精确答案。");
            templateAgentId = templateAgent.Id;

            var cloneResponse = await apiClient.PostAsync($"/api/v1/agents/templates/{templateAgent.Id}/clone", content: null);
            await AssertStatusCodeAsync(cloneResponse, HttpStatusCode.Created);

            var clonedAgent = await ReadRequiredJsonAsync<ApplicationAgent>(cloneResponse);
            clonedAgentId = clonedAgent.Id;

            var executeResponse = await apiClient.PostAsJsonAsync($"/api/v1/agents/{clonedAgent.Id}/execute", new AgentExecuteRequestDto
            {
                InputJson = JsonSerializer.Serialize(new { prompt = scenario.AgentQuestion })
            });
            await AssertStatusCodeAsync(executeResponse, HttpStatusCode.Accepted);

            using var executePayload = JsonDocument.Parse(await executeResponse.Content.ReadAsStringAsync());
            var executionId = executePayload.RootElement.GetProperty("executionId").GetGuid();

            var execution = await WaitForAgentExecutionCompletedAsync(adminClient, executionId, TimeSpan.FromMinutes(2));
            await Assert.That(execution.Status).IsEqualTo(AgentExecutionStatus.Completed);
            await Assert.That(execution.ErrorMessage).IsNull();

            using var outputPayload = JsonDocument.Parse(execution.OutputJson ?? "{}");
            var finalResponse = outputPayload.RootElement.GetProperty("final_response").GetString();
            await Assert.That(finalResponse).IsNotNullOrEmpty();
            await Assert.That(finalResponse!).Contains(scenario.UniqueCode);

            var toolResults = outputPayload.RootElement.GetProperty("tool_results");
            await Assert.That(toolResults.GetArrayLength()).IsGreaterThan(0);
            await Assert.That(HasSuccessfulToolCall(toolResults, "query_knowledge_base")).IsTrue();
        }
        finally
        {
            if (clonedAgentId != Guid.Empty)
            {
                await adminClient.DeleteAsync($"/api/ApplicationAgent/{clonedAgentId}");
            }

            if (templateAgentId != Guid.Empty)
            {
                await adminClient.DeleteAsync($"/api/AIAgent/{templateAgentId}");
            }

            if (applicationId != Guid.Empty)
            {
                await adminClient.DeleteAsync($"/api/Application/{applicationId}");
            }
        }
    }

    private static async Task<HttpClient> CreateApiServiceClientAsync(string apiKey)
    {
        var client = (GlobalHooks.App ?? throw new NullReferenceException()).CreateHttpClient("ApiService");
        if (GlobalHooks.NotificationService != null)
        {
            await GlobalHooks.NotificationService.WaitForResourceAsync("ApiService", KnownResourceStates.Running)
                .WaitAsync(TimeSpan.FromSeconds(30));
        }

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        return client;
    }

    private static async Task<ApplicationDetailDto> CreateApplicationAsync(HttpClient adminClient, string description)
    {
        var response = await adminClient.PostAsJsonAsync("/api/Application", new ApplicationAddDto
        {
            Name = $"OpenApi App {Guid.NewGuid().ToString()[..8]}",
            Description = description,
            IsEnabled = true,
        });

        await AssertStatusCodeAsync(response, HttpStatusCode.OK);
        return await ReadRequiredJsonAsync<ApplicationDetailDto>(response);
    }

    private static async Task<ApplicationApiKeyCredentialResultDto> CreateApplicationApiKeyAsync(HttpClient adminClient, Guid applicationId, string keyName)
    {
        var response = await adminClient.PostAsJsonAsync($"/api/Application/{applicationId}/api-keys", new ApplicationApiKeyAddDto
        {
            Name = keyName,
            ApiKeyExpiresInMonths = 3,
        });

        await AssertStatusCodeAsync(response, HttpStatusCode.OK);
        return await ReadRequiredJsonAsync<ApplicationApiKeyCredentialResultDto>(response);
    }

    private static async Task<AIModelProvider> CreateModelProviderAsync(HttpClient adminClient, Uri baseUri)
    {
        var response = await adminClient.PostAsJsonAsync("/api/AIModelProvider", new AIModelProviderAddDto
        {
            Name = $"Fake Provider {Guid.NewGuid().ToString()[..8]}",
            Description = "用于开放平台应用 ApiKey 集成测试的本地 OpenAI 兼容服务",
            Website = "https://example.test/fake-provider",
            ApiKey = "fake-provider-key",
            BaseUrl = baseUri.AbsoluteUri.TrimEnd('/'),
        });

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);
        return await ReadRequiredJsonAsync<AIModelProvider>(response);
    }

    private static async Task<AIModelInfo> CreateModelAsync(HttpClient adminClient, Guid providerId)
    {
        var modelName = $"fake-openai-{Guid.NewGuid():N}";
        var response = await adminClient.PostAsJsonAsync("/api/AIModelInfo", new AIModelInfoAddDto
        {
            Name = modelName,
            DisplayName = "Fake OpenAI Chat Model",
            Description = "用于开放平台应用 ApiKey 集成测试的临时模型",
            ProviderId = providerId,
            ContextLength = 8192,
            MaxContextTokens = 8192,
            SupportsChat = true,
            SupportsTools = false,
            SupportsEmbedding = false,
            SupportsVision = false,
            SupportsResponsesApi = false,
            InputPrice = 0,
            OutputPrice = 0,
            IsEnabled = true,
        });

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);
        return await ReadRequiredJsonAsync<AIModelInfo>(response);
    }

    private static async Task<ApplicationModelPermission> CreateApplicationModelPermissionAsync(HttpClient adminClient, Guid applicationId, Guid modelId)
    {
        var response = await adminClient.PostAsJsonAsync("/api/ApplicationModelPermission", new ApplicationModelPermissionAddDto
        {
            ApplicationId = applicationId,
            AIModelInfoId = modelId,
            IsEnabled = true,
        });

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);
        return await ReadRequiredJsonAsync<ApplicationModelPermission>(response);
    }

    private static async Task<ApplicationToolPermission> CreateApplicationToolPermissionAsync(HttpClient adminClient, Guid applicationId, string toolName)
    {
        var response = await adminClient.PostAsJsonAsync("/api/ApplicationToolPermission", new ApplicationToolPermissionAddDto
        {
            ApplicationId = applicationId,
            ToolName = toolName,
            IsEnabled = true,
        });

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);
        return await ReadRequiredJsonAsync<ApplicationToolPermission>(response);
    }

    private static async Task<AIModelInfoItemDto> GetModelByNameAsync(HttpClient adminClient, string modelName)
    {
        var response = await adminClient.PostAsJsonAsync("/api/AIModelInfo/filter", new AIModelInfoFilterDto
        {
            PageIndex = 1,
            PageSize = 200,
        });

        await AssertStatusCodeAsync(response, HttpStatusCode.OK);
        var pagedResult = await response.Content.ReadFromJsonAsync<PageList<AIModelInfoItemDto>>();
        await Assert.That(pagedResult).IsNotNull();

        var model = pagedResult!.Data.FirstOrDefault(item => string.Equals(item.Name, modelName, StringComparison.OrdinalIgnoreCase));
        await Assert.That(model).IsNotNull();
        return model!;
    }

    private static async Task<McpToolItemDto> EnsureBuiltinToolAsync(HttpClient adminClient, string toolName, string description, string schemaJson)
    {
        var listResponse = await adminClient.PostAsJsonAsync("/api/McpTool/filter", new McpToolFilterDto
        {
            PageIndex = 1,
            PageSize = 50,
            Name = toolName,
        });
        await AssertStatusCodeAsync(listResponse, HttpStatusCode.OK);

        var existingPage = await listResponse.Content.ReadFromJsonAsync<PageList<McpToolItemDto>>();
        await Assert.That(existingPage).IsNotNull();

        var existing = existingPage!.Data.FirstOrDefault(item => string.Equals(item.Name, toolName, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            var detailResponse = await adminClient.GetAsync($"/api/McpTool/{existing.Id}");
            await AssertStatusCodeAsync(detailResponse, HttpStatusCode.OK);

            var detail = await ReadRequiredJsonAsync<McpToolDetailDto>(detailResponse);
            if (!detail.IsEnabled
                || detail.ToolType != McpToolType.Builtin
                || !string.Equals(detail.SchemaJson, schemaJson, StringComparison.Ordinal)
                || !string.Equals(detail.Description, description, StringComparison.Ordinal)
                || !string.Equals(detail.Version, "1.0.0", StringComparison.Ordinal))
            {
                var updateResponse = await adminClient.PatchAsJsonAsync($"/api/McpTool/{existing.Id}", new McpToolUpdateDto
                {
                    Name = toolName,
                    Description = description,
                    ToolType = McpToolType.Builtin,
                    IsEnabled = true,
                    Version = "1.0.0",
                    SchemaJson = schemaJson,
                    ServerId = null,
                });
                await AssertStatusCodeAsync(updateResponse, HttpStatusCode.OK);
            }

            return existing;
        }

        var createResponse = await adminClient.PostAsJsonAsync("/api/McpTool", new McpToolAddDto
        {
            Name = toolName,
            Description = description,
            ToolType = McpToolType.Builtin,
            IsEnabled = true,
            Version = "1.0.0",
            SchemaJson = schemaJson,
        });
        await AssertStatusCodeAsync(createResponse, HttpStatusCode.Created);

        var created = await ReadRequiredJsonAsync<Entity.McpMod.McpTool>(createResponse);
        return new McpToolItemDto
        {
            Id = created.Id,
            Name = created.Name,
            ToolType = created.ToolType,
            IsEnabled = created.IsEnabled,
        };
    }

    private static async Task<AIAgent> CreateAgentAsync(
        HttpClient adminClient,
        string modelName,
        Guid? applicationId = null,
        bool isPublic = false,
        List<string>? tools = null,
        string? systemPrompt = null,
        string? description = null)
    {
        var response = await adminClient.PostAsJsonAsync("/api/aiagent", new AIAgentAddDto
        {
            Name = $"OpenApi Agent {Guid.NewGuid().ToString()[..8]}",
            Description = description ?? "用于验证应用 ApiKey 调用 Agent",
            ModelId = modelName,
            SystemPrompt = systemPrompt ?? "你是集成测试助手，请简洁回答。",
            Tools = tools ?? [],
            Enable = true,
            IsPublic = isPublic,
            ApplicationId = applicationId,
        });

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);
        return await ReadRequiredJsonAsync<AIAgent>(response);
    }

    private static async Task<RagDocumentDetailDto> WaitForRagDocumentCompletedAsync(HttpClient apiClient, Guid documentId, TimeSpan timeout)
    {
        var startTime = DateTimeOffset.UtcNow;

        while (DateTimeOffset.UtcNow - startTime < timeout)
        {
            var response = await apiClient.GetAsync($"/api/v1/rag/documents/{documentId}");
            await AssertStatusCodeAsync(response, HttpStatusCode.OK);

            var document = await ReadRequiredJsonAsync<RagDocumentDetailDto>(response);
            if (document.Status is RagDocumentStatus.Completed or RagDocumentStatus.Failed)
            {
                return document;
            }

            await Task.Delay(500);
        }

        throw new TimeoutException($"RAG document {documentId} did not finish parsing within {timeout.TotalSeconds} seconds.");
    }

    private static async Task<AgentExecutionDetailDto> WaitForAgentExecutionCompletedAsync(HttpClient adminClient, Guid executionId, TimeSpan timeout)
    {
        var startTime = DateTimeOffset.UtcNow;

        while (DateTimeOffset.UtcNow - startTime < timeout)
        {
            var response = await adminClient.GetAsync($"/api/AgentExecution/{executionId}");
            await AssertStatusCodeAsync(response, HttpStatusCode.OK);

            var execution = await ReadRequiredJsonAsync<AgentExecutionDetailDto>(response);
            if (execution.Status != AgentExecutionStatus.Running)
            {
                return execution;
            }

            await Task.Delay(300);
        }

        throw new TimeoutException($"Agent execution {executionId} did not finish within {timeout.TotalSeconds} seconds.");
    }

    private static async Task<T> ReadRequiredJsonAsync<T>(HttpResponseMessage response)
        where T : class
    {
        var result = await response.Content.ReadFromJsonAsync<T>();
        await Assert.That(result).IsNotNull();
        return result!;
    }

    private static bool HasSuccessfulToolCall(JsonElement toolResults, string toolName)
    {
        foreach (var item in toolResults.EnumerateArray())
        {
            if (!item.TryGetProperty("tool", out var toolProperty)
                || !string.Equals(toolProperty.GetString(), toolName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (item.TryGetProperty("result", out var resultProperty))
            {
                if (resultProperty.TryGetProperty("Success", out var successProperty) && successProperty.ValueKind == JsonValueKind.True)
                {
                    return true;
                }

                if (resultProperty.TryGetProperty("success", out successProperty) && successProperty.ValueKind == JsonValueKind.True)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private const string QueryKnowledgeBaseSchemaJson = """
        {
                    "type": "object",
                    "properties": {
                        "query": { "type": "string" },
                        "topK": { "type": "integer", "minimum": 1, "maximum": 10 },
                        "collectionId": { "type": "string", "format": "uuid" }
          },
                    "required": ["query"]
        }
        """;

    private static async Task AssertStatusCodeAsync(HttpResponseMessage response, HttpStatusCode expectedStatusCode)
    {
        if (response.StatusCode != expectedStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Expected status {(int)expectedStatusCode} ({expectedStatusCode}), but received {(int)response.StatusCode} ({response.StatusCode}). Response body: {body}");
        }

        await Assert.That(response.StatusCode).IsEqualTo(expectedStatusCode);
    }
}