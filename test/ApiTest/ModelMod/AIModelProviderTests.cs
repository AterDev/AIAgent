using ApiTest.Data;
using ModelMod.Models.AIModelProviderDtos;
using Entity.ModelMod;
using Perigon.AspNetCore.Models;
using System.Net;
using System.Net.Http.Json;

namespace ApiTest.ModelMod;

/// <summary>
/// AI模型提供商集成测试
/// </summary>
public class AIModelProviderTests
{
    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task AIModelProviderCRUD_ShouldWorkCorrectly(HttpClientDataClass httpClientData)
    {
        var httpClient = httpClientData.HttpClient;

        // Add - 创建模型提供商
        var addDto = new AIModelProviderAddDto
        {
            Name = $"Test Provider {Guid.NewGuid().ToString().Substring(0, 8)}",
            Description = "这是一个测试模型提供商",
            Website = "https://openai.com",
            BaseUrl = "https://api.openai.com"
        };

        var addResponse = await httpClient.PostAsJsonAsync("/api/aimodelprovider", addDto);
        await Assert.That(addResponse.StatusCode).IsEqualTo(HttpStatusCode.Created);

        var addedProvider = await addResponse.Content.ReadFromJsonAsync<AIModelProvider>();
        await Assert.That(addedProvider).IsNotNull();
        await Assert.That(addedProvider!.Name).IsEqualTo(addDto.Name);
        var providerId = addedProvider.Id;

        // Get - 获取提供商详情
        var getResponse = await httpClient.GetAsync($"/api/aimodelprovider/{providerId}");
        await Assert.That(getResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var providerDetail = await getResponse.Content.ReadFromJsonAsync<AIModelProviderDetailDto>();
        await Assert.That(providerDetail).IsNotNull();
        await Assert.That(providerDetail!.Name).IsEqualTo(addDto.Name);

        // Update - 更新提供商
        var updateDto = new AIModelProviderUpdateDto
        {
            Name = $"Updated Provider {Guid.NewGuid().ToString().Substring(0, 8)}",
            Description = "更新后的描述"
        };

        var updateResponse = await httpClient.PatchAsJsonAsync($"/api/aimodelprovider/{providerId}", updateDto);
        await Assert.That(updateResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var updateResult = await updateResponse.Content.ReadFromJsonAsync<bool>();
        await Assert.That(updateResult).IsTrue();

        // Verify Update
        var verifyResponse = await httpClient.GetAsync($"/api/aimodelprovider/{providerId}");
        var updatedProvider = await verifyResponse.Content.ReadFromJsonAsync<AIModelProviderDetailDto>();
        await Assert.That(updatedProvider!.Name).IsEqualTo(updateDto.Name);

        // Delete - 删除提供商
        var deleteResponse = await httpClient.DeleteAsync($"/api/aimodelprovider/{providerId}");
        await Assert.That(deleteResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var deleteResult = await deleteResponse.Content.ReadFromJsonAsync<bool>();
        await Assert.That(deleteResult).IsTrue();

        // Verify Delete
        var verifyDeleteResponse = await httpClient.GetAsync($"/api/aimodelprovider/{providerId}");
        await Assert.That(verifyDeleteResponse.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task ListAIModelProviders_ShouldReturnPagedResults(HttpClientDataClass httpClientData)
    {
        var httpClient = httpClientData.HttpClient;

        var filter = new AIModelProviderFilterDto
        {
            PageIndex = 1,
            PageSize = 10
        };

        var response = await httpClient.PostAsJsonAsync("/api/aimodelprovider/filter", filter);
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var pagedResult = await response.Content.ReadFromJsonAsync<PageList<AIModelProviderItemDto>>();
        await Assert.That(pagedResult).IsNotNull();
        await Assert.That(pagedResult!.Data).IsNotNull();
    }

    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task CreateMultipleProviders_ShouldAllBeCreatedSuccessfully(HttpClientDataClass httpClientData)
    {
        var httpClient = httpClientData.HttpClient;
        var providerIds = new List<Guid>();

        var providerNames = new[] { "OpenAI", "Anthropic", "Google" };

        // 创建多个提供商
        for (int i = 0; i < 3; i++)
        {
            var addDto = new AIModelProviderAddDto
            {
                Name = $"Provider {providerNames[i]} {Guid.NewGuid().ToString().Substring(0, 8)}",
                Description = $"AI模型提供商: {providerNames[i]}",
                Website = $"https://{providerNames[i].ToLower()}.com",
                BaseUrl = $"https://api.{providerNames[i].ToLower()}.com"
            };

            var response = await httpClient.PostAsJsonAsync("/api/aimodelprovider", addDto);
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);

            var provider = await response.Content.ReadFromJsonAsync<AIModelProvider>();
            providerIds.Add(provider!.Id);
        }

        // 验证所有创建的提供商
        foreach (var providerId in providerIds)
        {
            var response = await httpClient.GetAsync($"/api/aimodelprovider/{providerId}");
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

            var provider = await response.Content.ReadFromJsonAsync<AIModelProviderDetailDto>();
            await Assert.That(provider).IsNotNull();
        }
    }
}
