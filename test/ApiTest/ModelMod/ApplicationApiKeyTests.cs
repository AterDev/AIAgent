using ApiTest.Data;
using ModelMod.Models.ApplicationApiKeyDtos;
using ModelMod.Models.ApplicationDtos;
using System.Net.Http.Headers;
using System.Net.Http.Json;

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
    public async Task ApplicationApiKey_ShouldAuthorizeOwnRequests_AndRejectInvalidKeys(HttpClientDataClass httpClientData)
    {
        var adminClient = httpClientData.HttpClient;
        Guid applicationId = Guid.Empty;
        Guid otherApplicationId = Guid.Empty;

        try
        {
            var addPrimaryAppResponse = await adminClient.PostAsJsonAsync("/api/Application", new ApplicationAddDto
            {
                Name = $"OpenApi App {Guid.NewGuid().ToString()[..8]}",
                Description = "用于测试开放平台鉴权",
                IsEnabled = true
            });
            await AssertStatusCodeAsync(addPrimaryAppResponse, HttpStatusCode.OK);
            var primaryApp = await addPrimaryAppResponse.Content.ReadFromJsonAsync<ApplicationDetailDto>();
            await Assert.That(primaryApp).IsNotNull();
            applicationId = primaryApp!.Id;

            var addOtherAppResponse = await adminClient.PostAsJsonAsync("/api/Application", new ApplicationAddDto
            {
                Name = $"Other App {Guid.NewGuid().ToString()[..8]}",
                Description = "用于测试跨应用访问限制",
                IsEnabled = true
            });
            await AssertStatusCodeAsync(addOtherAppResponse, HttpStatusCode.OK);
            var otherApp = await addOtherAppResponse.Content.ReadFromJsonAsync<ApplicationDetailDto>();
            await Assert.That(otherApp).IsNotNull();
            otherApplicationId = otherApp!.Id;

            var addKeyResponse = await adminClient.PostAsJsonAsync($"/api/Application/{applicationId}/api-keys", new ApplicationApiKeyAddDto
            {
                Name = "OpenPlatform Key",
                ApiKeyExpiresInMonths = 3
            });
            await AssertStatusCodeAsync(addKeyResponse, HttpStatusCode.OK);

            var apiKeyResult = await addKeyResponse.Content.ReadFromJsonAsync<ApplicationApiKeyCredentialResultDto>();
            await Assert.That(apiKeyResult).IsNotNull();

            using var apiClient = await CreateApiServiceClientAsync(apiKeyResult.ApiKey);

            var ownDetailResponse = await apiClient.GetAsync($"/api/v1/apps/{applicationId}");
            await AssertStatusCodeAsync(ownDetailResponse, HttpStatusCode.OK);

            var ownDetail = await ownDetailResponse.Content.ReadFromJsonAsync<ApplicationDetailDto>();
            await Assert.That(ownDetail).IsNotNull();
            await Assert.That(ownDetail!.Id).IsEqualTo(applicationId);

            var otherDetailResponse = await apiClient.GetAsync($"/api/v1/apps/{otherApplicationId}");
            await AssertStatusCodeAsync(otherDetailResponse, HttpStatusCode.Forbidden);

            var filterResponse = await apiClient.PostAsJsonAsync("/api/v1/apps/filter", new ApplicationFilterDto
            {
                PageIndex = 1,
                PageSize = 10
            });
            await AssertStatusCodeAsync(filterResponse, HttpStatusCode.OK);

            var filterResult = await filterResponse.Content.ReadFromJsonAsync<Perigon.AspNetCore.Models.PageList<ApplicationItemDto>>();
            await Assert.That(filterResult).IsNotNull();
            await Assert.That(filterResult!.Count).IsEqualTo(1);
            await Assert.That(filterResult.Data[0].Id).IsEqualTo(applicationId);

            using var invalidFormatClient = await CreateApiServiceClientAsync("invalid-api-key");
            var invalidFormatResponse = await invalidFormatClient.GetAsync($"/api/v1/apps/{applicationId}");
            await AssertStatusCodeAsync(invalidFormatResponse, HttpStatusCode.Unauthorized);

            using var invalidValueClient = await CreateApiServiceClientAsync($"sk-{Guid.NewGuid():N}");
            var invalidValueResponse = await invalidValueClient.GetAsync($"/api/v1/apps/{applicationId}");
            await AssertStatusCodeAsync(invalidValueResponse, HttpStatusCode.Unauthorized);
        }
        finally
        {
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