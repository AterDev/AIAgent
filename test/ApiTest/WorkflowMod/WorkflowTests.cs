using ApiTest.Data;
using WorkflowMod.Models.WorkflowDtos;
using Entity.WorkflowMod;
using Perigon.AspNetCore.Models;
using System.Net.Http.Json;

namespace ApiTest.WorkflowMod;

/// <summary>
/// 工作流集成测试
/// </summary>
public class WorkflowTests
{
    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task WorkflowCRUD_ShouldWorkCorrectly(HttpClientDataClass httpClientData)
    {
        var httpClient = httpClientData.HttpClient;

        // Add - 创建工作流
        var addDto = new WorkflowAddDto
        {
            Name = $"Test Workflow {Guid.NewGuid().ToString().Substring(0, 8)}",
            Description = "这是一个测试工作流"
        };

        var addResponse = await httpClient.PostAsJsonAsync("/api/workflow", addDto);
        await Assert.That(addResponse.StatusCode).IsEqualTo(HttpStatusCode.Created);

        var addedWorkflow = await addResponse.Content.ReadFromJsonAsync<Workflow>();
        await Assert.That(addedWorkflow).IsNotNull();
        await Assert.That(addedWorkflow!.Name).IsEqualTo(addDto.Name);
        var workflowId = addedWorkflow.Id;

        // Get - 获取工作流详情
        var getResponse = await httpClient.GetAsync($"/api/workflow/{workflowId}");
        await Assert.That(getResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var workflowDetail = await getResponse.Content.ReadFromJsonAsync<WorkflowDetailDto>();
        await Assert.That(workflowDetail).IsNotNull();
        await Assert.That(workflowDetail!.Name).IsEqualTo(addDto.Name);

        // Update - 更新工作流
        var updateDto = new WorkflowUpdateDto
        {
            Name = $"Updated Workflow {Guid.NewGuid().ToString().Substring(0, 8)}",
            Description = "更新后的工作流描述"
        };

        var updateResponse = await httpClient.PatchAsJsonAsync($"/api/workflow/{workflowId}", updateDto);
        await Assert.That(updateResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var updateResult = await updateResponse.Content.ReadFromJsonAsync<bool>();
        await Assert.That(updateResult).IsTrue();

        // Verify Update
        var verifyResponse = await httpClient.GetAsync($"/api/workflow/{workflowId}");
        var updatedWorkflow = await verifyResponse.Content.ReadFromJsonAsync<WorkflowDetailDto>();
        await Assert.That(updatedWorkflow!.Name).IsEqualTo(updateDto.Name);

        // Delete - 删除工作流
        var deleteResponse = await httpClient.DeleteAsync($"/api/workflow/{workflowId}");
        await Assert.That(deleteResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var deleteResult = await deleteResponse.Content.ReadFromJsonAsync<bool>();
        await Assert.That(deleteResult).IsTrue();

        // Verify Delete (soft delete returns NoContent)
        var verifyDeleteResponse = await httpClient.GetAsync($"/api/workflow/{workflowId}");
        await Assert.That(verifyDeleteResponse.StatusCode == HttpStatusCode.NotFound || verifyDeleteResponse.StatusCode == HttpStatusCode.NoContent).IsTrue();
    }

    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task ListWorkflows_ShouldReturnPagedResults(HttpClientDataClass httpClientData)
    {
        var httpClient = httpClientData.HttpClient;

        var filter = new WorkflowFilterDto
        {
            PageIndex = 1,
            PageSize = 10
        };

        var response = await httpClient.PostAsJsonAsync("/api/workflow/filter", filter);
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var pagedResult = await response.Content.ReadFromJsonAsync<PageList<WorkflowItemDto>>();
        await Assert.That(pagedResult).IsNotNull();
        await Assert.That(pagedResult!.Data).IsNotNull();
    }
}
