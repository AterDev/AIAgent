namespace WorkflowMod.Models;

/// <summary>
/// 工作流执行进度信息
/// </summary>
public class WorkflowExecutionProgress
{
    public Guid ExecutionId { get; set; }
    public WorkflowExecutionStatus Status { get; set; }
    public int TotalSteps { get; set; }
    public int CompletedSteps { get; set; }
    public int FailedSteps { get; set; }
    public double ProgressPercentage { get; set; }
    public List<StepExecutionInfo> Steps { get; set; } = [];
    public string? CurrentStepName { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// 步骤执行信息
/// </summary>
public class StepExecutionInfo
{
    public int Index { get; set; }
    public string Name { get; set; } = string.Empty;
    public StepExecutionStatus Status { get; set; }
    public int DurationMs { get; set; }
    public string? ErrorMessage { get; set; }
}
