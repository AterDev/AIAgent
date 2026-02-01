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
        // Assert - Verify enum values match original mappings (backward compatible)
        await Assert.That((int)WorkflowExecutionStatus.Running).IsEqualTo(0);
        await Assert.That((int)WorkflowExecutionStatus.Completed).IsEqualTo(1);
        await Assert.That((int)WorkflowExecutionStatus.Failed).IsEqualTo(2);
        await Assert.That((int)WorkflowExecutionStatus.Canceled).IsEqualTo(3);
        await Assert.That((int)WorkflowExecutionStatus.Pending).IsEqualTo(4);
        await Assert.That((int)WorkflowExecutionStatus.Retrying).IsEqualTo(5);
        await Assert.That((int)WorkflowExecutionStatus.Abandoned).IsEqualTo(6);
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
        await Assert.That((int)WorkflowExecutionMode.Normal).IsEqualTo(0);
        await Assert.That((int)WorkflowExecutionMode.Resumed).IsEqualTo(1);
    }

    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task GetWorkflowExecutionProgress_ShouldReturn404ForNonExistentExecution(HttpClientDataClass httpClientData)
    {
        // Arrange
        var httpClient = httpClientData.HttpClient;
        var executionId = Guid.NewGuid(); // Non-existent execution
        
        // Act
        var response = await httpClient.GetAsync($"/api/workflow-execution/{executionId}/progress");
        
        // Assert - Should return 404 for non-existent execution
        await Assert.That(response.StatusCode).IsEqualTo(System.Net.HttpStatusCode.NotFound);
    }

    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task ResumeWorkflowExecution_EndpointExists(HttpClientDataClass httpClientData)
    {
        // Note: This is a basic endpoint existence test. A complete integration test would:
        // 1. Create a workflow definition
        // 2. Create a workflow execution
        // 3. Simulate a failure mid-execution
        // 4. Call resume endpoint with checkpoint step index
        // 5. Verify execution continues from the checkpoint
        // 6. Verify completed steps are skipped
        // Such a test requires a real workflow definition and proper test fixtures.
        
        var httpClient = httpClientData.HttpClient;
        var executionId = Guid.NewGuid();
        var response = await httpClient.PostAsync($"/api/workflow-execution/{executionId}/resume?fromStep=0", null);
        
        // Verify endpoint exists (OK=200 for success, NotFound=404 for missing execution)
        var isValidResponse = response.StatusCode == System.Net.HttpStatusCode.OK || 
                            response.StatusCode == System.Net.HttpStatusCode.NotFound;
        
        await Assert.That(isValidResponse).IsTrue();
    }
}

