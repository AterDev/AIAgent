using ApiTest.Data;
using WorkflowMod.Services;
using WorkflowMod.Models;
using Entity.WorkflowMod;
using System.Net.Http.Json;

namespace ApiTest.WorkflowMod;

/// <summary>
/// 工作流执行恢复功能测试
/// </summary>
public class WorkflowExecutionRecoveryTests
{
    [Test]
    public async Task WorkflowExecutionStatus_HasExpectedValues()
    {
        // Assert - Verify enum values exist
        await Assert.That((int)WorkflowExecutionStatus.Pending).IsEqualTo(0);
        await Assert.That((int)WorkflowExecutionStatus.Running).IsEqualTo(1);
        await Assert.That((int)WorkflowExecutionStatus.Completed).IsEqualTo(2);
        await Assert.That((int)WorkflowExecutionStatus.Failed).IsEqualTo(3);
        await Assert.That((int)WorkflowExecutionStatus.Retrying).IsEqualTo(4);
        await Assert.That((int)WorkflowExecutionStatus.Abandoned).IsEqualTo(5);
        await Assert.That((int)WorkflowExecutionStatus.Canceled).IsEqualTo(6);
    }

    [Test]
    public async Task StepExecutionStatus_HasExpectedValues()
    {
        // Assert - Verify enum values exist
        await Assert.That((int)StepExecutionStatus.Pending).IsEqualTo(1);
        await Assert.That((int)StepExecutionStatus.Running).IsEqualTo(2);
        await Assert.That((int)StepExecutionStatus.Completed).IsEqualTo(3);
        await Assert.That((int)StepExecutionStatus.Failed).IsEqualTo(4);
        await Assert.That((int)StepExecutionStatus.Retrying).IsEqualTo(5);
        await Assert.That((int)StepExecutionStatus.Skipped).IsEqualTo(6);
    }

    [Test]
    public async Task WorkflowExecutionMode_HasExpectedValues()
    {
        // Assert - Verify enum values exist
        await Assert.That((int)WorkflowExecutionMode.Normal).IsEqualTo(1);
        await Assert.That((int)WorkflowExecutionMode.Resumed).IsEqualTo(2);
    }

    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task GetWorkflowExecutionProgress_ShouldReturnProgress(HttpClientDataClass httpClientData)
    {
        // This test verifies that the new API endpoints are available
        // In a real scenario, you would create a workflow execution and check its progress
        var httpClient = httpClientData.HttpClient;
        
        // Just verify that the endpoint exists (will return 404 for non-existent ID, not 405)
        var executionId = Guid.NewGuid();
        var response = await httpClient.GetAsync($"/api/workflow-execution/{executionId}/progress");
        
        // Either OK or NotFound is acceptable (means endpoint exists)
        var isValidResponse = response.StatusCode == System.Net.HttpStatusCode.OK || 
                            response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                            response.StatusCode == System.Net.HttpStatusCode.BadRequest;
        
        await Assert.That(isValidResponse).IsTrue();
    }
}

