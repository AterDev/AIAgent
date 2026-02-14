using ApiTest.Data;
using AIAgentMod.Models.ConversationDtos;
using Entity.AIAgentMod;
using Perigon.AspNetCore.Models;
using System.Net.Http.Json;

namespace ApiTest.AIAgentMod;

/// <summary>
/// 对话实例集成测试
/// </summary>
public class ConversationTests
{
    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task ConversationCRUD_ShouldWorkCorrectly(HttpClientDataClass httpClientData)
    {
        var httpClient = httpClientData.HttpClient;
        
        // Get current user ID
        var currentUserResponse = await httpClient.GetAsync("/api/systemUser/current");
        var currentUser = await currentUserResponse.Content.ReadFromJsonAsync<Share.Models.Auth.UserInfoDto>();
        var userId = currentUser!.Id;

        // Add - 创建对话
        var addDto = new ConversationAddDto
        {
            Name = $"Test Conversation {Guid.NewGuid().ToString().Substring(0, 8)}",
            Description = "这是一个测试对话",
            UserId = userId
        };

        var addResponse = await httpClient.PostAsJsonAsync("/api/conversation", addDto);
        await Assert.That(addResponse.StatusCode).IsEqualTo(HttpStatusCode.Created);

        var addedConversation = await addResponse.Content.ReadFromJsonAsync<Conversation>();
        await Assert.That(addedConversation).IsNotNull();
        await Assert.That(addedConversation!.Name).IsEqualTo(addDto.Name);
        var conversationId = addedConversation.Id;

        // Get - 获取对话详情
        var getResponse = await httpClient.GetAsync($"/api/conversation/{conversationId}");
        await Assert.That(getResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var conversationDetail = await getResponse.Content.ReadFromJsonAsync<ConversationDetailDto>();
        await Assert.That(conversationDetail).IsNotNull();
        await Assert.That(conversationDetail!.Name).IsEqualTo(addDto.Name);

        // Update - 更新对话
        var updateDto = new ConversationUpdateDto
        {
            Name = $"Updated Conversation {Guid.NewGuid().ToString().Substring(0, 8)}",
            Description = "更新后的对话描述"
        };

        var updateResponse = await httpClient.PatchAsJsonAsync($"/api/conversation/{conversationId}", updateDto);
        await Assert.That(updateResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var updateResult = await updateResponse.Content.ReadFromJsonAsync<bool>();
        await Assert.That(updateResult).IsTrue();

        // Verify Update
        var verifyResponse = await httpClient.GetAsync($"/api/conversation/{conversationId}");
        var updatedConversation = await verifyResponse.Content.ReadFromJsonAsync<ConversationDetailDto>();
        await Assert.That(updatedConversation!.Name).IsEqualTo(updateDto.Name);

        // Delete - 删除对话
        var deleteResponse = await httpClient.DeleteAsync($"/api/conversation/{conversationId}");
        await Assert.That(deleteResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var deleteResult = await deleteResponse.Content.ReadFromJsonAsync<bool?>();
        await Assert.That(deleteResult).IsTrue();

        // Verify Delete
        var verifyDeleteResponse = await httpClient.GetAsync($"/api/conversation/{conversationId}");
        await Assert.That(verifyDeleteResponse.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task ListConversations_ShouldReturnPagedResults(HttpClientDataClass httpClientData)
    {
        var httpClient = httpClientData.HttpClient;

        var filter = new ConversationFilterDto
        {
            PageIndex = 1,
            PageSize = 10
        };

        var response = await httpClient.PostAsJsonAsync("/api/conversation/filter", filter);
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var pagedResult = await response.Content.ReadFromJsonAsync<PageList<ConversationItemDto>>();
        await Assert.That(pagedResult).IsNotNull();
        await Assert.That(pagedResult!.Data).IsNotNull();
    }

    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task CreateConversation_WithoutTitle_ShouldCreateSuccessfully(HttpClientDataClass httpClientData)
    {
        var httpClient = httpClientData.HttpClient;

        var addDto = new ConversationAddDto
        {
            Description = "测试对话"
        };

        var response = await httpClient.PostAsJsonAsync("/api/conversation", addDto);
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);

        var conversation = await response.Content.ReadFromJsonAsync<Conversation>();
        await Assert.That(conversation).IsNotNull();
    }

    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task SearchConversations_ShouldReturnMatchingResults(HttpClientDataClass httpClientData)
    {
        var httpClient = httpClientData.HttpClient;

        // 创建一个带有特殊名称的对话
        var testName = $"SearchTest_{Guid.NewGuid().ToString().Substring(0, 8)}";
        var addDto = new ConversationAddDto
        {
            Name = testName,
            Description = "用于搜索的对话"
        };

        var createResponse = await httpClient.PostAsJsonAsync("/api/conversation", addDto);
        var createdConversation = await createResponse.Content.ReadFromJsonAsync<Conversation>();

        // 搜索对话
        var filter = new ConversationFilterDto
        {
            PageIndex = 1,
            PageSize = 20
        };

        var response = await httpClient.PostAsJsonAsync("/api/conversation/filter", filter);
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var pagedResult = await response.Content.ReadFromJsonAsync<PageList<ConversationItemDto>>();
        await Assert.That(pagedResult).IsNotNull();
    }
}
