namespace WorkflowMod.Models;

/// <summary>
/// 步骤执行记录
/// </summary>
public class StepExecution
{
    /// <summary>
    /// 步骤索引
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// 步骤名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 执行状态
    /// </summary>
    public StepExecutionStatus Status { get; set; }

    /// <summary>
    /// 输入参数
    /// </summary>
    public Dictionary<string, object?> Input { get; set; } = [];

    /// <summary>
    /// 输出结果
    /// </summary>
    public Dictionary<string, object?> Output { get; set; } = [];

    /// <summary>
    /// 错误消息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 重试次数
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTimeOffset StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTimeOffset? EndTime { get; set; }

    /// <summary>
    /// 执行时长（毫秒）
    /// </summary>
    public int DurationMs { get; set; }
}
