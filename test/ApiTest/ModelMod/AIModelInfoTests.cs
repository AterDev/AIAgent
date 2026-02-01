using ApiTest.Data;
using ModelMod.Models.AIModelInfoDtos;
using ModelMod.Models.AIModelProviderDtos;
using Entity.ModelMod;
using Perigon.AspNetCore.Models;
using System.Net;
using System.Net.Http.Json;

namespace ApiTest.ModelMod;

/// <summary>
/// AI模型信息集成测试
/// </summary>
public class AIModelInfoTests
{
    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task AIModelInfoCRUD_ShouldWorkCorrectly(HttpClientDataClass httpClientData)
    {
        var httpClient = httpClientData.HttpClient;

        // 先创建一个提供商
        var providerDto = new AIModelProviderAddDto
        {
            Name = $"TestProvider_{Guid.NewGuid().ToString().Substring(0, 8)}",
            Description = "Test AI Provider",
            Website = "https://test.com"
        };

        var providerResponse = await httpClient.PostAsJsonAsync("/api/aiModelProvider", providerDto);
        await Assert.That(providerResponse.StatusCode).IsEqualTo(HttpStatusCode.Created);
        var provider = await providerResponse.Content.ReadFromJsonAsync<AIModelProvider>();
        var providerId = provider!.Id;

        // Add - 创建模型信息
        var addDto = new AIModelInfoAddDto
        {
            ProviderId = providerId,
            Name = "gpt-4o",
            DisplayName = "GPT-4 Omni",
            Description = "Most capable GPT-4 model",
            ContextLength = 128000,
            MaxContextTokens = 128000,
            SupportsChat = true,
            SupportsTools = true,
            SupportsVision = true,
            InputPrice = 0.005m,
            OutputPrice = 0.015m,
            IsEnabled = true
        };

        var addResponse = await httpClient.PostAsJsonAsync("/api/aiModelInfo", addDto);
        await Assert.That(addResponse.StatusCode).IsEqualTo(HttpStatusCode.Created);

        var addedModel = await addResponse.Content.ReadFromJsonAsync<AIModelInfo>();
        await Assert.That(addedModel).IsNotNull();
        await Assert.That(addedModel!.Name).IsEqualTo(addDto.Name);
        await Assert.That(addedModel.ProviderId).IsEqualTo(providerId);
        var modelId = addedModel.Id;

        // Get - 获取模型详情
        var getResponse = await httpClient.GetAsync($"/api/aiModelInfo/{modelId}");
        await Assert.That(getResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var modelDetail = await getResponse.Content.ReadFromJsonAsync<AIModelInfoDetailDto>();
        await Assert.That(modelDetail).IsNotNull();
        await Assert.That(modelDetail!.Name).IsEqualTo(addDto.Name);
        await Assert.That(modelDetail.SupportsChat).IsTrue();

        // Update - 更新模型
        var updateDto = new AIModelInfoUpdateDto
        {
            DisplayName = "GPT-4 Omni Updated",
            Description = "Updated description",
            InputPrice = 0.006m,
            OutputPrice = 0.016m
        };

        var updateResponse = await httpClient.PatchAsJsonAsync($"/api/aiModelInfo/{modelId}", updateDto);
        await Assert.That(updateResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var updateResult = await updateResponse.Content.ReadFromJsonAsync<bool>();
        await Assert.That(updateResult).IsTrue();

        // Verify Update
        var verifyResponse = await httpClient.GetAsync($"/api/aiModelInfo/{modelId}");
        var updatedModel = await verifyResponse.Content.ReadFromJsonAsync<AIModelInfoDetailDto>();
        await Assert.That(updatedModel!.DisplayName).IsEqualTo(updateDto.DisplayName);
        await Assert.That(updatedModel.InputPrice).IsEqualTo(updateDto.InputPrice);

        // Delete - 删除模型
        var deleteResponse = await httpClient.DeleteAsync($"/api/aiModelInfo/{modelId}");
        await Assert.That(deleteResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var deleteResult = await deleteResponse.Content.ReadFromJsonAsync<bool>();
        await Assert.That(deleteResult).IsTrue();

        // Verify Delete (soft delete returns NoContent)
        var verifyDeleteResponse = await httpClient.GetAsync($"/api/aiModelInfo/{modelId}");
        await Assert.That(verifyDeleteResponse.StatusCode == HttpStatusCode.NotFound || verifyDeleteResponse.StatusCode == HttpStatusCode.NoContent).IsTrue();

        // 清理提供商
        await httpClient.DeleteAsync($"/api/aiModelProvider/{providerId}");
    }

    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task ListAIModelInfos_ShouldReturnPagedResults(HttpClientDataClass httpClientData)
    {
        var httpClient = httpClientData.HttpClient;

        var filterDto = new AIModelInfoFilterDto
        {
            PageIndex = 1,
            PageSize = 10
        };

        var response = await httpClient.PostAsJsonAsync("/api/aiModelInfo/filter", filterDto);
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PageList<AIModelInfoItemDto>>();
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.Data).IsNotNull();
    }

    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task FilterAIModelInfos_ByProvider_ShouldReturnFiltered(HttpClientDataClass httpClientData)
    {
        var httpClient = httpClientData.HttpClient;

        // 创建提供商
        var providerDto = new AIModelProviderAddDto
        {
            Name = $"OpenAI_{Guid.NewGuid().ToString().Substring(0, 8)}",
            Description = "OpenAI Provider",
            Website = "https://openai.com"
        };

        var providerResponse = await httpClient.PostAsJsonAsync("/api/aiModelProvider", providerDto);
        var provider = await providerResponse.Content.ReadFromJsonAsync<AIModelProvider>();
        var providerId = provider!.Id;

        // 创建模型
        var model1 = new AIModelInfoAddDto
        {
            ProviderId = providerId,
            Name = "gpt-4",
            DisplayName = "GPT-4",
            ContextLength = 8192,
            MaxContextTokens = 8192,
            SupportsChat = true,
            InputPrice = 0.03m,
            OutputPrice = 0.06m
        };

        var model2 = new AIModelInfoAddDto
        {
            ProviderId = providerId,
            Name = "gpt-3.5-turbo",
            DisplayName = "GPT-3.5 Turbo",
            ContextLength = 16385,
            MaxContextTokens = 16385,
            SupportsChat = true,
            InputPrice = 0.0005m,
            OutputPrice = 0.0015m
        };

        await httpClient.PostAsJsonAsync("/api/aiModelInfo", model1);
        await httpClient.PostAsJsonAsync("/api/aiModelInfo", model2);

        // 按提供商筛选
        var filterDto = new AIModelInfoFilterDto
        {
            ProviderId = providerId,
            PageIndex = 1,
            PageSize = 10
        };

        var response = await httpClient.PostAsJsonAsync("/api/aiModelInfo/filter", filterDto);
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PageList<AIModelInfoItemDto>>();
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.Data.Count).IsGreaterThanOrEqualTo(2);
        await Assert.That(result.Data.All(m => m.ProviderId == providerId)).IsTrue();

        // 清理
        await httpClient.DeleteAsync($"/api/aiModelProvider/{providerId}");
    }

    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task CreateMultipleModels_WithDifferentCapabilities_ShouldSucceed(HttpClientDataClass httpClientData)
    {
        var httpClient = httpClientData.HttpClient;

        // 创建提供商
        var providerDto = new AIModelProviderAddDto
        {
            Name = $"Multi_{Guid.NewGuid().ToString().Substring(0, 8)}",
            Description = "Multi-Model Provider",
            Website = "https://multi.com"
        };

        var providerResponse = await httpClient.PostAsJsonAsync("/api/aiModelProvider", providerDto);
        var provider = await providerResponse.Content.ReadFromJsonAsync<AIModelProvider>();
        var providerId = provider!.Id;

        // 聊天模型
        var chatModel = new AIModelInfoAddDto
        {
            ProviderId = providerId,
            Name = "chat-model",
            DisplayName = "Chat Model",
            ContextLength = 4096,
            MaxContextTokens = 4096,
            SupportsChat = true,
            SupportsTools = true,
            InputPrice = 0.001m,
            OutputPrice = 0.002m
        };

        // 嵌入模型
        var embeddingModel = new AIModelInfoAddDto
        {
            ProviderId = providerId,
            Name = "embedding-model",
            DisplayName = "Embedding Model",
            ContextLength = 8191,
            MaxContextTokens = 8191,
            SupportsEmbedding = true,
            InputPrice = 0.0001m,
            OutputPrice = 0.0m
        };

        // 视觉模型
        var visionModel = new AIModelInfoAddDto
        {
            ProviderId = providerId,
            Name = "vision-model",
            DisplayName = "Vision Model",
            ContextLength = 128000,
            MaxContextTokens = 128000,
            SupportsChat = true,
            SupportsVision = true,
            InputPrice = 0.01m,
            OutputPrice = 0.03m
        };

        var response1 = await httpClient.PostAsJsonAsync("/api/aiModelInfo", chatModel);
        var response2 = await httpClient.PostAsJsonAsync("/api/aiModelInfo", embeddingModel);
        var response3 = await httpClient.PostAsJsonAsync("/api/aiModelInfo", visionModel);

        await Assert.That(response1.StatusCode).IsEqualTo(HttpStatusCode.Created);
        await Assert.That(response2.StatusCode).IsEqualTo(HttpStatusCode.Created);
        await Assert.That(response3.StatusCode).IsEqualTo(HttpStatusCode.Created);

        var model1 = await response1.Content.ReadFromJsonAsync<AIModelInfo>();
        var model2 = await response2.Content.ReadFromJsonAsync<AIModelInfo>();
        var model3 = await response3.Content.ReadFromJsonAsync<AIModelInfo>();

        await Assert.That(model1!.SupportsChat).IsTrue();
        await Assert.That(model2!.SupportsEmbedding).IsTrue();
        await Assert.That(model3!.SupportsVision).IsTrue();

        // 清理
        await httpClient.DeleteAsync($"/api/aiModelProvider/{providerId}");
    }

    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task CreateModelWithDifferentEnabledStates_ShouldSucceed(HttpClientDataClass httpClientData)
    {
        var httpClient = httpClientData.HttpClient;

        // 创建提供商
        var providerDto = new AIModelProviderAddDto
        {
            Name = $"Enabled_{Guid.NewGuid().ToString().Substring(0, 8)}",
            Description = "Enabled Provider",
            Website = "https://enabled.com"
        };

        var providerResponse = await httpClient.PostAsJsonAsync("/api/aiModelProvider", providerDto);
        var provider = await providerResponse.Content.ReadFromJsonAsync<AIModelProvider>();
        var providerId = provider!.Id;

        // 创建启用和禁用的模型
        var enabledModel = new AIModelInfoAddDto
        {
            ProviderId = providerId,
            Name = "enabled-model",
            ContextLength = 4096,
            MaxContextTokens = 4096,
            IsEnabled = true
        };

        var disabledModel = new AIModelInfoAddDto
        {
            ProviderId = providerId,
            Name = "disabled-model",
            ContextLength = 4096,
            MaxContextTokens = 4096,
            IsEnabled = false
        };

        var response1 = await httpClient.PostAsJsonAsync("/api/aiModelInfo", enabledModel);
        var response2 = await httpClient.PostAsJsonAsync("/api/aiModelInfo", disabledModel);

        await Assert.That(response1.StatusCode).IsEqualTo(HttpStatusCode.Created);
        await Assert.That(response2.StatusCode).IsEqualTo(HttpStatusCode.Created);

        var model1 = await response1.Content.ReadFromJsonAsync<AIModelInfo>();
        var model2 = await response2.Content.ReadFromJsonAsync<AIModelInfo>();

        await Assert.That(model1!.IsEnabled).IsTrue();
        await Assert.That(model2!.IsEnabled).IsFalse();

        // 清理
        await httpClient.DeleteAsync($"/api/aiModelProvider/{providerId}");
    }
}
