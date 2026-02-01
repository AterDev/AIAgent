namespace WorkflowMod.Services;

public interface IWorkflowExecutor
{
    /// <summary>
    /// 执行工作流
    /// </summary>
    Task<bool> ExecuteAsync(Guid executionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 断点续传执行
    /// </summary>
    Task<bool> ResumeAsync(Guid executionId, int fromStepIndex, CancellationToken cancellationToken = default);

    /// <summary>
    /// 取消执行
    /// </summary>
    Task<bool> CancelAsync(Guid executionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取执行进度
    /// </summary>
    Task<WorkflowExecutionProgress?> GetProgressAsync(Guid executionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 重试失败的执行
    /// </summary>
    Task<bool> RetryAsync(Guid executionId, CancellationToken cancellationToken = default);
}
