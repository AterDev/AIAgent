using ApiTest.Data;
using AIAgentMod.Models.AIAgentDtos;
using Entity.AIAgentMod;
using Perigon.AspNetCore.Models;
using System.Net.Http.Json;

namespace ApiTest.AIAgentMod;

/// <summary>
/// AI Agent集成测试
/// </summary>
public class AIAgentTests
{
    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task AIAgentCRUD_ShouldWorkCorrectly(HttpClientDataClass httpClientData)
    {
        var httpClient = httpClientData.HttpClient;

        // Add - 创建AI Agent
        var addDto = new AIAgentAddDto
        {
            Name = $"Test Agent {Guid.NewGuid().ToString().Substring(0, 8)}",
            Description = "这是一个测试Agent",
            ModelId = "gpt-4",
            SystemPrompt = "你是一个有帮助的AI助手"
        };

        var addResponse = await httpClient.PostAsJsonAsync("/api/aiagent", addDto);
        await Assert.That(addResponse.StatusCode).IsEqualTo(HttpStatusCode.Created);

        var addedAgent = await addResponse.Content.ReadFromJsonAsync<AIAgent>();
        await Assert.That(addedAgent).IsNotNull();
        await Assert.That(addedAgent!.Name).IsEqualTo(addDto.Name);
        var agentId = addedAgent.Id;

        // Get - 获取Agent详情
        var getResponse = await httpClient.GetAsync($"/api/aiagent/{agentId}");
        await Assert.That(getResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var agentDetail = await getResponse.Content.ReadFromJsonAsync<AIAgentDetailDto>();
        await Assert.That(agentDetail).IsNotNull();
        await Assert.That(agentDetail!.Name).IsEqualTo(addDto.Name);
        await Assert.That(agentDetail.Description).IsEqualTo(addDto.Description);

        // Update - 更新Agent
        var updateDto = new AIAgentUpdateDto
        {
            Name = $"Updated Agent {Guid.NewGuid().ToString().Substring(0, 8)}",
            Description = "更新后的描述"
        };

        var updateResponse = await httpClient.PatchAsJsonAsync($"/api/aiagent/{agentId}", updateDto);
        await Assert.That(updateResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var updateResult = await updateResponse.Content.ReadFromJsonAsync<bool>();
        await Assert.That(updateResult).IsTrue();

        // Verify Update
        var verifyResponse = await httpClient.GetAsync($"/api/aiagent/{agentId}");
        var updatedAgent = await verifyResponse.Content.ReadFromJsonAsync<AIAgentDetailDto>();
        await Assert.That(updatedAgent!.Name).IsEqualTo(updateDto.Name);

        // Delete - 删除Agent
        var deleteResponse = await httpClient.DeleteAsync($"/api/aiagent/{agentId}");
        await Assert.That(deleteResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var deleteResult = await deleteResponse.Content.ReadFromJsonAsync<bool>();
        await Assert.That(deleteResult).IsTrue();

        // Verify Delete
        var verifyDeleteResponse = await httpClient.GetAsync($"/api/aiagent/{agentId}");
        await Assert.That(verifyDeleteResponse.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task ListAIAgents_ShouldReturnPagedResults(HttpClientDataClass httpClientData)
    {
        var httpClient = httpClientData.HttpClient;

        var filter = new AIAgentFilterDto
        {
            PageIndex = 1,
            PageSize = 10
        };

        var response = await httpClient.PostAsJsonAsync("/api/aiagent/filter", filter);
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var pagedResult = await response.Content.ReadFromJsonAsync<PageList<AIAgentItemDto>>();
        await Assert.That(pagedResult).IsNotNull();
        await Assert.That(pagedResult!.Data).IsNotNull();
    }

    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task CreateMultipleAgents_ShouldAllBeCreatedSuccessfully(HttpClientDataClass httpClientData)
    {
        var httpClient = httpClientData.HttpClient;
        var agentIds = new List<Guid>();

        var modelIds = new[] { "gpt-4", "gpt-3.5-turbo", "claude-3" };

        // 创建多个Agent
        for (int i = 0; i < 3; i++)
        {
            var addDto = new AIAgentAddDto
            {
                Name = $"Batch Agent {i + 1} {Guid.NewGuid().ToString().Substring(0, 8)}",
                Description = $"批量创建的第{i + 1}个Agent",
                ModelId = modelIds[i],
                SystemPrompt = "这是一个批量测试"
            };

            var response = await httpClient.PostAsJsonAsync("/api/aiagent", addDto);
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);

            var agent = await response.Content.ReadFromJsonAsync<AIAgent>();
            agentIds.Add(agent!.Id);
        }

        // 验证所有创建的Agent
        foreach (var agentId in agentIds)
        {
            var response = await httpClient.GetAsync($"/api/aiagent/{agentId}");
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

            var agent = await response.Content.ReadFromJsonAsync<AIAgentDetailDto>();
            await Assert.That(agent).IsNotNull();
        }
    }
}
