using ApiTest.Data;
using KnowledgeBaseMod.Models.RagCollectionDtos;
using Entity.KnowledgeBaseMod;
using Perigon.AspNetCore.Models;
using System.Net.Http.Json;

namespace ApiTest.KnowledgeBaseMod;

/// <summary>
/// 知识库集合集成测试
/// </summary>
public class RagCollectionTests
{
    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task RagCollectionCRUD_ShouldWorkCorrectly(HttpClientDataClass httpClientData)
    {
        var httpClient = httpClientData.HttpClient;

        // Add - 创建知识库
        var addDto = new RagCollectionAddDto
        {
            Name = $"Test Collection {Guid.NewGuid().ToString().Substring(0, 8)}",
            Description = "这是一个测试知识库",
            IsPublic = false,
            IsEnabled = true
        };

        var addResponse = await httpClient.PostAsJsonAsync("/api/ragcollection", addDto);
        await Assert.That(addResponse.StatusCode).IsEqualTo(HttpStatusCode.Created);

        var addedCollection = await addResponse.Content.ReadFromJsonAsync<RagCollection>();
        await Assert.That(addedCollection).IsNotNull();
        await Assert.That(addedCollection!.Name).IsEqualTo(addDto.Name);
        var collectionId = addedCollection.Id;

        // Get - 获取知识库详情
        var getResponse = await httpClient.GetAsync($"/api/ragcollection/{collectionId}");
        await Assert.That(getResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var collectionDetail = await getResponse.Content.ReadFromJsonAsync<RagCollectionDetailDto>();
        await Assert.That(collectionDetail).IsNotNull();
        await Assert.That(collectionDetail!.Name).IsEqualTo(addDto.Name);

        // Update - 更新知识库
        var updateDto = new RagCollectionUpdateDto
        {
            Name = $"Updated Collection {Guid.NewGuid().ToString().Substring(0, 8)}",
            Description = "更新后的知识库描述"
        };

        var updateResponse = await httpClient.PatchAsJsonAsync($"/api/ragcollection/{collectionId}", updateDto);
        await Assert.That(updateResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var updateResult = await updateResponse.Content.ReadFromJsonAsync<bool>();
        await Assert.That(updateResult).IsTrue();

        // Verify Update
        var verifyResponse = await httpClient.GetAsync($"/api/ragcollection/{collectionId}");
        var updatedCollection = await verifyResponse.Content.ReadFromJsonAsync<RagCollectionDetailDto>();
        await Assert.That(updatedCollection!.Name).IsEqualTo(updateDto.Name);

        // Delete - 删除知识库
        var deleteResponse = await httpClient.DeleteAsync($"/api/ragcollection/{collectionId}");
        await Assert.That(deleteResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var deleteResult = await deleteResponse.Content.ReadFromJsonAsync<bool>();
        await Assert.That(deleteResult).IsTrue();

        // Verify Delete
        var verifyDeleteResponse = await httpClient.GetAsync($"/api/ragcollection/{collectionId}");
        await Assert.That(verifyDeleteResponse.StatusCode == HttpStatusCode.NotFound || verifyDeleteResponse.StatusCode == HttpStatusCode.NoContent).IsTrue();
    }

    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task ListRagCollections_ShouldReturnPagedResults(HttpClientDataClass httpClientData)
    {
        var httpClient = httpClientData.HttpClient;

        var filter = new RagCollectionFilterDto
        {
            PageIndex = 1,
            PageSize = 10
        };

        var response = await httpClient.PostAsJsonAsync("/api/ragcollection/filter", filter);
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var pagedResult = await response.Content.ReadFromJsonAsync<PageList<RagCollectionItemDto>>();
        await Assert.That(pagedResult).IsNotNull();
        await Assert.That(pagedResult!.Data).IsNotNull();
    }
}
