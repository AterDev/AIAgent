using ApiTest.Data;
using KnowledgeBaseMod.Models.RagDocumentDtos;
using KnowledgeBaseMod.Models.RagCollectionDtos;
using Entity.KnowledgeBaseMod;
using Perigon.AspNetCore.Models;
using System.Net;
using System.Net.Http.Json;

namespace ApiTest.KnowledgeBaseMod;

/// <summary>
/// RAG文档集成测试
/// </summary>
public class RagDocumentTests
{
    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task RagDocumentCRUD_ShouldWorkCorrectly(HttpClientDataClass httpClientData)
    {
        var httpClient = httpClientData.HttpClient;

        // 首先创建一个知识库供测试使用
        var collectionAddDto = new RagCollectionAddDto
        {
            Name = $"Test Collection {Guid.NewGuid().ToString().Substring(0, 8)}",
            IsEnabled = true
        };

        var collectionResponse = await httpClient.PostAsJsonAsync("/api/ragcollection", collectionAddDto);
        var collection = await collectionResponse.Content.ReadFromJsonAsync<RagCollection>();
        var collectionId = collection!.Id;

        // Add - 创建文档
        var addDto = new RagDocumentAddDto
        {
            Name = $"Test Document {Guid.NewGuid().ToString().Substring(0, 8)}",
            FileName = "test_document.txt",
            CollectionId = collectionId
        };

        var addResponse = await httpClient.PostAsJsonAsync("/api/ragdocument", addDto);
        await Assert.That(addResponse.StatusCode).IsEqualTo(HttpStatusCode.Created);

        var addedDocument = await addResponse.Content.ReadFromJsonAsync<RagDocument>();
        await Assert.That(addedDocument).IsNotNull();
        await Assert.That(addedDocument!.Name).IsEqualTo(addDto.Name);
        var documentId = addedDocument.Id;

        // Get - 获取文档详情
        var getResponse = await httpClient.GetAsync($"/api/ragdocument/{documentId}");
        await Assert.That(getResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var documentDetail = await getResponse.Content.ReadFromJsonAsync<RagDocumentDetailDto>();
        await Assert.That(documentDetail).IsNotNull();
        await Assert.That(documentDetail!.Name).IsEqualTo(addDto.Name);

        // Update - 更新文档
        var updateDto = new RagDocumentUpdateDto
        {
            Name = $"Updated Document {Guid.NewGuid().ToString().Substring(0, 8)}"
        };

        var updateResponse = await httpClient.PatchAsJsonAsync($"/api/ragdocument/{documentId}", updateDto);
        await Assert.That(updateResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var updateResult = await updateResponse.Content.ReadFromJsonAsync<bool>();
        await Assert.That(updateResult).IsTrue();

        // Verify Update
        var verifyResponse = await httpClient.GetAsync($"/api/ragdocument/{documentId}");
        var updatedDocument = await verifyResponse.Content.ReadFromJsonAsync<RagDocumentDetailDto>();
        await Assert.That(updatedDocument!.Name).IsEqualTo(updateDto.Name);

        // Delete - 删除文档
        var deleteResponse = await httpClient.DeleteAsync($"/api/ragdocument/{documentId}");
        await Assert.That(deleteResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var deleteResult = await deleteResponse.Content.ReadFromJsonAsync<bool>();
        await Assert.That(deleteResult).IsTrue();

        // Verify Delete
        var verifyDeleteResponse = await httpClient.GetAsync($"/api/ragdocument/{documentId}");
        await Assert.That(verifyDeleteResponse.StatusCode == HttpStatusCode.NotFound || verifyDeleteResponse.StatusCode == HttpStatusCode.NoContent).IsTrue();
    }

    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task ListRagDocuments_ShouldReturnPagedResults(HttpClientDataClass httpClientData)
    {
        var httpClient = httpClientData.HttpClient;

        var filter = new RagDocumentFilterDto
        {
            PageIndex = 1,
            PageSize = 10
        };

        var response = await httpClient.PostAsJsonAsync("/api/ragdocument/filter", filter);
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var pagedResult = await response.Content.ReadFromJsonAsync<PageList<RagDocumentItemDto>>();
        await Assert.That(pagedResult).IsNotNull();
        await Assert.That(pagedResult!.Data).IsNotNull();
    }

    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task CreateDocumentWithIngest_ShouldBeQueued(HttpClientDataClass httpClientData)
    {
        var httpClient = httpClientData.HttpClient;

        // 创建知识库
        var collectionAddDto = new RagCollectionAddDto
        {
            Name = $"Collection {Guid.NewGuid().ToString().Substring(0, 8)}",
            IsEnabled = true
        };

        var collectionResponse = await httpClient.PostAsJsonAsync("/api/ragcollection", collectionAddDto);
        var collection = await collectionResponse.Content.ReadFromJsonAsync<RagCollection>();
        var collectionId = collection!.Id;

        // 创建文档
        var addDto = new RagDocumentAddDto
        {
            Name = $"Document {Guid.NewGuid().ToString().Substring(0, 8)}",
            FileName = "test.txt",
            CollectionId = collectionId
        };

        var response = await httpClient.PostAsJsonAsync("/api/ragdocument", addDto);
        var document = await response.Content.ReadFromJsonAsync<RagDocument>();
        var documentId = document!.Id;

        // 向量化文档
        var ingestDto = new RagDocumentIngestDto
        {
            ContentText = "这是一个测试文档的内容，用于向量化处理。"
        };

        var ingestResponse = await httpClient.PostAsJsonAsync($"/api/ragdocument/{documentId}/ingest", ingestDto);
        // 由于是排队操作，应该返回Accepted(202)
        await Assert.That(ingestResponse.StatusCode).IsEqualTo(HttpStatusCode.Accepted);
    }
}
