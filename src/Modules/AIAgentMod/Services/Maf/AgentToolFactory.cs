using System.Text.Json;
using CoreMod.Models.RagQuery;
using Microsoft.Extensions.AI;

namespace AIAgentMod.Services.Maf;

/// <summary>
/// 将本仓库的业务能力（RAG、MCP、SQL、HTTP、文件系统等）封装成 MAF 可用的 <see cref="AITool"/>。
/// 供 <see cref="MafAgentRuntime"/> 在构建 <c>ChatClientAgent</c> 时组合到工具链中。
/// </summary>
public class AgentToolFactory(
    IRagQueryService ragQueryService,
    IMcpToolExecutor mcpToolExecutor
)
{
    /// <summary>
    /// 查询知识库 Tool。自动透传当前租户/应用上下文。
    /// </summary>
    public AITool CreateRagQueryTool(Guid? defaultCollectionId = null)
    {
        return AIFunctionFactory.Create(
            async (string query, int topK, CancellationToken ct) =>
            {
                using var activity = AgentTelemetry.StartToolInvoke("query_knowledge_base");
                var result = await ragQueryService.QueryAsync(new RagQueryRequest
                {
                    Query = query,
                    TopK = topK <= 0 ? 5 : topK,
                    CollectionId = defaultCollectionId,
                }, ct);
                activity?.SetTag("rag.hits", result.Items.Count);
                return new
                {
                    items = result.Items.Select(i => new
                    {
                        documentId = i.DocumentId,
                        content = i.Content,
                        score = i.Score,
                    }),
                };
            },
            name: "query_knowledge_base",
            description: "根据用户问题检索知识库中相关的文档片段。参数：query 用户查询，topK 返回数量（默认 5）。");
    }

    /// <summary>
    /// 构建一个运行任意已注册 MCP Tool 的代理 AITool。调用方式：<c>mcp_call(name, arguments)</c>。
    /// 适用场景：Agent 对 MCP Tool 列表未知，把 MCP 作为"统一路由"暴露给模型。
    /// 正式场景推荐为每个具体 MCP Tool 生成专属 AITool（参见 <see cref="CreateMcpTool"/>）。
    /// </summary>
    public AITool CreateGenericMcpTool()
    {
        return AIFunctionFactory.Create(
            async (string name, string argumentsJson, CancellationToken ct) =>
            {
                using var activity = AgentTelemetry.StartToolInvoke(name);
                var result = await mcpToolExecutor.ExecuteAsync(
                    new ToolExecutionRequest
                    {
                        ToolName = name,
                        ArgumentsJson = argumentsJson,
                    }, ct);
                return new
                {
                    success = result.Success,
                    error = result.ErrorMessage,
                    output = TryParseJson(result.OutputJson),
                };
            },
            name: "mcp_call",
            description: "调用已注册的 MCP Tool。参数：name 工具名，argumentsJson 工具参数 JSON 字符串。");
    }

    /// <summary>
    /// 为具体 MCP Tool 创建强类型代理 AITool，暴露给模型时附带该工具的描述和 JSON schema。
    /// </summary>
    public AITool CreateMcpTool(string toolName, string description, JsonElement jsonSchema)
    {
        return AIFunctionFactory.CreateDeclaration(
            name: toolName,
            description: description,
            jsonSchema: jsonSchema,
            returnJsonSchema: null);
    }

    private static object? TryParseJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.Clone();
        }
        catch (JsonException)
        {
            return json;
        }
    }
}
