namespace Entity.WorkflowMod;

/// <summary>
/// 工作流执行记录
/// </summary>
[Index(nameof(WorkflowId), nameof(Status))]
public class WorkflowExecution : EntityBase
{
    public Guid WorkflowId { get; set; }

    [ForeignKey(nameof(WorkflowId))]
    public Workflow? Workflow { get; set; }

    public WorkflowExecutionStatus Status { get; set; }

    public WorkflowExecutionMode ExecutionMode { get; set; } = WorkflowExecutionMode.Normal;

    [MaxLength(4000)]
    public string InputJson { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string OutputJson { get; set; } = string.Empty;

    /// <summary>
    /// 全局执行上下文（所有步骤的中间结果）
    /// </summary>
    [MaxLength(8000)]
    public string ContextJson { get; set; } = string.Empty;

    /// <summary>
    /// 步骤执行记录（序列化的 StepExecution 列表）
    /// </summary>
    public string? StepExecutionsJson { get; set; }

    /// <summary>
    /// 上一次检查点（用于断点续传）
    /// </summary>
    public int? LastCheckpointStepIndex { get; set; }

    /// <summary>
    /// 已执行步骤数量
    /// </summary>
    public int ExecutedStepCount { get; set; }

    /// <summary>
    /// 重试次数
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// 最大重试次数
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// 是否已放弃
    /// </summary>
    public bool IsAbandoned { get; set; }

    public DateTimeOffset? CompletedTime { get; set; }

    public int DurationMs { get; set; }

    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 断点续传的恢复时间
    /// </summary>
    public DateTimeOffset? ResumedAt { get; set; }
}
