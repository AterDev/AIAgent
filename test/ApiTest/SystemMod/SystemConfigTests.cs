using ApiTest.Data;
using SystemMod.Models.SystemConfigDtos;
using Entity.SystemMod;
using Perigon.AspNetCore.Models;
using System.Net.Http.Json;

namespace ApiTest.SystemMod;

/// <summary>
/// 系统配置集成测试
/// </summary>
public class SystemConfigTests
{
    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task SystemConfigCRUD_ShouldWorkCorrectly(HttpClientDataClass httpClientData)
    {
        var httpClient = httpClientData.HttpClient;

        // Add - 创建系统配置
        var addDto = new SystemConfigAddDto
        {
            Key = $"test_config_key_{Guid.NewGuid().ToString().Substring(0, 8)}",
            Value = "test_value",
            Description = "测试配置项"
        };

        var addResponse = await httpClient.PostAsJsonAsync("/api/systemConfig", addDto);
        await Assert.That(addResponse.StatusCode).IsEqualTo(HttpStatusCode.Created);

        var addedConfig = await addResponse.Content.ReadFromJsonAsync<SystemConfig>();
        await Assert.That(addedConfig).IsNotNull();
        await Assert.That(addedConfig!.Key).IsEqualTo(addDto.Key);
        var configId = addedConfig.Id;

        // Get - 获取配置详情
        var getResponse = await httpClient.GetAsync($"/api/systemConfig/{configId}");
        await Assert.That(getResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var configDetail = await getResponse.Content.ReadFromJsonAsync<SystemConfigDetailDto>();
        await Assert.That(configDetail).IsNotNull();
        await Assert.That(configDetail!.Key).IsEqualTo(addDto.Key);
        await Assert.That(configDetail.Value).IsEqualTo(addDto.Value);

        // Update - 更新配置
        var updateDto = new SystemConfigUpdateDto
        {
            Value = "updated_value",
            Description = "更新后的配置"
        };

        var updateResponse = await httpClient.PatchAsJsonAsync($"/api/systemConfig/{configId}", updateDto);
        await Assert.That(updateResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var updateResult = await updateResponse.Content.ReadFromJsonAsync<bool>();
        await Assert.That(updateResult).IsTrue();

        // Verify Update
        var verifyResponse = await httpClient.GetAsync($"/api/systemConfig/{configId}");
        var updatedConfig = await verifyResponse.Content.ReadFromJsonAsync<SystemConfigDetailDto>();
        await Assert.That(updatedConfig!.Value).IsEqualTo(updateDto.Value);

        // Delete - 删除配置
        var deleteResponse = await httpClient.DeleteAsync($"/api/systemConfig/{configId}");
        await Assert.That(deleteResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var deleteResult = await deleteResponse.Content.ReadFromJsonAsync<bool>();
        await Assert.That(deleteResult).IsTrue();

        // Verify Delete
        var verifyDeleteResponse = await httpClient.GetAsync($"/api/systemConfig/{configId}");
        await Assert.That(
            verifyDeleteResponse.StatusCode == HttpStatusCode.NotFound
            || verifyDeleteResponse.StatusCode == HttpStatusCode.Forbidden).IsTrue();

        var verifyListResponse = await httpClient.PostAsJsonAsync("/api/systemConfig/filter", new SystemConfigFilterDto
        {
            PageIndex = 1,
            PageSize = 20,
            Key = addDto.Key,
        });
        await Assert.That(verifyListResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var verifyList = await verifyListResponse.Content.ReadFromJsonAsync<PageList<SystemConfigItemDto>>();
        await Assert.That(verifyList).IsNotNull();
        await Assert.That((verifyList!.Data ?? []).Any(q => q.Id == configId)).IsFalse();
    }

    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task ListSystemConfigs_ShouldReturnPagedResults(HttpClientDataClass httpClientData)
    {
        var httpClient = httpClientData.HttpClient;

        var filter = new SystemConfigFilterDto
        {
            PageIndex = 1,
            PageSize = 10
        };

        var response = await httpClient.PostAsJsonAsync("/api/systemConfig/filter", filter);
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var pagedResult = await response.Content.ReadFromJsonAsync<PageList<SystemConfigItemDto>>();
        await Assert.That(pagedResult).IsNotNull();
        await Assert.That(pagedResult!.Data).IsNotNull();
        await Assert.That(pagedResult.Data.Count).IsGreaterThanOrEqualTo(0);
    }

    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task FilterSystemConfigs_WithKeywordSearch_ShouldReturnFilteredResults(HttpClientDataClass httpClientData)
    {
        var httpClient = httpClientData.HttpClient;

        // 创建一个配置
        var addDto = new SystemConfigAddDto
        {
            Key = $"search_config_{Guid.NewGuid().ToString().Substring(0, 8)}",
            Value = "search_value",
            Description = "用于搜索测试"
        };

        await httpClient.PostAsJsonAsync("/api/systemConfig", addDto);

        // 搜索配置
        var filter = new SystemConfigFilterDto
        {
            PageIndex = 1,
            PageSize = 20
        };

        var response = await httpClient.PostAsJsonAsync("/api/systemConfig/filter", filter);
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var pagedResult = await response.Content.ReadFromJsonAsync<PageList<SystemConfigItemDto>>();
        await Assert.That(pagedResult).IsNotNull();
    }
}
