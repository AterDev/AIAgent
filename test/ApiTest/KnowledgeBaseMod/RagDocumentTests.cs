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

    /// <summary>
    /// 测试不支持的文件类型
    /// </summary>
    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task CreateDocumentWithUnsupportedFileType_ShouldFail(HttpClientDataClass httpClientData)
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

        // 尝试创建包含不支持的文件类型的文档 (jpg)
        var addDto = new RagDocumentAddDto
        {
            Name = $"Image Document {Guid.NewGuid().ToString().Substring(0, 8)}",
            FileName = "photo.jpg",  // jpg 不支持（需要 OCR）
            CollectionId = collectionId
        };

        var response = await httpClient.PostAsJsonAsync("/api/ragdocument", addDto);
        // 应该返回 BadRequest 或其他错误响应
        await Assert.That(response.IsSuccessStatusCode).IsFalse();
    }

    /// <summary>
    /// 测试文件大小限制 - PDF 50MB
    /// </summary>
    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task UploadPdfExceeding50MB_ShouldFail(HttpClientDataClass httpClientData)
    {
        var httpClient = httpClientData.HttpClient;

        using var content = new MultipartFormDataContent();
        
        // 创建一个 55MB 的虚拟 PDF 文件内容
        var largeBuffer = new byte[55 * 1024 * 1024];
        new Random().NextBytes(largeBuffer);
        
        using var fileStream = new MemoryStream(largeBuffer);
        content.Add(new StreamContent(fileStream), "file", "large_file.pdf");

        var response = await httpClient.PostAsync("/api/fileupload/upload", content);
        
        // 应该返回 BadRequest，因为超过了 50MB 限制
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// 测试文件大小限制 - 其他文件 20MB
    /// </summary>
    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task UploadDocxExceeding20MB_ShouldFail(HttpClientDataClass httpClientData)
    {
        var httpClient = httpClientData.HttpClient;

        using var content = new MultipartFormDataContent();
        
        // 创建一个 25MB 的虚拟 DOCX 文件内容
        var largeBuffer = new byte[25 * 1024 * 1024];
        new Random().NextBytes(largeBuffer);
        
        using var fileStream = new MemoryStream(largeBuffer);
        content.Add(new StreamContent(fileStream), "file", "large_document.docx");

        var response = await httpClient.PostAsync("/api/fileupload/upload", content);
        
        // 应该返回 BadRequest，因为超过了 20MB 限制
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// 测试支持的文件类型 - PDF
    /// </summary>
    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task UploadValidPdfFile_ShouldSucceed(HttpClientDataClass httpClientData)
    {
        var httpClient = httpClientData.HttpClient;

        using var content = new MultipartFormDataContent();
        
        // 创建一个小的有效 PDF 文件内容
        var pdfContent = "%PDF-1.4\n%简单的 PDF 文件内容";
        using var fileStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(pdfContent));
        content.Add(new StreamContent(fileStream), "file", "test_document.pdf");

        var response = await httpClient.PostAsync("/api/fileupload/upload", content);
        
        // 应该成功上传
        await Assert.That(response.IsSuccessStatusCode).IsTrue();
        
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        await Assert.That((object?)result).IsNotNull();
    }

    /// <summary>
    /// 测试不支持的上传文件类型
    /// </summary>
    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task UploadInvalidFileType_ShouldFail(HttpClientDataClass httpClientData)
    {
        var httpClient = httpClientData.HttpClient;

        using var content = new MultipartFormDataContent();
        
        // 创建一个 .exe 文件（不允许）
        using var fileStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("MZ executable"));
        content.Add(new StreamContent(fileStream), "file", "malware.exe");

        var response = await httpClient.PostAsync("/api/fileupload/upload", content);
        
        // 应该返回 BadRequest，因为不支持 .exe 文件
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }
}
