namespace Entity.WorkflowMod;

using System.ComponentModel;

public enum WorkflowExecutionStatus
{
    [Description("Running")]
    Running = 0,

    [Description("Completed")]
    Completed = 1,

    [Description("Failed")]
    Failed = 2,

    [Description("Canceled")]
    Canceled = 3,
}
