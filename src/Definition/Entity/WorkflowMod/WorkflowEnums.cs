namespace Entity.WorkflowMod;

using System.ComponentModel;

public enum WorkflowExecutionStatus
{
    // Original values preserved for backward compatibility
    [Description("Running")]
    Running = 0,

    [Description("Completed")]
    Completed = 1,

    [Description("Failed")]
    Failed = 2,

    [Description("Canceled")]
    Canceled = 3,

    // New statuses appended
    [Description("Pending")]
    Pending = 4,

    [Description("Retrying")]
    Retrying = 5,

    [Description("Abandoned")]
    Abandoned = 6,
}

public enum WorkflowExecutionMode
{
    [Description("Normal")]
    Normal = 0,

    [Description("Resumed")]
    Resumed = 1
}

public enum StepExecutionStatus
{
    [Description("Pending")]
    Pending = 1,

    [Description("Running")]
    Running = 2,

    [Description("Completed")]
    Completed = 3,

    [Description("Failed")]
    Failed = 4,

    [Description("Retrying")]
    Retrying = 5,

    [Description("Skipped")]
    Skipped = 6
}
