namespace Entity.ModelMod;

using System.ComponentModel;

public enum ModelProviderType
{
    [Description("OpenAI compatible")]
    OpenAiCompatible = 0,

    [Description("Custom provider")]
    Custom = 1,
}

public enum InvocationStatus
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

public enum QuotaPeriodType
{
    [Description("Minute")]
    Minute = 0,

    [Description("Hour")]
    Hour = 1,

    [Description("Day")]
    Day = 2,

    [Description("Month")]
    Month = 3,
}
