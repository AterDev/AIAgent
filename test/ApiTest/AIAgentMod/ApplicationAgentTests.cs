using ApiTest.Data;
using AIAgentMod.Models.AIAgentDtos;
using Entity.AIAgentMod;
using ModelMod.Models.ApplicationDtos;
using Perigon.AspNetCore.Models;
using System.Net.Http.Json;

namespace ApiTest.AIAgentMod;

public class ApplicationAgentTests
{
    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task ApplicationAgentCRUD_ShouldWorkCorrectly(HttpClientDataClass httpClientData)
    {
        var httpClient = httpClientData.HttpClient;
        Guid applicationId = Guid.Empty;
        Guid agentId = Guid.Empty;

        try
        {
            var addAppResponse = await httpClient.PostAsJsonAsync("/api/Application", new ApplicationAddDto
            {
                Name = $"AppAgent-{Guid.NewGuid():N}"[..17],
                Description = "用于验证应用侧 Agent CRUD",
                IsEnabled = true,
            });
            await Assert.That(addAppResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

            var application = await addAppResponse.Content.ReadFromJsonAsync<ApplicationDetailDto>();
            await Assert.That(application).IsNotNull();
            applicationId = application!.Id;

            var addResponse = await httpClient.PostAsJsonAsync("/api/ApplicationAgent", new AIAgentAddDto
            {
                Name = $"ApplicationAgent-{Guid.NewGuid():N}"[..25],
                Description = "应用侧 Agent",
                ModelId = "deepseek-chat",
                SystemPrompt = "你是应用侧集成测试 Agent",
                Enable = true,
                ApplicationId = applicationId,
            });
            await Assert.That(addResponse.StatusCode).IsEqualTo(HttpStatusCode.Created);

            var addedAgent = await addResponse.Content.ReadFromJsonAsync<ApplicationAgent>();
            await Assert.That(addedAgent).IsNotNull();
            agentId = addedAgent!.Id;

            var listResponse = await httpClient.PostAsJsonAsync("/api/ApplicationAgent/filter", new AIAgentFilterDto
            {
                PageIndex = 1,
                PageSize = 20,
                ApplicationId = applicationId,
                Name = addedAgent.Name,
            });
            await Assert.That(listResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

            var list = await listResponse.Content.ReadFromJsonAsync<PageList<AIAgentItemDto>>();
            await Assert.That(list).IsNotNull();
            await Assert.That((list!.Data ?? []).Any(q => q.Id == agentId)).IsTrue();

            var detailResponse = await httpClient.GetAsync($"/api/ApplicationAgent/{agentId}");
            await Assert.That(detailResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

            var detail = await detailResponse.Content.ReadFromJsonAsync<AIAgentDetailDto>();
            await Assert.That(detail).IsNotNull();
            await Assert.That(detail!.ApplicationId).IsEqualTo(applicationId);

            var updateResponse = await httpClient.PatchAsJsonAsync($"/api/ApplicationAgent/{agentId}", new AIAgentUpdateDto
            {
                Description = "更新后的应用侧 Agent",
                Enable = false,
            });
            await Assert.That(updateResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

            var deleteResponse = await httpClient.DeleteAsync($"/api/ApplicationAgent/{agentId}");
            await Assert.That(deleteResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
            agentId = Guid.Empty;
        }
        finally
        {
            if (agentId != Guid.Empty)
            {
                await httpClient.DeleteAsync($"/api/ApplicationAgent/{agentId}");
            }

            if (applicationId != Guid.Empty)
            {
                await httpClient.DeleteAsync($"/api/Application/{applicationId}");
            }
        }
    }
}