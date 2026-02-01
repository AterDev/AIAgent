using ApiTest.Data;
using McpMod.Models.McpToolDtos;
using Entity.McpMod;
using Perigon.AspNetCore.Models;
using System.Net.Http.Json;

namespace ApiTest.McpMod;

/// <summary>
/// MCP工具集成测试
/// </summary>
public class McpToolTests
{
    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task McpToolCRUD_ShouldWorkCorrectly(HttpClientDataClass httpClientData)
    {
        var httpClient = httpClientData.HttpClient;

        // Add - 创建MCP工具
        var addDto = new McpToolAddDto
        {
            Name = $"Test Tool {Guid.NewGuid().ToString().Substring(0, 8)}",
            Description = "这是一个测试MCP工具"
        };

        var addResponse = await httpClient.PostAsJsonAsync("/api/mcptool", addDto);
        await Assert.That(addResponse.StatusCode).IsEqualTo(HttpStatusCode.Created);

        var addedTool = await addResponse.Content.ReadFromJsonAsync<McpTool>();
        await Assert.That(addedTool).IsNotNull();
        await Assert.That(addedTool!.Name).IsEqualTo(addDto.Name);
        var toolId = addedTool.Id;

        // Get - 获取工具详情
        var getResponse = await httpClient.GetAsync($"/api/mcptool/{toolId}");
        await Assert.That(getResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var toolDetail = await getResponse.Content.ReadFromJsonAsync<McpToolDetailDto>();
        await Assert.That(toolDetail).IsNotNull();
        await Assert.That(toolDetail!.Name).IsEqualTo(addDto.Name);

        // Update - 更新工具
        var updateDto = new McpToolUpdateDto
        {
            Name = $"Updated Tool {Guid.NewGuid().ToString().Substring(0, 8)}",
            Description = "更新后的工具描述"
        };

        var updateResponse = await httpClient.PatchAsJsonAsync($"/api/mcptool/{toolId}", updateDto);
        await Assert.That(updateResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var updateResult = await updateResponse.Content.ReadFromJsonAsync<bool>();
        await Assert.That(updateResult).IsTrue();

        // Verify Update
        var verifyResponse = await httpClient.GetAsync($"/api/mcptool/{toolId}");
        var updatedTool = await verifyResponse.Content.ReadFromJsonAsync<McpToolDetailDto>();
        await Assert.That(updatedTool!.Name).IsEqualTo(updateDto.Name);

        // Delete - 删除工具
        var deleteResponse = await httpClient.DeleteAsync($"/api/mcptool/{toolId}");
        await Assert.That(deleteResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var deleteResult = await deleteResponse.Content.ReadFromJsonAsync<bool>();
        await Assert.That(deleteResult).IsTrue();

        // Verify Delete
        var verifyDeleteResponse = await httpClient.GetAsync($"/api/mcptool/{toolId}");
        await Assert.That(verifyDeleteResponse.StatusCode == HttpStatusCode.NotFound || verifyDeleteResponse.StatusCode == HttpStatusCode.NoContent).IsTrue();
    }

    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task ListMcpTools_ShouldReturnPagedResults(HttpClientDataClass httpClientData)
    {
        var httpClient = httpClientData.HttpClient;

        var filter = new McpToolFilterDto
        {
            PageIndex = 1,
            PageSize = 10
        };

        var response = await httpClient.PostAsJsonAsync("/api/mcptool/filter", filter);
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var pagedResult = await response.Content.ReadFromJsonAsync<PageList<McpToolItemDto>>();
        await Assert.That(pagedResult).IsNotNull();
        await Assert.That(pagedResult!.Data).IsNotNull();
    }

    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task GetDefinitions_ShouldReturnToolList(HttpClientDataClass httpClientData)
    {
        var httpClient = httpClientData.HttpClient;

        var response = await httpClient.GetAsync("/api/mcptool/definitions");
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var definitions = await response.Content.ReadFromJsonAsync<List<global::McpMod.Models.ToolDefinitionDto>>();
        await Assert.That(definitions).IsNotNull();
    }
}
