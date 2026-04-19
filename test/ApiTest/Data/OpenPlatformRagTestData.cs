using System.Net.Http.Headers;
using System.Text;

namespace ApiTest.Data;

internal static class OpenPlatformRagTestData
{
    public static RagScenario CreateScenario()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        var uniqueCode = $"KB-ALPHA-{suffix}";
        var documentName = $"Atlas Runbook {suffix}";

        var content = string.Join('\n',
            $"Atlas integration knowledge base document {suffix}.",
            "This document is used by automated integration tests.",
            $"Atlas rollout approval code: {uniqueCode}.",
            $"The approval code for the Atlas rollout is {uniqueCode}.",
            "The local embedding runtime is Ollama bge-m3.",
            "The fallback support mailbox is atlas-support@example.test.",
            "Always answer with the exact approval code when the user asks for rollout approval.");

        return new RagScenario(
            CollectionName: $"OpenApi KB {suffix}",
            DocumentName: documentName,
            FileName: $"atlas-runbook-{suffix.ToLowerInvariant()}.txt",
            SearchQuery: uniqueCode,
            AgentQuestion: "请先查询知识库，再告诉我 Atlas rollout 的 approval code，直接返回精确代码。",
            UniqueCode: uniqueCode,
            Content: content);
    }

    public static MultipartFormDataContent CreateUploadContent(Guid collectionId, RagScenario scenario)
    {
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(scenario.Content));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");

        content.Add(fileContent, "File", scenario.FileName);
        content.Add(new StringContent(collectionId.ToString()), "CollectionId");
        content.Add(new StringContent(scenario.DocumentName), "Name");
        content.Add(new StringContent("true"), "AutoParse");
        content.Add(new StringContent("integration"), "Tags");
        content.Add(new StringContent("open-platform"), "Tags");

        return content;
    }
}

internal sealed record RagScenario(
    string CollectionName,
    string DocumentName,
    string FileName,
    string SearchQuery,
    string AgentQuestion,
    string UniqueCode,
    string Content);