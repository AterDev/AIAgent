namespace Entity.WorkflowMod;

using System.ComponentModel;

public enum WorkflowExecutionStatus
{
    [Description("Pending")]
    Pending = 0,

    [Description("Running")]
    Running = 1,

    [Description("Completed")]
    Completed = 2,

    [Description("Failed")]
    Failed = 3,

    [Description("Retrying")]
    Retrying = 4,

    [Description("Abandoned")]
    Abandoned = 5,

    [Description("Canceled")]
    Canceled = 6,
}

public enum WorkflowExecutionMode
{
    [Description("Normal")]
    Normal = 1,

    [Description("Resumed")]
    Resumed = 2
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
