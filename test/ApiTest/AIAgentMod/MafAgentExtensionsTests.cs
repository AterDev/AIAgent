using ApiTest.Data;
using AIAgentMod.Models.AIAgentDtos;
using Entity.AIAgentMod;
using Perigon.AspNetCore.Models;
using System.Net.Http.Json;

namespace ApiTest.AIAgentMod;

/// <summary>
/// MAF 1.1 补强字段的集成测试：验证 AIAgent 新增字段（Temperature/TopP/MaxOutputTokens/
/// Capabilities/MemoryMode/ContextWindow/HandoffTargets/Skills/Tags/ResponseSchemaJson）
/// 在 DTO → Entity → 查询回读链路上的一致性。
/// </summary>
public class MafAgentExtensionsTests
{
    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task AIAgent_WithMafExtensionFields_ShouldRoundTrip(HttpClientDataClass httpClientData)
    {
        var httpClient = httpClientData.HttpClient;

        const string schemaJson = """{"type":"object","properties":{"answer":{"type":"string"}},"required":["answer"]}""";

        var addDto = new AIAgentAddDto
        {
            Name = $"Maf Agent {Guid.NewGuid():N}"[..20],
            Description = "验证 MAF 扩展字段",
            ModelId = "deepseek-chat",
            SystemPrompt = "You are a MAF test agent.",
            Enable = true,
            IsPublic = true,
            Temperature = 0.35f,
            TopP = 0.9f,
            MaxOutputTokens = 512,
            ContextWindow = 30,
            MemoryMode = AgentMemoryMode.Summary,
            Capabilities = AgentCapabilities.Tools | AgentCapabilities.StructuredOutput | AgentCapabilities.Handoff,
            HandoffTargets = ["DemoReviewerAgent"],
            Skills = ["translate", "summarize"],
            Tags = ["integration", "maf"],
            ResponseSchemaJson = schemaJson,
        };

        var addResponse = await httpClient.PostAsJsonAsync("/api/aiagent", addDto);
        await Assert.That(addResponse.StatusCode).IsEqualTo(HttpStatusCode.Created);

        var added = await addResponse.Content.ReadFromJsonAsync<AIAgent>();
        await Assert.That(added).IsNotNull();
        var id = added!.Id;

        try
        {
            var detailResponse = await httpClient.GetAsync($"/api/aiagent/{id}");
            await Assert.That(detailResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

            var detail = await detailResponse.Content.ReadFromJsonAsync<AIAgentDetailDto>();
            await Assert.That(detail).IsNotNull();
            await Assert.That(detail!.Temperature).IsEqualTo(0.35f);
            await Assert.That(detail.TopP).IsEqualTo(0.9f);
            await Assert.That(detail.MaxOutputTokens).IsEqualTo(512);
            await Assert.That(detail.ContextWindow).IsEqualTo(30);
            await Assert.That(detail.MemoryMode).IsEqualTo(AgentMemoryMode.Summary);
            await Assert.That(detail.Capabilities!.Value.HasFlag(AgentCapabilities.StructuredOutput)).IsTrue();
            await Assert.That(detail.Capabilities!.Value.HasFlag(AgentCapabilities.Handoff)).IsTrue();
            await Assert.That(detail.HandoffTargets).IsNotNull();
            await Assert.That(detail.HandoffTargets!.Contains("DemoReviewerAgent")).IsTrue();
            await Assert.That(detail.Skills!.Count).IsEqualTo(2);
            await Assert.That(detail.Tags!.Contains("maf")).IsTrue();
            await Assert.That(detail.ResponseSchemaJson).IsEqualTo(schemaJson);
        }
        finally
        {
            await httpClient.DeleteAsync($"/api/aiagent/{id}");
        }
    }

    /// <summary>
    /// 验证 MigrationService 种子已经创建出 translator → rewriter → reviewer 三段式 Agent，
    /// 以及 handoff 关联链路配置完整。
    /// </summary>
    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task SeededTranslationAgents_ShouldFormHandoffChain(HttpClientDataClass httpClientData)
    {
        var httpClient = httpClientData.HttpClient;

        async Task<AIAgentItemDto?> FindAsync(string name)
        {
            var resp = await httpClient.PostAsJsonAsync("/api/aiagent/filter", new AIAgentFilterDto
            {
                PageIndex = 1,
                PageSize = 10,
                Name = name,
            });
            await Assert.That(resp.StatusCode).IsEqualTo(HttpStatusCode.OK);

            var page = await resp.Content.ReadFromJsonAsync<PageList<AIAgentItemDto>>();
            return page?.Data?.FirstOrDefault(q => q.Name == name);
        }

        var translator = await FindAsync("DemoTranslatorAgent");
        var rewriter = await FindAsync("DemoRewriterAgent");
        var reviewer = await FindAsync("DemoReviewerAgent");

        await Assert.That(translator).IsNotNull();
        await Assert.That(rewriter).IsNotNull();
        await Assert.That(reviewer).IsNotNull();

        // 详情层验证 handoff 目标串联正确
        var translatorDetailResp = await httpClient.GetAsync($"/api/aiagent/{translator!.Id}");
        var rewriterDetailResp = await httpClient.GetAsync($"/api/aiagent/{rewriter!.Id}");

        var translatorDetail = await translatorDetailResp.Content.ReadFromJsonAsync<AIAgentDetailDto>();
        var rewriterDetail = await rewriterDetailResp.Content.ReadFromJsonAsync<AIAgentDetailDto>();

        await Assert.That(translatorDetail!.HandoffTargets).IsNotNull();
        await Assert.That(translatorDetail.HandoffTargets!.Contains("DemoRewriterAgent")).IsTrue();
        await Assert.That(rewriterDetail!.HandoffTargets!.Contains("DemoReviewerAgent")).IsTrue();
    }
}
