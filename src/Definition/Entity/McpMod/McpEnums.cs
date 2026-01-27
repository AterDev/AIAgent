namespace Entity.McpMod;

using System.ComponentModel;

public enum McpToolType
{
    [Description("Builtin")]
    Builtin = 0,

    [Description("External")]
    External = 1,

    [Description("Custom")]
    Custom = 2,
}

public enum ToolCallStatus
{
    [Description("Success")]
    Success = 0,

    [Description("Failed")]
    Failed = 1,

    [Description("Timeout")]
    Timeout = 2,

    [Description("Canceled")]
    Canceled = 3,
}
